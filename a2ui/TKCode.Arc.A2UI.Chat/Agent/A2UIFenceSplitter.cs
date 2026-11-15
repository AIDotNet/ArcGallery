using System.Text;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// Splits streamed assistant text into Markdown and A2UI: a line <c>```a2ui</c> opens a block, each complete line inside
/// is one A2UI envelope, a line <c>```</c> closes it. Text outside blocks is passed through as it arrives (a partial line
/// is held back only while it could still turn into a fence).
/// </summary>
public sealed class A2UIFenceSplitter
{
    private readonly StringBuilder _line = new();
    private bool _inBlock;

    /// <summary>Markdown text outside A2UI blocks.</summary>
    public event Action<string>? Markdown;

    /// <summary>One A2UI envelope line.</summary>
    public event Action<string>? Envelope;

    /// <summary>A block opened (the place where its surfaces belong).</summary>
    public event Action? BlockOpened;

    /// <summary>True inside an A2UI block.</summary>
    public bool InBlock => _inBlock;

    public void Feed(string delta)
    {
        foreach (var c in delta)
        {
            if (c == '\n')
            {
                EndLine(complete: true);
                continue;
            }

            _line.Append(c);
            // Outside a block, flush text as soon as the line cannot be a fence.
            if (!_inBlock && !CouldBeFence(_line))
            {
                Markdown?.Invoke(_line.ToString());
                _line.Clear();
                _flushedPartial = true;
            }
        }
    }

    private bool _flushedPartial;

    /// <summary>Ends the stream: a held line is emitted; an unclosed block's last line is processed.</summary>
    public void Complete()
    {
        if (_line.Length > 0)
        {
            EndLine(complete: false);
        }

        _inBlock = false;
    }

    private void EndLine(bool complete)
    {
        var line = _line.ToString();
        _line.Clear();
        var trimmed = line.Trim();
        if (!_inBlock)
        {
            if (!_flushedPartial && trimmed.Equals("```a2ui", StringComparison.OrdinalIgnoreCase))
            {
                _inBlock = true;
                BlockOpened?.Invoke();
            }
            else
            {
                Markdown?.Invoke(complete ? line + "\n" : line);
            }

            _flushedPartial = false;
            return;
        }

        if (trimmed == "```")
        {
            _inBlock = false;
            return;
        }

        if (trimmed.Length > 0)
        {
            Envelope?.Invoke(trimmed);
        }
    }

    // A line start that may still become "```a2ui".
    private bool CouldBeFence(StringBuilder line)
    {
        if (_flushedPartial)
        {
            return false;
        }

        var i = 0;
        while (i < line.Length && line[i] is ' ' or '\t')
        {
            i++;
        }

        const string fence = "```a2ui";
        for (var j = 0; i + j < line.Length; j++)
        {
            if (j >= fence.Length)
            {
                return false;
            }

            if (char.ToLowerInvariant(line[i + j]) != fence[j])
            {
                return false;
            }
        }

        return true;
    }
}
