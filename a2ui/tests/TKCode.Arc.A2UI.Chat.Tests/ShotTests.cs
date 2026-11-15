using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;

namespace TKCode.Arc.A2UI.Chat.Tests;

/// <summary>Screenshots for review, written only when A2UI_SHOTS names an output folder.</summary>
public sealed class ShotTests
{
    private static string? Folder => Environment.GetEnvironmentVariable("A2UI_SHOTS");

    [AvaloniaFact]
    public void Capture_examples()
    {
        if (Folder is not { } folder)
        {
            return;
        }

        Directory.CreateDirectory(folder);
        Capture(folder, "empty", 1280, 820, null);
        foreach (var showcase in ShowcaseLibrary.All)
        {
            Capture(folder, showcase.Id, 1280, ShowcaseLibrary.Rich.Contains(showcase) ? 1700 : 1000, showcase);
            if (s_after.TryGetValue(showcase.Id, out var label))
            {
                Capture(folder, showcase.Id + "-after", 1280, 1700, showcase, label);
            }
        }

        Capture(folder, "narrow-overview", 420, 900, ShowcaseLibrary.All[0]);
        CaptureSettings(folder, "settings", 1280, 900);
        CaptureSettings(folder, "settings-narrow", 420, 900);
    }

    private static void CaptureSettings(string folder, string name, int width, int height)
    {
        var state = new ChatAppState(new MemoryChatStorage());
        state.Providers.Add(ChatProviderConfig.AnthropicPreset());
        state.Providers.Add(ChatProviderConfig.OpenAIResponsesPreset());
        var window = new Window { Width = width, Height = height, Content = new A2UIChatApp(state) };
        window.Show();
        Ui.Pump();
        if (width < 768)
        {
            // Narrow: the sidebar lives in the drawer.
            window.GetVisualDescendants().OfType<Drawer>().First().IsOpen = true;
            Ui.Pump();
        }

        AppTests.OpenSettings(window);
        for (var i = 0; i < 20; i++)
        {
            Ui.Pump();
            Thread.Sleep(10);
        }

        var frame = window.CaptureRenderedFrame();
#pragma warning disable CS0618
        frame?.Save(Path.Combine(folder, name + ".png"));
#pragma warning restore CS0618
        window.Close();
    }

    // A button to press after the first answer, to capture the reply too.
    private static readonly Dictionary<string, string> s_after = new()
    {
        ["sales"] = "Share report",
        ["incident"] = "Escalate",
        ["project"] = "Plan next sprint",
        ["webhooks"] = "Create endpoint"
    };

    private static void Capture(string folder, string name, int width, int height, Showcase? showcase, string? press = null)
    {
        var state = new ChatAppState(new MemoryChatStorage());
        var app = new A2UIChatApp(state);
        var window = new Window { Width = width, Height = height, Content = app };
        window.Show();
        Ui.Pump();
        if (showcase is not null)
        {
            // Open it the way a visitor does: the example's prompt chip on the empty page.
            // ... or its row in the sidebar (the empty page offers only the rich examples).
            var button = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Content is string s && s == showcase.Prompt)
                ?? window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => ToolTip.GetTip(b) is string tip && tip == showcase.Description);
            if (button is not null)
            {
                button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            }

            Ui.Until(() => state.Conversations.Count > 0 && !state.Conversations[0].IsRunning, 30000);
            for (var i = 0; i < 30; i++)
            {
                Ui.Pump();
                Thread.Sleep(10);
            }
        }

        if (press is not null)
        {
            var label = window.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == press);
            label.GetVisualAncestors().OfType<Button>().First().RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Ui.Until(() => !state.Conversations[0].IsRunning, 30000);
            for (var i = 0; i < 30; i++)
            {
                Ui.Pump();
                Thread.Sleep(10);
            }

            // Show the end of the thread, where the reply is.
            var thread = window.GetVisualDescendants().OfType<ScrollViewer>().First(v => v.GetType().Name == "ChatThreadView");
            thread.Offset = new Avalonia.Vector(0, thread.Extent.Height);
            Ui.Pump();
        }

        var frame = window.CaptureRenderedFrame();
#pragma warning disable CS0618 // Avalonia's own Save(path) overload; the encoder options do not matter for a review picture.
        frame?.Save(Path.Combine(folder, name + ".png"));
#pragma warning restore CS0618
        window.Close();
    }
}
