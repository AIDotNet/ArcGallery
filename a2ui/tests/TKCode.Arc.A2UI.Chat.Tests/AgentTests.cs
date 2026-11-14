using Avalonia.Headless.XUnit;

namespace TKCode.Arc.A2UI.Chat.Tests;

public sealed class AgentTests
{
    [Fact]
    public void Splitter_separates_markdown_and_a2ui_lines()
    {
        var splitter = new A2UIFenceSplitter();
        var markdown = new List<string>();
        var lines = new List<string>();
        splitter.Markdown += markdown.Add;
        splitter.Envelope += lines.Add;
        foreach (var piece in new[] { "Hel", "lo\n``", "`a2", "ui\n{\"a\":", "1}\n{\"b\":2}\n``", "`\nBye" })
        {
            splitter.Feed(piece);
        }

        splitter.Complete();
        Assert.Equal("Hello\nBye", string.Concat(markdown));
        Assert.Equal(["{\"a\":1}", "{\"b\":2}"], lines);
    }

    [Fact]
    public void Calculator_evaluates_without_code()
    {
        Assert.Equal("14", ChatTools.Run("calculate", "{\"expression\":\"2 + 3 * 4\"}", out var e1));
        Assert.False(e1);
        ChatTools.Run("calculate", "{\"expression\":\"1/0\"}", out var e2);
        Assert.True(e2);
    }

    [AvaloniaFact]
    public void Agent_runs_tools_renders_a2ui_and_sends_actions_back()
    {
        const string basic = A2UICatalogs.BasicCatalogId;
        var provider = new ScriptedProvider(
            [new ChatToolCallStarted("t1", "calculate"), new ChatToolCallDone(new ChatToolCallPart("t1", "calculate", "{\"expression\":\"6*7\"}")), new ChatCompleted(ChatStopReason.ToolUse)],
            [
                new ChatTextDelta("The answer:\n```a2ui\n"),
                new ChatTextDelta("{\"version\":\"1.0\",\"createSurface\":{\"surfaceId\":\"s1\",\"catalogId\":\"" + basic + "\",\"components\":[{\"id\":\"root\",\"component\":\"Button\",\"child\":\"l\",\"action\":{\"event\":{\"name\":\"ok\"}}},{\"id\":\"l\",\"component\":\"Text\",\"text\":\"42\"}]}}\n"),
                new ChatTextDelta("```\n"),
                new ChatCompleted(ChatStopReason.EndTurn)
            ],
            [new ChatTextDelta("Thanks!"), new ChatCompleted(ChatStopReason.EndTurn)]);
        var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() })
        {
            ProviderSource = () => (provider, "m")
        };

        Ui.Wait(conversation.SendAsync("What is 6*7?"));
        Assert.Equal(2, provider.Requests.Count);
        Assert.Contains(provider.Requests[1].Messages, m => m.Parts.OfType<ChatToolResultPart>().Any(r => r.Content == "42"));
        var turn = (AssistantTurn)conversation.Turns[1];
        Assert.Contains(turn.Segments, s => s is SurfaceSegment { SurfaceId: "s1" });
        Assert.Contains(turn.Segments, s => s is ToolSegment { IsDone: true, Result: "42" });

        Assert.True(conversation.Renderer.GetSurface("s1")!.Root!.InvokeAction());
        Ui.Until(() => provider.Requests.Count == 3 && !conversation.IsRunning);
        var last = provider.Requests[2].Messages[^1];
        Assert.StartsWith("<a2ui_action>", ((ChatTextPart)last.Parts[0]).Text);
        Assert.Contains(conversation.Turns, t => t is UserTurn { IsAction: true, Text: "ok" });
    }

    [AvaloniaFact]
    public void Rejected_a2ui_is_sent_back_for_correction()
    {
        var provider = new ScriptedProvider(
            [new ChatTextDelta("```a2ui\n{\"version\":\"1.0\",\"createSurface\":{\"surfaceId\":\"bad\",\"catalogId\":\"" + A2UICatalogs.BasicCatalogId + "\",\"components\":[{\"id\":\"root\",\"component\":\"Text\",\"text\":5}]}}\n```\n"), new ChatCompleted(ChatStopReason.EndTurn)],
            [new ChatTextDelta("Fixed."), new ChatCompleted(ChatStopReason.EndTurn)]);
        var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() })
        {
            ProviderSource = () => (provider, "m")
        };
        Ui.Wait(conversation.SendAsync("show"));
        Assert.Equal(2, provider.Requests.Count);
        Assert.StartsWith("<a2ui_errors>", ((ChatTextPart)provider.Requests[1].Messages[^1].Parts[0]).Text);
    }

    [Fact]
    public void The_system_prompt_lists_the_arc_catalog_and_how_to_mix_catalogs()
    {
        var prompt = A2UISystemPrompt.Build(ChatAppState.Catalogs, fullSchema: false, []);
        Assert.Contains("\"@catalogId\": \"<that catalog's id>\"", prompt);
        Assert.Contains("catalogId: \"" + A2UICatalogs.ArcCatalogId + "\"", prompt);
        Assert.Contains("- Markdown (Rich text", prompt);
        Assert.Contains("- Rating (", prompt);
        Assert.Contains("getBaggageAllowance(flightId)", prompt);
        Assert.DoesNotContain("component*: ", prompt);
        // Compact enough to send with every request (cached by the provider): about 6k tokens at most.
        Assert.True(prompt.Length < 24_000, $"prompt is {prompt.Length} characters");
    }

    [AvaloniaFact]
    public void Every_example_plays_without_renderer_errors()
    {
        foreach (var showcase in ShowcaseLibrary.All)
        {
            var errors = new List<string>();
            var replay = new ReplayProvider(showcase) { TextDelay = TimeSpan.Zero, LineDelay = TimeSpan.Zero };
            var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() })
            {
                ProviderSource = () => (replay, "replay"),
                Settings = new ChatAgentSettings { Tools = [], AutoCorrect = false }
            };
            foreach (var (name, function) in showcase.AgentFunctions)
            {
                conversation.AgentFunctions[name] = function;
            }

            conversation.Renderer.ErrorReported += (_, e) => errors.Add(showcase.Id + ": " + e.Error.Message);
            Ui.Wait(conversation.SendAsync(showcase.Prompt));
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.NotEmpty(conversation.Renderer.Surfaces);

            // Every scripted answer to an action applies cleanly too ("name:value" keys carry the choice as context).
            foreach (var key in showcase.OnAction.Keys.Where(k => k != "*"))
            {
                var colon = key.IndexOf(':');
                var action = new System.Text.Json.Nodes.JsonObject { ["name"] = colon < 0 ? key : key[..colon] };
                if (colon >= 0)
                {
                    action["context"] = new System.Text.Json.Nodes.JsonObject { ["value"] = key[(colon + 1)..] };
                }

                Ui.Wait(conversation.SendAsync("<a2ui_action>" + action.ToJsonString() + "</a2ui_action>"));
                Assert.True(errors.Count == 0, key + "\n" + string.Join("\n", errors));
            }

            conversation.Dispose();
        }
    }

    [AvaloniaFact]
    public void Conversations_survive_save_and_restore()
    {
        var showcase = ShowcaseLibrary.All.First(s => s.Id == "contact");
        var replay = new ReplayProvider(showcase) { TextDelay = TimeSpan.Zero, LineDelay = TimeSpan.Zero };
        var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() }) { ProviderSource = () => (replay, "r") };
        Ui.Wait(conversation.SendAsync(showcase.Prompt));
        conversation.Renderer.GetSurface("contact_1")!.DataModel.Set("/contact/name", A2UIJson.Parse("\"Ada\""));
        var json = conversation.ToJson();

        var restored = new ChatConversation(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() });
        restored.Restore(json);
        Assert.Equal("Ada", A2UIJson.AsString(restored.Renderer.GetSurface("contact_1")!.DataModel.Get("/contact/name")));
        Assert.Equal(conversation.Turns.Count, restored.Turns.Count);
    }
}
