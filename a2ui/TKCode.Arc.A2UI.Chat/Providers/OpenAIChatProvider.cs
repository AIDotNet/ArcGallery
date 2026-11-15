using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// OpenAI Chat Completions (and compatible endpoints) over HTTP with server-sent events. Tool calls are assembled from
/// their streamed fragments by index; a compatible endpoint's <c>reasoning_content</c> is shown as reasoning.
/// </summary>
public sealed class OpenAIChatProvider(ChatProviderConfig config, HttpClient? http = null) : HttpChatProvider(config, http)
{
    protected override void AddAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(Config.ApiKey))
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + Config.ApiKey);
        }
    }

    internal JsonObject BuildBody(ChatRequest request)
    {
        var messages = new JsonArray { (JsonNode?)(new JsonObject { ["role"] = "system", ["content"] = request.System }) };
        foreach (var message in request.Messages)
        {
            if (message.Role == ChatRole.User)
            {
                foreach (var result in message.Parts.OfType<ChatToolResultPart>())
                {
                    messages.Add((JsonNode?)(new JsonObject { ["role"] = "tool", ["tool_call_id"] = result.CallId, ["content"] = result.Content }));
                }

                var text = string.Concat(message.Parts.OfType<ChatTextPart>().Select(t => t.Text));
                if (text.Length > 0)
                {
                    messages.Add((JsonNode?)(new JsonObject { ["role"] = "user", ["content"] = text }));
                }

                continue;
            }

            var assistant = new JsonObject { ["role"] = "assistant" };
            var content = string.Concat(message.Parts.OfType<ChatTextPart>().Select(t => t.Text));
            assistant["content"] = content.Length > 0 ? content : null;
            var calls = message.Parts.OfType<ChatToolCallPart>().ToList();
            if (calls.Count > 0)
            {
                assistant["tool_calls"] = Array(calls.Select(c => (JsonNode?)new JsonObject
                {
                    ["id"] = c.Id,
                    ["type"] = "function",
                    ["function"] = new JsonObject { ["name"] = c.Name, ["arguments"] = c.ArgumentsJson }
                }));
            }

            messages.Add((JsonNode?)(assistant));
        }

        var body = new JsonObject
        {
            ["model"] = request.Model,
            ["messages"] = messages,
            ["stream"] = true,
            ["stream_options"] = new JsonObject { ["include_usage"] = true }
        };
        body["max_completion_tokens"] = Config.MaxOutputTokens;

        if (!string.IsNullOrWhiteSpace(Config.Effort))
        {
            body["reasoning_effort"] = Config.Effort;
        }

        if (request.Tools.Count > 0)
        {
            body["tools"] = Array(request.Tools.Select(t => (JsonNode?)new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = t.Name, ["description"] = t.Description, ["parameters"] = t.InputSchema.DeepClone() }
            }));
            body["tool_choice"] = "auto";
        }

        return body;
    }

    public override async IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = BuildBody(request);
        using var response = await SendAsync(() => NewRequest(HttpMethod.Post, "/chat/completions", body), cancellationToken).ConfigureAwait(false);
        var calls = new SortedDictionary<int, (string Id, string Name, StringBuilder Args)>();
        var reasoning = new StringBuilder();
        var stop = ChatStopReason.EndTurn;
        await foreach (var sse in ReadSseAsync(response, cancellationToken).ConfigureAwait(false))
        {
            if (sse.Data == "[DONE]")
            {
                break;
            }

            if (ParseObject(sse.Data) is not { } data)
            {
                continue;
            }

            if (data["error"] is JsonObject error)
            {
                throw new ChatProviderException(A2UIJson.AsString(error["message"]) ?? "The provider reported an error.", type: A2UIJson.AsString(error["type"]));
            }

            if (data["usage"] is JsonObject usage && A2UIJson.TryGetDouble(usage["prompt_tokens"], out var input)
                && A2UIJson.TryGetDouble(usage["completion_tokens"], out var output))
            {
                yield return new ChatUsage((long)input, (long)output);
            }

            if (data["choices"] is not JsonArray { Count: > 0 } choices || choices[0] is not JsonObject choice)
            {
                continue;
            }

            if (choice["delta"] is JsonObject delta)
            {
                if (A2UIJson.AsString(delta["reasoning_content"]) is { Length: > 0 } thinking)
                {
                    reasoning.Append(thinking);
                    yield return new ChatReasoningDelta(thinking);
                }

                if (A2UIJson.AsString(delta["content"]) is { Length: > 0 } text)
                {
                    yield return new ChatTextDelta(text);
                }

                if (delta["tool_calls"] is JsonArray toolCalls)
                {
                    foreach (var tc in toolCalls.OfType<JsonObject>())
                    {
                        A2UIJson.TryGetDouble(tc["index"], out var idx);
                        var index = (int)idx;
                        if (!calls.TryGetValue(index, out var entry))
                        {
                            entry = (A2UIJson.AsString(tc["id"]) ?? $"call_{index}", A2UIJson.AsString(tc["function"]?["name"]) ?? string.Empty, new StringBuilder());
                            calls[index] = entry;
                            yield return new ChatToolCallStarted(entry.Id, entry.Name);
                        }
                        else if (A2UIJson.AsString(tc["function"]?["name"]) is { Length: > 0 } lateName && entry.Name.Length == 0)
                        {
                            calls[index] = entry = (entry.Id, lateName, entry.Args);
                        }

                        if (A2UIJson.AsString(tc["function"]?["arguments"]) is { } args)
                        {
                            entry.Args.Append(args);
                        }
                    }
                }
            }

            if (A2UIJson.AsString(choice["finish_reason"]) is { } finish)
            {
                stop = finish switch
                {
                    "tool_calls" or "function_call" => ChatStopReason.ToolUse,
                    "length" => ChatStopReason.MaxTokens,
                    "content_filter" => ChatStopReason.Refusal,
                    _ => ChatStopReason.EndTurn
                };
            }
        }

        if (reasoning.Length > 0)
        {
            yield return new ChatReasoningDone(new ChatReasoningPart(reasoning.ToString(), ChatProtocol.OpenAIChatCompletions, null));
        }

        foreach (var (_, call) in calls)
        {
            yield return new ChatToolCallDone(new ChatToolCallPart(call.Id, call.Name, call.Args.Length == 0 ? "{}" : call.Args.ToString()));
        }

        if (calls.Count > 0 && stop == ChatStopReason.EndTurn)
        {
            stop = ChatStopReason.ToolUse;
        }

        yield return new ChatCompleted(stop);
    }

    public override async Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken)
        => await ListOpenAIModelsAsync(this, cancellationToken).ConfigureAwait(false);

    internal static async Task<IReadOnlyList<ChatModelInfo>> ListOpenAIModelsAsync(HttpChatProvider provider, CancellationToken cancellationToken)
    {
        using var response = await provider.SendForList(cancellationToken).ConfigureAwait(false);
        var json = ParseObject(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        return (json?["data"] as JsonArray ?? new JsonArray())
            .OfType<JsonObject>()
            .Select(m => new ChatModelInfo(A2UIJson.AsString(m["id"]) ?? string.Empty, null))
            .Where(m => m.Id.Length > 0)
            .OrderBy(m => m.Id, StringComparer.Ordinal)
            .ToList();
    }
}
