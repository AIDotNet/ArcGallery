using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The Anthropic Messages API over HTTP with server-sent events: adaptive thinking with summarized display, effort,
/// prompt caching on the system prompt, client tools with eager input streaming, refusal handling and (on the official
/// endpoint) server-side fallback. Thinking blocks are returned unchanged on later requests.
/// </summary>
public sealed class AnthropicProvider(ChatProviderConfig config, HttpClient? http = null) : HttpChatProvider(config, http)
{
    private const string Version = "2023-06-01";
    private const string FallbackBeta = "server-side-fallback-2026-07-01";

    protected override void AddAuth(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("x-api-key", Config.ApiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", Version);
        if (OperatingSystem.IsBrowser())
        {
            // Anthropic's switch for requests made directly from a browser (CORS).
            request.Headers.TryAddWithoutValidation("anthropic-dangerous-direct-browser-access", "true");
        }
    }

    internal JsonObject BuildBody(ChatRequest request)
    {
        var messages = new JsonArray();
        foreach (var message in request.Messages)
        {
            var content = new JsonArray();
            foreach (var part in message.Parts)
            {
                switch (part)
                {
                    case ChatTextPart { Text.Length: > 0 } text:
                        content.Add((JsonNode?)(new JsonObject { ["type"] = "text", ["text"] = text.Text }));
                        break;
                    case ChatReasoningPart { Protocol: ChatProtocol.AnthropicMessages, Raw: { } raw }:
                        content.Add(raw.DeepClone());
                        break;
                    case ChatToolCallPart call:
                        content.Add((JsonNode?)(new JsonObject { ["type"] = "tool_use", ["id"] = call.Id, ["name"] = call.Name, ["input"] = ParseArguments(call.ArgumentsJson) }));
                        break;
                    case ChatToolResultPart result:
                        content.Add((JsonNode?)(new JsonObject
                        {
                            ["type"] = "tool_result",
                            ["tool_use_id"] = result.CallId,
                            ["content"] = result.Content,
                            ["is_error"] = result.IsError
                        }));
                        break;
                }
            }

            if (content.Count > 0)
            {
                messages.Add((JsonNode?)(new JsonObject { ["role"] = message.Role == ChatRole.User ? "user" : "assistant", ["content"] = content }));
            }
        }

        var body = new JsonObject
        {
            ["model"] = request.Model,
            ["max_tokens"] = Config.MaxOutputTokens,
            ["stream"] = true,
            ["system"] = new JsonArray
            {
                (JsonNode?)(new JsonObject { ["type"] = "text", ["text"] = request.System, ["cache_control"] = new JsonObject { ["type"] = "ephemeral" } })
            },
            ["messages"] = messages
        };
        if (Config.Thinking)
        {
            body["thinking"] = new JsonObject { ["type"] = "adaptive", ["display"] = "summarized" };
        }

        if (!string.IsNullOrWhiteSpace(Config.Effort))
        {
            body["output_config"] = new JsonObject { ["effort"] = Config.Effort };
        }

        if (request.Tools.Count > 0)
        {
            body["tools"] = Array(request.Tools.Select(t => (JsonNode?)new JsonObject
            {
                ["name"] = t.Name,
                ["description"] = t.Description,
                ["input_schema"] = t.InputSchema.DeepClone(),
                ["eager_input_streaming"] = true
            }));
            body["tool_choice"] = new JsonObject { ["type"] = "auto" };
        }

        if (Config.ServerFallback)
        {
            body["fallbacks"] = "default";
        }

        return body;
    }

    public override async IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = BuildBody(request);
        using var response = await SendAsync(() =>
        {
            var http = NewRequest(HttpMethod.Post, "/v1/messages", body);
            if (Config.ServerFallback)
            {
                http.Headers.TryAddWithoutValidation("anthropic-beta", FallbackBeta);
            }

            return http;
        }, cancellationToken).ConfigureAwait(false);

        var blocks = new Dictionary<int, Block>();
        var stop = ChatStopReason.EndTurn;
        string? detail = null;
        long input = 0, output = 0;
        await foreach (var sse in ReadSseAsync(response, cancellationToken).ConfigureAwait(false))
        {
            if (ParseObject(sse.Data) is not { } data)
            {
                continue;
            }

            switch (A2UIJson.AsString(data["type"]))
            {
                case "message_start":
                    if (data["message"]?["usage"] is JsonObject startUsage)
                    {
                        A2UIJson.TryGetDouble(startUsage["input_tokens"], out var i);
                        input = (long)i;
                    }

                    break;
                case "content_block_start":
                {
                    var index = (int)(data["index"]?.GetValue<double>() ?? 0);
                    var block = data["content_block"] as JsonObject ?? new JsonObject();
                    var b = new Block(A2UIJson.AsString(block["type"]) ?? "text", block);
                    blocks[index] = b;
                    if (b.Type == "tool_use")
                    {
                        b.Id = A2UIJson.AsString(block["id"]) ?? string.Empty;
                        b.Name = A2UIJson.AsString(block["name"]) ?? string.Empty;
                        yield return new ChatToolCallStarted(b.Id, b.Name);
                    }
                    else if (b.Type == "text" && A2UIJson.AsString(block["text"]) is { Length: > 0 } initial)
                    {
                        b.Text.Append(initial);
                        yield return new ChatTextDelta(initial);
                    }

                    break;
                }
                case "content_block_delta":
                {
                    var index = (int)(data["index"]?.GetValue<double>() ?? 0);
                    if (!blocks.TryGetValue(index, out var b) || data["delta"] is not JsonObject delta)
                    {
                        break;
                    }

                    switch (A2UIJson.AsString(delta["type"]))
                    {
                        case "text_delta" when A2UIJson.AsString(delta["text"]) is { } text:
                            b.Text.Append(text);
                            yield return new ChatTextDelta(text);
                            break;
                        case "thinking_delta" when A2UIJson.AsString(delta["thinking"]) is { } thinking:
                            b.Text.Append(thinking);
                            yield return new ChatReasoningDelta(thinking);
                            break;
                        case "signature_delta" when A2UIJson.AsString(delta["signature"]) is { } signature:
                            b.Signature.Append(signature);
                            break;
                        case "input_json_delta" when A2UIJson.AsString(delta["partial_json"]) is { } json:
                            b.Text.Append(json);
                            break;
                    }

                    break;
                }
                case "content_block_stop":
                {
                    var index = (int)(data["index"]?.GetValue<double>() ?? 0);
                    if (!blocks.TryGetValue(index, out var b))
                    {
                        break;
                    }

                    switch (b.Type)
                    {
                        case "thinking":
                            yield return new ChatReasoningDone(new ChatReasoningPart(b.Text.ToString(), ChatProtocol.AnthropicMessages, new JsonObject
                            {
                                ["type"] = "thinking",
                                ["thinking"] = b.Text.ToString(),
                                ["signature"] = b.Signature.ToString()
                            }));
                            break;
                        case "redacted_thinking":
                            yield return new ChatReasoningDone(new ChatReasoningPart(string.Empty, ChatProtocol.AnthropicMessages, b.Start.DeepClone()));
                            break;
                        case "tool_use":
                            yield return new ChatToolCallDone(new ChatToolCallPart(b.Id, b.Name, b.Text.Length == 0 ? "{}" : b.Text.ToString()));
                            break;
                    }

                    break;
                }
                case "message_delta":
                    if (data["delta"] is JsonObject md)
                    {
                        stop = A2UIJson.AsString(md["stop_reason"]) switch
                        {
                            "tool_use" => ChatStopReason.ToolUse,
                            "max_tokens" => ChatStopReason.MaxTokens,
                            "refusal" => ChatStopReason.Refusal,
                            "end_turn" or "stop_sequence" or null => ChatStopReason.EndTurn,
                            _ => ChatStopReason.Other
                        };
                        detail = A2UIJson.AsString(md["stop_details"]?["explanation"]) ?? A2UIJson.AsString(md["stop_details"]?["category"]);
                    }

                    if (data["usage"] is JsonObject usage && A2UIJson.TryGetDouble(usage["output_tokens"], out var o))
                    {
                        output = (long)o;
                    }

                    break;
                case "error":
                    throw new ChatProviderException(A2UIJson.AsString(data["error"]?["message"]) ?? "The provider reported an error.",
                                                    type: A2UIJson.AsString(data["error"]?["type"]),
                                                    retryable: A2UIJson.AsString(data["error"]?["type"]) == "overloaded_error");
                case "message_stop":
                    yield return new ChatUsage(input, output);
                    yield return new ChatCompleted(stop, detail);
                    yield break;
            }
        }

        yield return new ChatUsage(input, output);
        yield return new ChatCompleted(stop, detail);
    }

    public override async Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken)
    {
        using var response = await SendAsync(() => NewRequest(HttpMethod.Get, "/v1/models?limit=100", null), cancellationToken).ConfigureAwait(false);
        var json = ParseObject(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        return (json?["data"] as JsonArray ?? new JsonArray())
            .OfType<JsonObject>()
            .Select(m => new ChatModelInfo(A2UIJson.AsString(m["id"]) ?? string.Empty, A2UIJson.AsString(m["display_name"])))
            .Where(m => m.Id.Length > 0)
            .ToList();
    }

    private sealed class Block(string type, JsonObject start)
    {
        public string Type { get; } = type;
        public JsonObject Start { get; } = start;
        public StringBuilder Text { get; } = new();
        public StringBuilder Signature { get; } = new();
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
