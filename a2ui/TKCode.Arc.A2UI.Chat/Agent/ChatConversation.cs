using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>Agent settings of a conversation.</summary>
public sealed class ChatAgentSettings
{
    /// <summary>Send renderer errors back to the model to correct (at most <see cref="MaxCorrections"/> rounds a turn).</summary>
    public bool AutoCorrect { get; set; } = true;

    public int MaxCorrections { get; set; } = 2;

    /// <summary>Tool steps per user turn.</summary>
    public int MaxSteps { get; set; } = 8;

    /// <summary>The tools offered to the model.</summary>
    public IReadOnlyList<ChatTool> Tools { get; set; } = ChatTools.All;

    /// <summary>Include the full catalog JSON Schema in the system prompt instead of the compact reference.</summary>
    public bool FullSchemaInPrompt { get; set; }
}

/// <summary>
/// One conversation: the history sent to the model, the visible turns, and the A2UI renderer of its surfaces. Runs the
/// agent loop: stream a response, render A2UI blocks as they arrive, run tools, send interface actions back, and let the
/// model fix rejected A2UI.
/// </summary>
public sealed class ChatConversation : IDisposable
{
    private readonly List<ChatMessage> _history = new();
    private readonly List<string> _turnErrors = new();
    private readonly Queue<string> _pendingActions = new();
    private CancellationTokenSource? _run;
    private AssistantTurn? _current;

    public ChatConversation(A2UIRendererOptions? rendererOptions = null)
    {
        Renderer = new A2UIRenderer(rendererOptions ?? new A2UIRendererOptions());
        Renderer.SurfaceCreated += (_, e) => _current?.Segments.Add(new SurfaceSegment(e.Surface.Id));
        Renderer.Outbound += OnOutbound;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = "New chat";

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>True for a built-in example conversation.</summary>
    public string? ShowcaseId { get; set; }

    public A2UIRenderer Renderer { get; }

    public ObservableCollection<ChatTurn> Turns { get; } = new();

    /// <summary>A2UI traffic for the inspector: inbound lines ("in") and outbound messages ("out").</summary>
    public ObservableCollection<(string Direction, string Json)> Log { get; } = new();

    public IReadOnlyList<ChatMessage> History => _history;

    public ChatAgentSettings Settings { get; set; } = new();

    /// <summary>Supplies the provider and model for the next request (the user may switch between turns).</summary>
    public Func<(IChatProvider Provider, string Model)?>? ProviderSource { get; set; }

    /// <summary>Agent-side functions answered locally (<c>callAgentFunction</c>).</summary>
    public Dictionary<string, Func<JsonObject, JsonNode?>> AgentFunctions { get; } = new(StringComparer.Ordinal);

    public bool IsRunning => _run is not null;

    public event EventHandler? RunningChanged;

    /// <summary>Raised when the conversation changed in a way worth saving.</summary>
    public event EventHandler? Saved;

    /// <summary>Sends a user message and runs the agent until it answers.</summary>
    public async Task SendAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || IsRunning)
        {
            return;
        }

        if (Turns.Count == 0)
        {
            Title = text.Length > 48 ? text[..48].TrimEnd() + "…" : text;
        }

        Turns.Add(new UserTurn(text, isAction: false));
        _history.Add(ChatMessage.User(text));
        await RunAsync().ConfigureAwait(true);
    }

    /// <summary>Stops the running answer; what arrived stays.</summary>
    public void Stop() => _run?.Cancel();

    private async Task RunAsync()
    {
        if (ProviderSource?.Invoke() is not { } source)
        {
            Turns.Add(Notice(NoticeKind.Warning, "No provider is set up. Add one in Settings, or open an example."));
            return;
        }

        _run = new CancellationTokenSource();
        RunningChanged?.Invoke(this, EventArgs.Empty);
        var turn = new AssistantTurn();
        _current = turn;
        Turns.Add(turn);
        turn.SetStreaming(true);
        _turnErrors.Clear();
        var corrections = 0;
        try
        {
            var system = A2UISystemPrompt.Build(Renderer.Catalogs, Settings.FullSchemaInPrompt, Settings.Tools);
            for (var step = 0; step < Settings.MaxSteps; step++)
            {
                var stop = await StreamStepAsync(source.Provider, source.Model, system, turn, _run.Token).ConfigureAwait(true);
                if (stop.Calls.Count > 0)
                {
                    var results = new List<ChatPart>();
                    foreach (var call in stop.Calls)
                    {
                        var output = ChatTools.Run(call.Name, call.ArgumentsJson, out var isError);
                        results.Add(new ChatToolResultPart(call.Id, call.Name, output, isError));
                        turn.Segments.OfType<ToolSegment>().LastOrDefault(t => t.Id == call.Id)?.Complete(output, isError);
                    }

                    _history.Add(new ChatMessage(ChatRole.User, results));
                    continue;
                }

                if (stop.Reason == ChatStopReason.Refusal)
                {
                    turn.Segments.Add(new NoticeSegment(NoticeKind.Warning, "The model declined to answer.", stop.Detail));
                }
                else if (stop.Reason == ChatStopReason.MaxTokens)
                {
                    turn.Segments.Add(new NoticeSegment(NoticeKind.Warning, "The answer was cut off at the output limit."));
                }

                if (_turnErrors.Count > 0 && Settings.AutoCorrect && corrections < Settings.MaxCorrections)
                {
                    corrections++;
                    var errors = "<a2ui_errors>[" + string.Join(",", _turnErrors) + "]</a2ui_errors>";
                    turn.Segments.Add(new NoticeSegment(NoticeKind.Info, $"Fixing {_turnErrors.Count} interface error(s)…", string.Join("\n", _turnErrors)));
                    _turnErrors.Clear();
                    _history.Add(ChatMessage.User(errors));
                    continue;
                }

                break;
            }
        }
        catch (OperationCanceledException)
        {
            turn.Segments.Add(new NoticeSegment(NoticeKind.Info, "Stopped."));
        }
        catch (ChatProviderException ex)
        {
            turn.Segments.Add(new NoticeSegment(NoticeKind.Error, ex.Message, ex.ErrorType));
        }
        finally
        {
            turn.CloseMarkdown();
            turn.SetStreaming(false);
            _current = null;
            _run.Dispose();
            _run = null;
            UpdatedAt = DateTimeOffset.Now;
            RunningChanged?.Invoke(this, EventArgs.Empty);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        if (_pendingActions.Count > 0)
        {
            var next = _pendingActions.Dequeue();
            _history.Add(ChatMessage.User(next));
            await RunAsync().ConfigureAwait(true);
        }
    }

    private sealed record StepResult(ChatStopReason Reason, string? Detail, List<ChatToolCallPart> Calls);

    private async Task<StepResult> StreamStepAsync(IChatProvider provider, string model, string system, AssistantTurn turn, CancellationToken token)
    {
        var request = new ChatRequest { Model = model, System = system, Messages = _history.ToList(), Tools = Settings.Tools };
        var text = new StringBuilder();
        var parts = new List<ChatPart>();
        var calls = new List<ChatToolCallPart>();
        var splitter = new A2UIFenceSplitter();
        splitter.Markdown += md => turn.Markdown().Append(md);
        splitter.BlockOpened += turn.CloseMarkdown;
        splitter.Envelope += line =>
        {
            Log.Add(("in", line));
            Renderer.Process(line);
        };
        ReasoningSegment? reasoning = null;
        var reason = ChatStopReason.EndTurn;
        string? detail = null;
        try
        {
            await foreach (var evt in provider.StreamAsync(request, token).ConfigureAwait(true))
            {
                switch (evt)
                {
                    case ChatTextDelta delta:
                        reasoning?.Done();
                        text.Append(delta.Text);
                        splitter.Feed(delta.Text);
                        break;
                    case ChatReasoningDelta delta:
                        if (reasoning is null || reasoning.IsDone)
                        {
                            reasoning = new ReasoningSegment();
                            turn.CloseMarkdown();
                            turn.Segments.Add(reasoning);
                        }

                        reasoning.Append(delta.Text);
                        break;
                    case ChatReasoningDone done:
                        reasoning?.Done();
                        parts.Add(done.Part);
                        break;
                    case ChatToolCallStarted started:
                        turn.CloseMarkdown();
                        turn.Segments.Add(new ToolSegment(started.Id, started.Name));
                        break;
                    case ChatToolCallDone done:
                        calls.Add(done.Call);
                        var segment = turn.Segments.OfType<ToolSegment>().LastOrDefault(t => t.Id == done.Call.Id);
                        if (segment is null)
                        {
                            segment = new ToolSegment(done.Call.Id, done.Call.Name);
                            turn.Segments.Add(segment);
                        }

                        segment.SetArguments(done.Call.ArgumentsJson);
                        break;
                    case ChatCompleted completed:
                        reason = completed.Reason;
                        detail = completed.Detail;
                        break;
                }
            }
        }
        finally
        {
            splitter.Complete();
            reasoning?.Done();
            // What arrived stays in the history, even when stopped, so later turns see the same interface.
            if (text.Length > 0)
            {
                parts.Add(new ChatTextPart(text.ToString()));
            }

            parts.AddRange(calls);
            if (parts.Count > 0)
            {
                _history.Add(new ChatMessage(ChatRole.Assistant, parts));
            }
        }

        return new StepResult(reason, detail, calls);
    }

    private void OnOutbound(object? sender, A2UIOutboundEventArgs e)
    {
        Log.Add(("out", e.ToJson()));
        switch (e.Kind)
        {
            case A2UIOutboundKind.Error:
                if (_current is not null)
                {
                    _turnErrors.Add(e.Envelope["error"]!.ToJsonString());
                }

                break;
            case A2UIOutboundKind.Action:
                OnAction(e);
                break;
            case A2UIOutboundKind.CallAgentFunction:
                AnswerAgentFunction(e.Envelope["callAgentFunction"] as JsonObject);
                break;
        }
    }

    private void OnAction(A2UIOutboundEventArgs e)
    {
        var action = e.Action!;
        var label = A2UIJson.AsString(action["userMessage"]) ?? A2UIJson.AsString(action["name"]) ?? "Action";
        var message = new StringBuilder("<a2ui_action>").Append(action.ToJsonString()).Append("</a2ui_action>");
        if (Renderer.GetSurface(e.SurfaceId ?? string.Empty) is { SendDataModel: true } surface)
        {
            message.Append("\n<a2ui_data_model surface=\"").Append(surface.Id).Append("\">")
                   .Append(surface.DataModel.Get("/")?.ToJsonString() ?? "{}").Append("</a2ui_data_model>");
        }

        Turns.Add(new UserTurn(label, isAction: true));
        if (IsRunning)
        {
            _pendingActions.Enqueue(message.ToString());
            return;
        }

        _history.Add(ChatMessage.User(message.ToString()));
        _ = RunAsync();
    }

    private void AnswerAgentFunction(JsonObject? call)
    {
        if (call is null)
        {
            return;
        }

        var id = A2UIJson.AsString(call["functionCallId"]) ?? string.Empty;
        var name = A2UIJson.AsString(call["callFunction"]?["@call"]) ?? string.Empty;
        var args = call["callFunction"]?["args"] as JsonObject ?? new JsonObject();
        var response = new JsonObject { ["functionCallId"] = id };
        if (AgentFunctions.TryGetValue(name, out var function))
        {
            response["value"] = function(args)?.DeepClone();
        }
        else
        {
            response["error"] = new JsonObject { ["code"] = "INVALID_FUNCTION_CALL", ["message"] = $"The agent has no function '{name}'." };
        }

        // Answered after the current message is applied.
        Avalonia.Threading.Dispatcher.UIThread.Post(() => Renderer.Process(new JsonObject { ["version"] = "1.0", ["agentFunctionResponse"] = response }.ToJsonString()));
    }

    /// <summary>The conversation as JSON: history, title and the surfaces' current data.</summary>
    public JsonObject ToJson()
    {
        var history = new JsonArray();
        foreach (var message in _history)
        {
            history.Add((JsonNode?)(ChatSerializer.Message(message)));
        }

        var data = new JsonObject();
        foreach (var surface in Renderer.Surfaces)
        {
            data[surface.Id] = surface.DataModel.Get("/");
        }

        return new JsonObject
        {
            ["id"] = Id,
            ["title"] = Title,
            ["updatedAt"] = UpdatedAt.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
            ["showcaseId"] = ShowcaseId,
            ["history"] = history,
            ["data"] = data
        };
    }

    /// <summary>
    /// Restores a saved conversation: the history is replayed into the visible turns and the renderer (no requests,
    /// no outbound messages), then the saved data is written back into the surfaces.
    /// </summary>
    public void Restore(JsonObject json)
    {
        Id = A2UIJson.AsString(json["id"]) ?? Id;
        Title = A2UIJson.AsString(json["title"]) ?? Title;
        ShowcaseId = A2UIJson.AsString(json["showcaseId"]);
        if (DateTimeOffset.TryParse(A2UIJson.AsString(json["updatedAt"]), System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.RoundtripKind, out var updated))
        {
            UpdatedAt = updated;
        }

        Renderer.Outbound -= OnOutbound;
        try
        {
            AssistantTurn? turn = null;
            foreach (var messageJson in (json["history"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
            {
                var message = ChatSerializer.Message(messageJson);
                _history.Add(message);
                if (message.Role == ChatRole.User)
                {
                    foreach (var result in message.Parts.OfType<ChatToolResultPart>())
                    {
                        turn?.Segments.OfType<ToolSegment>().LastOrDefault(t => t.Id == result.CallId)?.Complete(result.Content, result.IsError);
                    }

                    var text = string.Concat(message.Parts.OfType<ChatTextPart>().Select(t => t.Text));
                    if (text.StartsWith("<a2ui_errors>", StringComparison.Ordinal) || text.Length == 0)
                    {
                        continue;
                    }

                    if (text.StartsWith("<a2ui_action>", StringComparison.Ordinal))
                    {
                        var end = text.IndexOf("</a2ui_action>", StringComparison.Ordinal);
                        var action = end > 0 ? A2UIJson.Parse(text[13..end]) as JsonObject : null;
                        Turns.Add(new UserTurn(A2UIJson.AsString(action?["userMessage"]) ?? A2UIJson.AsString(action?["name"]) ?? "Action", isAction: true));
                    }
                    else
                    {
                        Turns.Add(new UserTurn(text, isAction: false));
                    }

                    turn = null;
                    continue;
                }

                if (turn is null)
                {
                    turn = new AssistantTurn();
                    Turns.Add(turn);
                }

                _current = turn;
                var splitter = new A2UIFenceSplitter();
                splitter.Markdown += md => turn.Markdown().Append(md);
                splitter.BlockOpened += turn.CloseMarkdown;
                splitter.Envelope += line => Renderer.Process(line);
                foreach (var part in message.Parts)
                {
                    switch (part)
                    {
                        case ChatReasoningPart reasoning when reasoning.Summary.Length > 0:
                            var segment = new ReasoningSegment();
                            segment.Append(reasoning.Summary);
                            segment.Done();
                            turn.Segments.Add(segment);
                            break;
                        case ChatTextPart text:
                            splitter.Feed(text.Text);
                            splitter.Complete();
                            break;
                        case ChatToolCallPart call:
                            var tool = new ToolSegment(call.Id, call.Name);
                            tool.SetArguments(call.ArgumentsJson);
                            turn.Segments.Add(tool);
                            break;
                    }
                }

                turn.CloseMarkdown();
                _current = null;
            }

            if (json["data"] is JsonObject data)
            {
                foreach (var (surfaceId, value) in data)
                {
                    Renderer.GetSurface(surfaceId)?.DataModel.Set("/", value);
                }
            }
        }
        finally
        {
            _current = null;
            Renderer.Outbound += OnOutbound;
        }
    }

    private static AssistantTurn Notice(NoticeKind kind, string text)
    {
        var turn = new AssistantTurn();
        turn.Segments.Add(new NoticeSegment(kind, text));
        return turn;
    }

    public void Dispose()
    {
        _run?.Cancel();
        Renderer.Dispose();
    }
}
