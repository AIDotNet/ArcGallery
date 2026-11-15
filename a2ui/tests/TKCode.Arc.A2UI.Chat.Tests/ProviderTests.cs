using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat.Tests;

public sealed class ProviderTests
{
    private static async Task<List<ChatStreamEvent>> Collect(IChatProvider provider, ChatRequest request)
    {
        var list = new List<ChatStreamEvent>();
        await foreach (var evt in provider.StreamAsync(request, CancellationToken.None))
        {
            list.Add(evt);
        }

        return list;
    }

    private static ChatRequest Request(params ChatMessage[] messages) => new()
    {
        Model = "m",
        System = "sys",
        Messages = messages.Length > 0 ? messages : [ChatMessage.User("hi")],
        Tools = [ChatTools.All[0]]
    };

    [Fact]
    public async Task Anthropic_stream_gives_thinking_text_and_tool_calls()
    {
        const string sse = """
            event: message_start
            data: {"type":"message_start","message":{"usage":{"input_tokens":12}}}

            event: content_block_start
            data: {"type":"content_block_start","index":0,"content_block":{"type":"thinking","thinking":""}}

            event: content_block_delta
            data: {"type":"content_block_delta","index":0,"delta":{"type":"thinking_delta","thinking":"Plan."}}

            event: content_block_delta
            data: {"type":"content_block_delta","index":0,"delta":{"type":"signature_delta","signature":"sig"}}

            event: content_block_stop
            data: {"type":"content_block_stop","index":0}

            event: content_block_start
            data: {"type":"content_block_start","index":1,"content_block":{"type":"text","text":""}}

            event: content_block_delta
            data: {"type":"content_block_delta","index":1,"delta":{"type":"text_delta","text":"Hello"}}

            event: content_block_stop
            data: {"type":"content_block_stop","index":1}

            event: content_block_start
            data: {"type":"content_block_start","index":2,"content_block":{"type":"tool_use","id":"tu_1","name":"get_current_time","input":{}}}

            event: content_block_delta
            data: {"type":"content_block_delta","index":2,"delta":{"type":"input_json_delta","partial_json":"{}"}}

            event: content_block_stop
            data: {"type":"content_block_stop","index":2}

            event: message_delta
            data: {"type":"message_delta","delta":{"stop_reason":"tool_use"},"usage":{"output_tokens":30}}

            event: message_stop
            data: {"type":"message_stop"}

            """;
        var handler = new FixtureHandler(sse);
        var config = ChatProviderConfig.AnthropicPreset();
        config.ApiKey = "k";
        var events = await Collect(new AnthropicProvider(config, new HttpClient(handler)), Request());

        var reasoning = events.OfType<ChatReasoningDone>().Single().Part;
        Assert.Equal("Plan.", reasoning.Summary);
        Assert.Equal("sig", A2UIJson.AsString(reasoning.Raw!["signature"]));
        Assert.Equal("Hello", string.Concat(events.OfType<ChatTextDelta>().Select(t => t.Text)));
        Assert.Equal("get_current_time", events.OfType<ChatToolCallDone>().Single().Call.Name);
        Assert.Equal(ChatStopReason.ToolUse, events.OfType<ChatCompleted>().Single().Reason);

        var (request, body) = handler.Requests.Single();
        Assert.Equal("https://api.anthropic.com/v1/messages", request.RequestUri!.ToString());
        Assert.Equal("k", request.Headers.GetValues("x-api-key").Single());
        var json = (JsonObject)A2UIJson.Parse(body)!;
        Assert.Equal("adaptive", A2UIJson.AsString(json["thinking"]!["type"]));
        Assert.Equal("medium", A2UIJson.AsString(json["output_config"]!["effort"]));
        Assert.Equal("ephemeral", A2UIJson.AsString(json["system"]![0]!["cache_control"]!["type"]));
        Assert.Equal(true, A2UIJson.AsBoolean(json["tools"]![0]!["eager_input_streaming"]));
        Assert.Equal("default", A2UIJson.AsString(json["fallbacks"]));
        Assert.False(json.ContainsKey("temperature"));
    }

    [Fact]
    public void Every_protocol_asks_for_32000_output_tokens_by_default()
    {
        var request = Request(ChatMessage.User("hi"));
        Assert.Equal(32000, new AnthropicProvider(ChatProviderConfig.AnthropicPreset()).BuildBody(request)["max_tokens"]!.GetValue<int>());
        Assert.Equal(32000, new OpenAIChatProvider(ChatProviderConfig.OpenAIChatPreset()).BuildBody(request)["max_completion_tokens"]!.GetValue<int>());
        Assert.Equal(32000, new OpenAIResponsesProvider(ChatProviderConfig.OpenAIResponsesPreset()).BuildBody(request)["max_output_tokens"]!.GetValue<int>());

        // A saved provider from before the setting existed gets the default; a saved value is kept.
        var old = ChatProviderConfig.OpenAIChatPreset().ToJson(includeKey: false);
        old.Remove("maxOutputTokens");
        Assert.Equal(32000, ChatProviderConfig.FromJson(old).MaxOutputTokens);
        var custom = ChatProviderConfig.AnthropicPreset();
        custom.MaxOutputTokens = 8000;
        Assert.Equal(8000, ChatProviderConfig.FromJson(custom.ToJson(includeKey: false)).MaxOutputTokens);
    }

    [Fact]
    public void Anthropic_history_returns_thinking_blocks_unchanged()
    {
        var raw = new JsonObject { ["type"] = "thinking", ["thinking"] = "x", ["signature"] = "s" };
        var provider = new AnthropicProvider(ChatProviderConfig.AnthropicPreset());
        var body = provider.BuildBody(Request(
            ChatMessage.User("hi"),
            new ChatMessage(ChatRole.Assistant, [new ChatReasoningPart("x", ChatProtocol.AnthropicMessages, raw), new ChatTextPart("a"), new ChatToolCallPart("t1", "calculate", "{\"expression\":\"1+1\"}")]),
            new ChatMessage(ChatRole.User, [new ChatToolResultPart("t1", "calculate", "2", false)])));
        var assistant = body["messages"]![1]!["content"]!.AsArray();
        Assert.True(A2UIJson.DeepEquals(raw, assistant[0]));
        Assert.Equal("tool_use", A2UIJson.AsString(assistant[2]!["type"]));
        Assert.Equal("tool_result", A2UIJson.AsString(body["messages"]![2]!["content"]![0]!["type"]));
    }

    [Fact]
    public async Task OpenAI_chat_assembles_tool_call_fragments()
    {
        const string sse = """
            data: {"choices":[{"delta":{"content":"Hi "}}]}

            data: {"choices":[{"delta":{"tool_calls":[{"index":0,"id":"call_1","function":{"name":"calculate","arguments":"{\"expr"}}]}}]}

            data: {"choices":[{"delta":{"tool_calls":[{"index":0,"function":{"arguments":"ession\":\"2*3\"}"}}]}}]}

            data: {"choices":[{"delta":{},"finish_reason":"tool_calls"}]}

            data: {"choices":[],"usage":{"prompt_tokens":5,"completion_tokens":7}}

            data: [DONE]

            """;
        var handler = new FixtureHandler(sse);
        var config = ChatProviderConfig.OpenAIChatPreset();
        config.ApiKey = "k";
        var events = await Collect(new OpenAIChatProvider(config, new HttpClient(handler)), Request());
        var call = events.OfType<ChatToolCallDone>().Single().Call;
        Assert.Equal("{\"expression\":\"2*3\"}", call.ArgumentsJson);
        Assert.Equal(ChatStopReason.ToolUse, events.OfType<ChatCompleted>().Single().Reason);
        Assert.Equal(7, events.OfType<ChatUsage>().Single().OutputTokens);
        Assert.Equal("Bearer k", handler.Requests.Single().Request.Headers.GetValues("Authorization").Single());
        Assert.EndsWith("/chat/completions", handler.Requests.Single().Request.RequestUri!.ToString());
    }

    [Fact]
    public async Task OpenAI_responses_reads_text_reasoning_and_function_calls()
    {
        const string sse = """
            event: response.reasoning_summary_text.delta
            data: {"type":"response.reasoning_summary_text.delta","delta":"Thinking"}

            event: response.output_item.done
            data: {"type":"response.output_item.done","item":{"type":"reasoning","id":"rs_1","summary":[{"type":"summary_text","text":"Thinking"}],"encrypted_content":"enc"}}

            event: response.output_text.delta
            data: {"type":"response.output_text.delta","delta":"Hey"}

            event: response.output_item.added
            data: {"type":"response.output_item.added","item":{"type":"function_call","call_id":"c1","name":"get_weather"}}

            event: response.output_item.done
            data: {"type":"response.output_item.done","item":{"type":"function_call","call_id":"c1","name":"get_weather","arguments":"{\"city\":\"Oslo\"}"}}

            event: response.completed
            data: {"type":"response.completed","response":{"usage":{"input_tokens":3,"output_tokens":4}}}

            """;
        var handler = new FixtureHandler(sse);
        var events = await Collect(new OpenAIResponsesProvider(ChatProviderConfig.OpenAIResponsesPreset(), new HttpClient(handler)), Request());
        Assert.Equal("Hey", string.Concat(events.OfType<ChatTextDelta>().Select(t => t.Text)));
        Assert.Equal("enc", A2UIJson.AsString(events.OfType<ChatReasoningDone>().Single().Part.Raw!["encrypted_content"]));
        Assert.Equal("get_weather", events.OfType<ChatToolCallDone>().Single().Call.Name);
        Assert.Equal(ChatStopReason.ToolUse, events.OfType<ChatCompleted>().Single().Reason);
        var json = (JsonObject)A2UIJson.Parse(handler.Requests.Single().Body)!;
        Assert.Equal(false, A2UIJson.AsBoolean(json["store"]));
        Assert.Equal("sys", A2UIJson.AsString(json["instructions"]));
    }

    [Fact]
    public async Task Http_errors_become_readable_messages()
    {
        var handler = new ErrorHandler();
        var config = ChatProviderConfig.OpenAIChatPreset();
        var error = await Assert.ThrowsAsync<ChatProviderException>(() => Collect(new OpenAIChatProvider(config, new HttpClient(handler)), Request()));
        Assert.Equal(401, error.Status);
        Assert.Contains("Check the API key", error.Message);
    }

    private sealed class ErrorHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("""{"error":{"message":"Incorrect API key","type":"invalid_request_error"}}""")
            });
    }
}
