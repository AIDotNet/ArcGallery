using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>A provider the user configured: protocol, endpoint, key, models and options.</summary>
public sealed class ChatProviderConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = "Provider";

    public ChatProtocol Protocol { get; set; }

    /// <summary>The API base, for example <c>https://api.anthropic.com</c> or <c>https://api.openai.com/v1</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The API key. Kept in memory unless <see cref="RememberKey"/>.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>True to store the key on this device.</summary>
    public bool RememberKey { get; set; }

    /// <summary>Models the user added or fetched.</summary>
    public List<string> Models { get; set; } = new();

    public string? DefaultModel { get; set; }

    /// <summary>Extra request headers (gateways, routers).</summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>An optional proxy: requests go to <c>ProxyUrl + path</c> with an <c>X-Target-Base-Url</c> header.</summary>
    public string? ProxyUrl { get; set; }

    /// <summary>Effort (<c>low</c>, <c>medium</c>, <c>high</c>); empty sends none.</summary>
    public string Effort { get; set; } = string.Empty;

    /// <summary>Anthropic: adaptive thinking with summarized display.</summary>
    public bool Thinking { get; set; }

    /// <summary>Anthropic: server-side model fallback on refusals (official endpoint only by default).</summary>
    public bool ServerFallback { get; set; }

    /// <summary>The output limit a new provider starts with.</summary>
    public const int DefaultMaxOutputTokens = 32000;

    /// <summary>The output token limit sent with every request (all three protocols).</summary>
    public int MaxOutputTokens { get; set; } = DefaultMaxOutputTokens;

    /// <summary>True for a provider built in for the examples (no network).</summary>
    public bool IsReplay { get; set; }

    public bool IsOfficialAnthropic => Protocol == ChatProtocol.AnthropicMessages
                                       && Uri.TryCreate(BaseUrl, UriKind.Absolute, out var u) && u.Host == "api.anthropic.com";

    public static ChatProviderConfig AnthropicPreset() => new()
    {
        Name = "Anthropic",
        Protocol = ChatProtocol.AnthropicMessages,
        BaseUrl = "https://api.anthropic.com",
        Models = ["claude-opus-5-5", "claude-sonnet-5-5", "claude-haiku-5-5"],
        DefaultModel = "claude-opus-5-5",
        Effort = "medium",
        Thinking = true,
        ServerFallback = true
    };

    public static ChatProviderConfig OpenAIResponsesPreset() => new()
    {
        Name = "OpenAI",
        Protocol = ChatProtocol.OpenAIResponses,
        BaseUrl = "https://api.openai.com/v1"
    };

    public static ChatProviderConfig OpenAIChatPreset() => new()
    {
        Name = "OpenAI (Chat Completions)",
        Protocol = ChatProtocol.OpenAIChatCompletions,
        BaseUrl = "https://api.openai.com/v1"
    };

    /// <summary>Base URL templates for common OpenAI-compatible endpoints (filled in, not verified).</summary>
    public static IReadOnlyList<(string Name, string BaseUrl)> CompatibleTemplates { get; } =
    [
        ("OpenRouter", "https://openrouter.ai/api/v1"),
        ("DeepSeek", "https://api.deepseek.com"),
        ("Ollama (this computer)", "http://localhost:11434/v1")
    ];

    public JsonObject ToJson(bool includeKey)
    {
        var headers = new JsonObject();
        foreach (var (key, value) in Headers)
        {
            headers[key] = value;
        }

        var models = new JsonArray();
        foreach (var model in Models)
        {
            models.Add((JsonNode?)(model));
        }

        var json = new JsonObject
        {
            ["id"] = Id,
            ["name"] = Name,
            ["protocol"] = Protocol.ToString(),
            ["baseUrl"] = BaseUrl,
            ["rememberKey"] = RememberKey,
            ["models"] = models,
            ["defaultModel"] = DefaultModel,
            ["headers"] = headers,
            ["proxyUrl"] = ProxyUrl,
            ["effort"] = Effort,
            ["thinking"] = Thinking,
            ["serverFallback"] = ServerFallback,
            ["maxOutputTokens"] = MaxOutputTokens
        };
        if (includeKey && RememberKey)
        {
            json["apiKey"] = ApiKey;
        }

        return json;
    }

    public static ChatProviderConfig FromJson(JsonObject json)
    {
        var config = new ChatProviderConfig
        {
            Id = A2UIJson.AsString(json["id"]) ?? Guid.NewGuid().ToString("N"),
            Name = A2UIJson.AsString(json["name"]) ?? "Provider",
            Protocol = Enum.TryParse<ChatProtocol>(A2UIJson.AsString(json["protocol"]), out var p) ? p : ChatProtocol.OpenAIChatCompletions,
            BaseUrl = A2UIJson.AsString(json["baseUrl"]) ?? string.Empty,
            ApiKey = A2UIJson.AsString(json["apiKey"]) ?? string.Empty,
            RememberKey = A2UIJson.AsBoolean(json["rememberKey"]) == true,
            DefaultModel = A2UIJson.AsString(json["defaultModel"]),
            ProxyUrl = A2UIJson.AsString(json["proxyUrl"]),
            Effort = A2UIJson.AsString(json["effort"]) ?? string.Empty,
            Thinking = A2UIJson.AsBoolean(json["thinking"]) == true,
            ServerFallback = A2UIJson.AsBoolean(json["serverFallback"]) == true,
            MaxOutputTokens = A2UIJson.TryGetDouble(json["maxOutputTokens"], out var max) && max >= 1 ? (int)max : DefaultMaxOutputTokens
        };
        if (json["models"] is JsonArray models)
        {
            config.Models = models.Select(A2UIJson.AsString).Where(m => m is not null).Select(m => m!).ToList();
        }

        if (json["headers"] is JsonObject headers)
        {
            foreach (var (key, value) in headers)
            {
                if (A2UIJson.AsString(value) is { } v)
                {
                    config.Headers[key] = v;
                }
            }
        }

        return config;
    }

    public ChatProviderConfig Clone() => FromJson(ToJson(includeKey: false)).WithKey(ApiKey);

    private ChatProviderConfig WithKey(string key)
    {
        ApiKey = key;
        return this;
    }
}
