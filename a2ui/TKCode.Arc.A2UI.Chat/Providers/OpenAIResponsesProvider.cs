using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The OpenAI Responses API over HTTP with server-sent events. Nothing is stored on the server (<c>store: false</c>),
/// so every request carries the whole history; encrypted reasoning items are kept locally and sent back.
/// </summary>
public sealed class OpenAIResponsesProvider(ChatProviderConfig config, HttpClient? http = null) : HttpChatProvider(config, http)
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
        var input = new JsonArray();
        foreach (var message in request.Messages)
        {
            if (message.Role == ChatRole.User)
            {
                foreach (var result in message.Parts.OfType<ChatToolResultPart>())
                {
                    input.Add((JsonNode?)(new JsonObject { ["type"] = "function_call_output", ["call_id"] = result.CallId, ["output"] = result.Content }));
                }

                var text = string.Concat(message.Parts.OfType<ChatTextPart>().Select(t => t.Text));
                if (text.Length > 0)
                {
                    input.Add((JsonNode?)(new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = new JsonArray { (JsonNode?)(new JsonObject { ["type"] = "input_text", ["text"] = text }) }
                    }));
                }

                continue;
            }

            foreach (var part in message.Parts)
            {
                switch (part)
                {
                    case ChatReasoningPart { Protocol: ChatProtocol.OpenAIResponses, Raw: { } raw }:
                        input.Add(raw.DeepClone());
                        break;
                    case ChatTextPart { Text.Length: > 0 } text:
                        input.Add((JsonNode?)(new JsonObject
                        {
                            ["role"] = "assistant",
                            ["content"] = new JsonArray { (JsonNode?)(new JsonObject { ["type"] = "output_text", ["text"] = text.Text }) }
                        }));
                        break;
                    case ChatToolCallPart call:
                        input.Add((JsonNode?)(new JsonObject { ["type"] = "function_call", ["call_id"] = call.Id, ["name"] = call.Name, ["arguments"] = call.ArgumentsJson }));
                        break;
                }
            }
        }

        var body = new JsonObject
        {
            ["model"] = request.Model,
            ["instructions"] = request.System,
            ["input"] = input,
            ["stream"] = true,
            ["store"] = false,
            ["include"] = new JsonArray { (JsonNode?)("reasoning.encrypted_content") }
        };
        body["max_output_tokens"] = Config.MaxOutputTokens;

        if (!string.IsNullOrWhiteSpace(Config.Effort))
        {
            body["reasoning"] = new JsonObject { ["effort"] = Config.Effort, ["summary"] = "auto" };
        }

        if (request.Tools.Count > 0)
        {
            body["tools"] = Array(request.Tools.Select(t => (JsonNode?)new JsonObject
            {
                ["type"] = "function",
                ["name"] = t.Name,
                ["description"] = t.Description,
                ["parameters"] = t.InputSchema.DeepClone()
            }));
            body["tool_choice"] = "auto";
        }

        return body;
    }

    public override async IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = BuildBody(request);
        using var response = await SendAsync(() => NewRequest(HttpMethod.Post, "/responses", body), cancellationToken).ConfigureAwait(false);
        var stop = ChatStopReason.EndTurn;
        string? detail = null;
        var sawToolCall = false;
        await foreach (var sse in ReadSseAsync(response, cancellationToken).ConfigureAwait(false))
        {
            if (ParseObject(sse.Data) is not { } data)
            {
                continue;
            }

            switch (A2UIJson.AsString(data["type"]) ?? sse.Event)
            {
                case "response.output_text.delta" when A2UIJson.AsString(data["delta"]) is { } text:
                    yield return new ChatTextDelta(text);
                    break;
                case "response.reasoning_summary_text.delta" when A2UIJson.AsString(data["delta"]) is { } summary:
                    yield return new ChatReasoningDelta(summary);
                    break;
                case "response.output_item.added" when data["item"] is JsonObject { } added && A2UIJson.AsString(added["type"]) == "function_call":
                    yield return new ChatToolCallStarted(A2UIJson.AsString(added["call_id"]) ?? string.Empty, A2UIJson.AsString(added["name"]) ?? string.Empty);
                    break;
                case "response.output_item.done" when data["item"] is JsonObject item:
                    switch (A2UIJson.AsString(item["type"]))
                    {
                        case "function_call":
                            sawToolCall = true;
                            yield return new ChatToolCallDone(new ChatToolCallPart(
                                A2UIJson.AsString(item["call_id"]) ?? string.Empty,
                                A2UIJson.AsString(item["name"]) ?? string.Empty,
                                A2UIJson.AsString(item["arguments"]) is { Length: > 0 } args ? args : "{}"));
                            break;
                        case "reasoning":
                            var summaryText = new StringBuilder();
                            foreach (var s in (item["summary"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                            {
                                summaryText.Append(A2UIJson.AsString(s["text"]));
                            }

                            // Kept only when it can be sent back (it carries encrypted content).
                            var raw = item.ContainsKey("encrypted_content") ? item.DeepClone() : null;
                            yield return new ChatReasoningDone(new ChatReasoningPart(summaryText.ToString(), ChatProtocol.OpenAIResponses, raw));
                            break;
                    }

                    break;
                case "response.completed" or "response.incomplete":
                    if (data["response"]?["usage"] is JsonObject usage && A2UIJson.TryGetDouble(usage["input_tokens"], out var i)
                        && A2UIJson.TryGetDouble(usage["output_tokens"], out var o))
                    {
                        yield return new ChatUsage((long)i, (long)o);
                    }

                    if (A2UIJson.AsString(data["type"]) == "response.incomplete")
                    {
                        var reason = A2UIJson.AsString(data["response"]?["incomplete_details"]?["reason"]);
                        stop = reason == "content_filter" ? ChatStopReason.Refusal : ChatStopReason.MaxTokens;
                        detail = reason;
                    }

                    break;
                case "response.failed" or "error":
                {
                    var error = data["response"]?["error"] ?? data["error"] ?? data;
                    throw new ChatProviderException(A2UIJson.AsString(error["message"]) ?? "The provider reported an error.",
                                                    type: A2UIJson.AsString(error["code"]) ?? A2UIJson.AsString(error["type"]));
                }
            }
        }

        if (sawToolCall && stop == ChatStopReason.EndTurn)
        {
            stop = ChatStopReason.ToolUse;
        }

        yield return new ChatCompleted(stop, detail);
    }

    public override async Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken)
        => await OpenAIChatProvider.ListOpenAIModelsAsync(this, cancellationToken).ConfigureAwait(false);
}
