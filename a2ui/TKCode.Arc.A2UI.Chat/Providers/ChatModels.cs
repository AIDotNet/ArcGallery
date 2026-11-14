using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>The wire protocol a provider speaks.</summary>
public enum ChatProtocol
{
    /// <summary>Anthropic Messages API (<c>POST /v1/messages</c>).</summary>
    AnthropicMessages,

    /// <summary>OpenAI Chat Completions (<c>POST /chat/completions</c>) and compatible endpoints.</summary>
    OpenAIChatCompletions,

    /// <summary>OpenAI Responses (<c>POST /responses</c>).</summary>
    OpenAIResponses
}

/// <summary>Who wrote a message.</summary>
public enum ChatRole
{
    User,
    Assistant
}

/// <summary>One part of a message, in the provider-neutral history.</summary>
public abstract record ChatPart;

/// <summary>Text (for the assistant, the raw text including A2UI fences).</summary>
public sealed record ChatTextPart(string Text) : ChatPart;

/// <summary>
/// Reasoning the model returned. <see cref="Raw"/> is the provider's own block (an Anthropic thinking block with its
/// signature, an OpenAI reasoning item with encrypted content); it is sent back unchanged to the same protocol.
/// </summary>
public sealed record ChatReasoningPart(string Summary, ChatProtocol Protocol, JsonNode? Raw) : ChatPart;

/// <summary>A tool call the model made.</summary>
public sealed record ChatToolCallPart(string Id, string Name, string ArgumentsJson) : ChatPart;

/// <summary>The result of a tool call, sent back to the model.</summary>
public sealed record ChatToolResultPart(string CallId, string Name, string Content, bool IsError) : ChatPart;

/// <summary>One message of the conversation sent to the model.</summary>
public sealed class ChatMessage(ChatRole role, IReadOnlyList<ChatPart> parts)
{
    public ChatRole Role { get; } = role;

    public IReadOnlyList<ChatPart> Parts { get; } = parts;

    public static ChatMessage User(string text) => new(ChatRole.User, [new ChatTextPart(text)]);
}

/// <summary>A tool the model may call: name, description and JSON Schema of its input.</summary>
public sealed record ChatTool(string Name, string Description, JsonObject InputSchema);

/// <summary>Everything one model request needs.</summary>
public sealed class ChatRequest
{
    public required string Model { get; init; }

    public required string System { get; init; }

    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    public IReadOnlyList<ChatTool> Tools { get; init; } = [];
}

/// <summary>Why the model stopped.</summary>
public enum ChatStopReason
{
    EndTurn,
    ToolUse,
    MaxTokens,
    Refusal,
    Cancelled,
    Other
}

/// <summary>An event of a streamed response.</summary>
public abstract record ChatStreamEvent;

/// <summary>More answer text.</summary>
public sealed record ChatTextDelta(string Text) : ChatStreamEvent;

/// <summary>More reasoning summary text.</summary>
public sealed record ChatReasoningDelta(string Text) : ChatStreamEvent;

/// <summary>A finished reasoning block (to keep in the history).</summary>
public sealed record ChatReasoningDone(ChatReasoningPart Part) : ChatStreamEvent;

/// <summary>A tool call started (its arguments are still streaming).</summary>
public sealed record ChatToolCallStarted(string Id, string Name) : ChatStreamEvent;

/// <summary>A complete tool call.</summary>
public sealed record ChatToolCallDone(ChatToolCallPart Call) : ChatStreamEvent;

/// <summary>Token usage, when the provider reports it.</summary>
public sealed record ChatUsage(long InputTokens, long OutputTokens) : ChatStreamEvent;

/// <summary>The response finished.</summary>
public sealed record ChatCompleted(ChatStopReason Reason, string? Detail = null) : ChatStreamEvent;

/// <summary>A provider error (HTTP status, error type, message).</summary>
public sealed class ChatProviderException(string message, int? status = null, string? type = null, bool retryable = false, bool isCors = false)
    : Exception(message)
{
    public int? Status { get; } = status;

    public string? ErrorType { get; } = type;

    public bool IsRetryable { get; } = retryable;

    /// <summary>True when the request was blocked before any HTTP status (in a browser: cross-origin or network).</summary>
    public bool IsNetworkOrCors { get; } = isCors;
}

/// <summary>A model offered by a provider.</summary>
public sealed record ChatModelInfo(string Id, string? DisplayName);

/// <summary>A provider: streams one response, lists models.</summary>
public interface IChatProvider
{
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken);
}
