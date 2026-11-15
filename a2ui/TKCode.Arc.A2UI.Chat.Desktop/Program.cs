using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using TKCode.Arc;
using TKCode.Arc.A2UI.Chat;
using TKCode.Arc.Fonts;
using TKCode.Arc.Localization;

namespace TKCode.Arc.A2UI.Chat.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .WithInterFont()
                  .WithArcFonts()
                  .UseArc(arc => arc
                      .UseLanguages(Initial(), [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW])
                      .UseArcA2UIChat());

    private static LanguageTag Initial()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        return name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? name.Contains("TW", StringComparison.OrdinalIgnoreCase) || name.Contains("HK", StringComparison.OrdinalIgnoreCase) ? LanguageTags.ZhTW : LanguageTags.ZhCN
            : LanguageTags.EnUS;
    }
}

public sealed class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var state = new ChatAppState(new FileChatStorage());
            state.LoadConversations();
            desktop.MainWindow = new Window
            {
                Title = "A2UI Chat",
                Width = 1280,
                Height = 860,
                MinWidth = 380,
                MinHeight = 480,
                Content = new A2UIChatApp(state)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
