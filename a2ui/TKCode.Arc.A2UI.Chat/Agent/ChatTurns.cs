using System.Collections.ObjectModel;
using System.Text;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>One entry of the visible conversation.</summary>
public abstract class ChatTurn;

/// <summary>What the user wrote, or an action they took on an interface.</summary>
public sealed class UserTurn(string text, bool isAction) : ChatTurn
{
    public string Text { get; } = text;

    /// <summary>True for an interface action (shown as a small chip).</summary>
    public bool IsAction { get; } = isAction;
}

/// <summary>An assistant answer: an ordered list of segments that grows while it streams.</summary>
public sealed class AssistantTurn : ChatTurn
{
    public ObservableCollection<ChatSegment> Segments { get; } = new();

    /// <summary>True while the answer is streaming.</summary>
    public bool IsStreaming { get; internal set; }

    public event EventHandler? StreamingChanged;

    internal void SetStreaming(bool value)
    {
        IsStreaming = value;
        StreamingChanged?.Invoke(this, EventArgs.Empty);
    }

    internal MarkdownSegment Markdown()
    {
        if (Segments.Count > 0 && Segments[^1] is MarkdownSegment { IsClosed: false } last)
        {
            return last;
        }

        var segment = new MarkdownSegment();
        Segments.Add(segment);
        return segment;
    }

    internal void CloseMarkdown()
    {
        if (Segments.Count > 0 && Segments[^1] is MarkdownSegment last)
        {
            last.Close();
        }
    }
}

/// <summary>A piece of an assistant answer.</summary>
public abstract class ChatSegment;

/// <summary>Markdown text, appended as it streams.</summary>
public sealed class MarkdownSegment : ChatSegment
{
    private readonly StringBuilder _text = new();

    public string Text => _text.ToString();

    public bool IsClosed { get; private set; }

    public event Action<string>? Appended;

    public event EventHandler? Closed;

    internal void Append(string text)
    {
        _text.Append(text);
        Appended?.Invoke(text);
    }

    internal void Close()
    {
        if (!IsClosed)
        {
            IsClosed = true;
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}

/// <summary>An A2UI surface shown at this place of the answer.</summary>
public sealed class SurfaceSegment(string surfaceId) : ChatSegment
{
    public string SurfaceId { get; } = surfaceId;
}

/// <summary>Reasoning summary.</summary>
public sealed class ReasoningSegment : ChatSegment
{
    private readonly StringBuilder _text = new();
    private readonly DateTime _start = DateTime.UtcNow;

    public string Text => _text.ToString();

    public bool IsDone { get; private set; }

    public TimeSpan Elapsed { get; private set; }

    public event EventHandler? Changed;

    internal void Append(string text)
    {
        _text.Append(text);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void Done()
    {
        if (!IsDone)
        {
            IsDone = true;
            Elapsed = DateTime.UtcNow - _start;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}

/// <summary>A tool call: running, then done with its result.</summary>
public sealed class ToolSegment(string id, string name) : ChatSegment
{
    public string Id { get; } = id;

    public string Name { get; } = name;

    public string? Arguments { get; private set; }

    public string? Result { get; private set; }

    public bool IsError { get; private set; }

    public bool IsDone { get; private set; }

    public event EventHandler? Changed;

    internal void SetArguments(string json)
    {
        Arguments = json;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void Complete(string result, bool isError)
    {
        Result = result;
        IsError = isError;
        IsDone = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>The kind of a notice.</summary>
public enum NoticeKind
{
    Info,
    Warning,
    Error
}

/// <summary>A short line: stopped, refused, corrected, errors.</summary>
public sealed class NoticeSegment(NoticeKind kind, string text, string? details = null) : ChatSegment
{
    public NoticeKind Kind { get; } = kind;

    public string Text { get; } = text;

    public string? Details { get; } = details;
}
