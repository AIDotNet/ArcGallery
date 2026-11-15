using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>A built-in example: a first prompt and scripted answers, one per interface action.</summary>
public sealed class Showcase
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Prompt { get; init; }

    /// <summary>The answer to the prompt.</summary>
    public required string Start { get; init; }

    /// <summary>Answers to actions, by action name; <c>*</c> answers any other action.</summary>
    public IReadOnlyDictionary<string, string> OnAction { get; init; } = new Dictionary<string, string>();

    /// <summary>Agent functions the example answers locally.</summary>
    public IReadOnlyDictionary<string, Func<JsonObject, JsonNode?>> AgentFunctions { get; init; } = new Dictionary<string, Func<JsonObject, JsonNode?>>();
}

/// <summary>
/// A provider that replays an example's script instead of calling a model: text streams in small pieces, A2UI lines one
/// by one, so the examples show the same progressive rendering as a real model.
/// </summary>
public sealed class ReplayProvider(Showcase showcase) : IChatProvider
{
    /// <summary>Delay between text pieces (shorter in tests).</summary>
    public TimeSpan TextDelay { get; set; } = TimeSpan.FromMilliseconds(12);

    /// <summary>Delay between A2UI lines.</summary>
    public TimeSpan LineDelay { get; set; } = TimeSpan.FromMilliseconds(70);

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var script = Pick(request);
        var inBlock = false;
        foreach (var rawLine in script.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine + "\n";
            if (rawLine.Trim().StartsWith("```", StringComparison.Ordinal))
            {
                inBlock = !inBlock;
                yield return new ChatTextDelta(line);
                continue;
            }

            if (inBlock)
            {
                await Task.Delay(LineDelay, cancellationToken).ConfigureAwait(true);
                yield return new ChatTextDelta(line);
                continue;
            }

            for (var i = 0; i < line.Length; i += 6)
            {
                await Task.Delay(TextDelay, cancellationToken).ConfigureAwait(true);
                yield return new ChatTextDelta(line.Substring(i, Math.Min(6, line.Length - i)));
            }
        }

        yield return new ChatCompleted(ChatStopReason.EndTurn);
    }

    private string Pick(ChatRequest request)
    {
        var last = request.Messages.LastOrDefault(m => m.Role == ChatRole.User);
        var text = last?.Parts.OfType<ChatTextPart>().FirstOrDefault()?.Text ?? string.Empty;
        const string open = "<a2ui_action>";
        var start = text.IndexOf(open, StringComparison.Ordinal);
        if (start < 0)
        {
            return text.StartsWith("<a2ui_errors>", StringComparison.Ordinal) ? "Fixed." : showcase.Start;
        }

        var end = text.IndexOf("</a2ui_action>", start, StringComparison.Ordinal);
        var json = end > start ? text[(start + open.Length)..end] : string.Empty;
        string? name = null;
        string? choice = null;
        try
        {
            var action = A2UIJson.Parse(json) as JsonObject;
            name = A2UIJson.AsString(action?["name"]);
            // An action whose context carries one string can have its own answer: "name:value".
            if (action?["context"] is JsonObject { Count: 1 } context)
            {
                choice = A2UIJson.AsString(context.First().Value);
            }
        }
        catch (A2UIException)
        {
        }

        if (name is not null && choice is not null && showcase.OnAction.TryGetValue(name + ":" + choice, out var specific))
        {
            return specific;
        }

        if (name is not null && showcase.OnAction.TryGetValue(name, out var answer))
        {
            return answer;
        }

        return showcase.OnAction.TryGetValue("*", out var any) ? any : "Got it.";
    }

    public Task<IReadOnlyList<ChatModelInfo>> ListModelsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<ChatModelInfo>>([new ChatModelInfo("replay", "Example")]);
}
