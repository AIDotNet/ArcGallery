using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>Shared HTTP plumbing: URLs and proxy, headers, browser streaming, retries, error mapping, SSE.</summary>
public abstract class HttpChatProvider : IChatProvider
{
    private static readonly HttpRequestOptionsKey<bool> s_browserStreaming = new("WebAssemblyEnableStreamingResponse");
    private static readonly Lazy<HttpClient> s_shared = new(() => new HttpClient { Timeout = Timeout.InfiniteTimeSpan });

    protected HttpChatProvider(ChatProviderConfig config, HttpClient? http = null)
    {
        Config = config;
        Http = http ?? s_shared.Value;
    }

    protected ChatProviderConfig Config { get; }

    protected HttpClient Http { get; }

    /// <summary>The first byte must arrive within this time.</summary>
    public static TimeSpan FirstByteTimeout { get; set; } = TimeSpan.FromSeconds(120);

    public abstract IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken);

    protected abstract void AddAuth(HttpRequestMessage request);

    protected Uri Url(string path)
    {
        var baseUrl = Config.BaseUrl.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(Config.ProxyUrl))
        {
            return new Uri(Config.ProxyUrl.TrimEnd('/') + path);
        }

        return new Uri(baseUrl + path);
    }

    protected HttpRequestMessage NewRequest(HttpMethod method, string path, JsonObject? body)
    {
        var request = new HttpRequestMessage(method, Url(path));
        if (!string.IsNullOrWhiteSpace(Config.ProxyUrl))
        {
            request.Headers.TryAddWithoutValidation("X-Target-Base-Url", Config.BaseUrl.TrimEnd('/'));
        }

        AddAuth(request);
        foreach (var (key, value) in Config.Headers)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }

        if (body is not null)
        {
            request.Content = new StringContent(A2UIJson.ToJson(body), Encoding.UTF8, "application/json");
        }

        // In a browser, read the body as it arrives instead of after it has completed.
        request.Options.Set(s_browserStreaming, true);
        return request;
    }

    /// <summary>Sends a request; retries 429 / 5xx / network failures twice; throws <see cref="ChatProviderException"/>.</summary>
    protected async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> create, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var firstByte = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            firstByte.CancelAfter(FirstByteTimeout);
            HttpResponseMessage response;
            try
            {
                response = await Http.SendAsync(create(), HttpCompletionOption.ResponseHeadersRead, firstByte.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw new ChatProviderException("The provider did not answer in time.", retryable: true);
            }
            catch (HttpRequestException ex)
            {
                if (attempt < 2)
                {
                    await Task.Delay(Backoff(attempt, null), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw new ChatProviderException(
                    OperatingSystem.IsBrowser()
                        ? "The request was blocked before it reached the provider. The endpoint may not allow requests from a browser (CORS), or the network is down. Use a CORS-enabled endpoint or a proxy URL."
                        : "Could not reach the provider: " + ex.Message,
                    retryable: true, isCors: OperatingSystem.IsBrowser());
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var status = (int)response.StatusCode;
            var retryable = status == 429 || status >= 500;
            if (retryable && attempt < 2)
            {
                var wait = Backoff(attempt, response.Headers.RetryAfter);
                response.Dispose();
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                continue;
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            response.Dispose();
            throw ParseError(status, text, retryable);
        }
    }

    private static TimeSpan Backoff(int attempt, RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta is { } delta && delta < TimeSpan.FromSeconds(30))
        {
            return delta;
        }

        return TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt));
    }

    private static ChatProviderException ParseError(int status, string body, bool retryable)
    {
        string? message = null, type = null;
        try
        {
            if (A2UIJson.Parse(body) is JsonObject json)
            {
                var error = json["error"] as JsonObject;
                message = A2UIJson.AsString(error?["message"]) ?? A2UIJson.AsString(json["message"]);
                type = A2UIJson.AsString(error?["type"]) ?? A2UIJson.AsString(error?["code"]);
            }
        }
        catch (A2UIException)
        {
        }

        message ??= string.IsNullOrWhiteSpace(body) ? $"HTTP {status}" : body.Length > 300 ? body[..300] : body;
        var hint = status switch
        {
            401 or 403 => " Check the API key.",
            404 => " Check the base URL and the model name.",
            429 => " The provider is rate limiting; wait a moment.",
            _ => string.Empty
        };
        return new ChatProviderException($"{status}: {message}{hint}", status, type, retryable);
    }

    /// <summary>The SSE events of a response body.</summary>
    protected static async IAsyncEnumerable<A2UISseEvent> ReadSseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var evt in A2UISse.ReadAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            yield return evt;
        }
    }

    protected static JsonObject? ParseObject(string data)
    {
        try
        {
            return A2UIJson.Parse(data) as JsonObject;
        }
        catch (A2UIException)
        {
            return null;
        }
    }

    /// <summary>Parses tool arguments; an empty or broken stream gives an empty object (the tool then reports the problem).</summary>
    protected static JsonObject ParseArguments(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        return ParseObject(json) ?? new JsonObject { ["_invalid"] = json };
    }

    protected static JsonArray Array(IEnumerable<JsonNode?> items)
    {
        var array = new JsonArray();
        foreach (var item in items)
        {
            array.Add(item);
        }

        return array;
    }

    /// <summary>Creates the provider for a configuration.</summary>
    public static IChatProvider Create(ChatProviderConfig config, HttpClient? http = null) => config.Protocol switch
    {
        ChatProtocol.AnthropicMessages => new AnthropicProvider(config, http),
        ChatProtocol.OpenAIResponses => new OpenAIResponsesProvider(config, http),
        _ => new OpenAIChatProvider(config, http)
    };

    internal Task<HttpResponseMessage> SendForList(CancellationToken cancellationToken)
        => SendAsync(() => NewRequest(HttpMethod.Get, "/models", null), cancellationToken);
}
