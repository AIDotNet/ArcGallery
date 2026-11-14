using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;

namespace TKCode.Arc.A2UI.Chat.Tests;

public sealed class AppTests
{
    [AvaloniaFact]
    public void App_opens_an_example_and_shows_its_surface()
    {
        var state = new ChatAppState(new MemoryChatStorage());
        var app = new A2UIChatApp(state);
        var window = new Window { Width = 1200, Height = 800, Content = app };
        window.Show();
        Ui.Pump();
        Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "What can I show you?");

        var first = window.GetVisualDescendants().OfType<Button>().First(b => b.Content is string s && s == ShowcaseLibrary.All[0].Prompt);
        first.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Ui.Until(() => window.GetVisualDescendants().OfType<A2UISurfaceView>().Any(v => v.ShownSurface?.Root is not null) && !state.Conversations[0].IsRunning, 20000);
        Assert.Contains(window.GetVisualDescendants().OfType<MetricCard>(), _ => true);
        window.Close();
    }

    [AvaloniaFact]
    public void Narrow_window_moves_the_sidebar_into_a_drawer()
    {
        var app = new A2UIChatApp(new ChatAppState(new MemoryChatStorage()));
        var window = new Window { Width = 420, Height = 800, Content = app };
        window.Show();
        Ui.Pump();
        Assert.Contains(window.GetVisualDescendants().OfType<Drawer>(), d => d.Content is not null);
        window.Close();
    }

    [AvaloniaFact]
    public void Settings_keep_keys_only_when_remembered()
    {
        var storage = new MemoryChatStorage();
        var state = new ChatAppState(storage);
        var forget = ChatProviderConfig.AnthropicPreset();
        forget.ApiKey = "secret-1";
        var remember = ChatProviderConfig.OpenAIChatPreset();
        remember.ApiKey = "secret-2";
        remember.RememberKey = true;
        state.Providers.Add(forget);
        state.Providers.Add(remember);
        state.Save();

        var reloaded = new ChatAppState(storage);
        Assert.Equal(string.Empty, reloaded.Providers[0].ApiKey);
        Assert.Equal("secret-2", reloaded.Providers[1].ApiKey);
        Assert.DoesNotContain("secret", state.Export());
    }

    [AvaloniaFact]
    public void An_example_opens_once_and_stays_out_of_recents()
    {
        var state = new ChatAppState(new MemoryChatStorage());
        var first = state.OpenShowcase(ShowcaseLibrary.All[0]);
        var again = state.OpenShowcase(ShowcaseLibrary.All[0]);
        Assert.Same(first, again);
        Assert.Single(state.Conversations);
        Ui.Until(() => !first.IsRunning);
    }

    [AvaloniaFact]
    public void A_long_answer_shows_a_vertical_scroll_bar()
    {
        var state = new ChatAppState(new MemoryChatStorage());
        var app = new A2UIChatApp(state);
        var window = new Window { Width = 1200, Height = 600, Content = app };
        window.Show();
        Ui.Pump();
        var first = window.GetVisualDescendants().OfType<Button>().First(b => b.Content is string s && s == ShowcaseLibrary.Rich[0].Prompt);
        first.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Ui.Until(() => state.Conversations.Count > 0 && !state.Conversations[0].IsRunning, 20000);
        Ui.Pump();

        var thread = window.GetVisualDescendants().OfType<ScrollViewer>().First(v => v.GetType().Name == "ChatThreadView");
        Assert.True(thread.Extent.Height > thread.Viewport.Height, $"extent {thread.Extent.Height}, viewport {thread.Viewport.Height}");
        Ui.Pump();
        var bar = thread.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.ScrollBar>().First(b => b.TemplatedParent == thread && b.Orientation == Avalonia.Layout.Orientation.Vertical);
        Assert.True(bar.IsEffectivelyVisible && bar.Bounds.Height > 0, $"visible {bar.IsVisible}/{bar.IsEffectivelyVisible}, bounds {bar.Bounds}, vis {thread.VerticalScrollBarVisibility}");
        Assert.False(bar.AllowAutoHide);
        window.Close();
    }

    internal static void OpenSettings(Window window)
    {
        var label = window.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == "Settings");
        var button = label.GetVisualAncestors().OfType<Button>().First();
        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Ui.Pump();
    }

    [AvaloniaFact]
    public void Settings_page_keeps_its_buttons_clear_of_the_fields_and_closes_on_escape()
    {
        var state = new ChatAppState(new MemoryChatStorage());
        state.Providers.Add(ChatProviderConfig.AnthropicPreset());
        var window = new Window { Width = 1200, Height = 700, Content = new A2UIChatApp(state) };
        window.Show();
        Ui.Pump();
        OpenSettings(window);

        var save = window.GetVisualDescendants().OfType<Button>().First(b => b.Content is string s && s == "Save");
        var saveTop = save.TranslatePoint(new Point(0, 0), window)!.Value.Y;
        var editor = save.GetVisualAncestors().OfType<DockPanel>().First().GetVisualDescendants().OfType<ScrollViewer>().First();
        editor.Offset = new Vector(0, editor.Extent.Height);
        Ui.Pump();
        foreach (var field in editor.GetVisualDescendants().OfType<Textarea>().Where(t => t.IsEffectivelyVisible))
        {
            // Every frame stays inside its field and the field inside the scrolling area, above the footer.
            var frame = field.GetVisualDescendants().OfType<FieldFrame>().First();
            Assert.True(frame.Bounds.Height <= field.Bounds.Height + 0.5, $"{field.Label}: frame {frame.Bounds.Height} > field {field.Bounds.Height}");
            var bottom = field.TranslatePoint(new Point(0, field.Bounds.Height), window)!.Value.Y;
            Assert.True(bottom <= saveTop || bottom > editor.TranslatePoint(new Point(0, editor.Bounds.Height), window)!.Value.Y, $"{field.Label} reaches the footer");
        }

        Assert.True(editor.TranslatePoint(new Point(0, editor.Bounds.Height), window)!.Value.Y <= saveTop);
        save.Focus();
        Ui.Pump();
        window.KeyPressQwerty(Avalonia.Input.PhysicalKey.Escape, Avalonia.Input.RawInputModifiers.None);
        Ui.Pump();
        Assert.DoesNotContain(window.GetVisualDescendants().OfType<Button>(), b => b.Content is string s && s == "Save" && b.IsEffectivelyVisible);
        window.Close();
    }
}
