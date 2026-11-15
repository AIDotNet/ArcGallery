using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls.ApplicationLifetimes;
using TKCode.Arc;
using TKCode.Arc.A2UI.Chat;
using TKCode.Arc.Fonts;
using TKCode.Arc.Localization;

namespace TKCode.Arc.A2UI.Chat.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        // A visitor who reads Chinese needs the font for the first frame: fetched here, the page stays responsive meanwhile.
        if (Initial() != LanguageTags.EnUS)
        {
            await CjkFontCollection.PreloadAsync();
        }

        await BuildAvaloniaApp().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .WithInterFont()
                  .WithArcFonts()
                  .WithCjkFont()
                  .UseArc(arc => arc
                      .UseLanguages(Initial(), [LanguageTags.EnUS, LanguageTags.ZhCN, LanguageTags.ZhTW])
                      .UseArcA2UIChat());

    private static LanguageTag Initial()
    {
        foreach (var name in BrowserChatStorage.Languages().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                var traditional = name.Contains("TW", StringComparison.OrdinalIgnoreCase) || name.Contains("HK", StringComparison.OrdinalIgnoreCase)
                                  || name.Contains("Hant", StringComparison.OrdinalIgnoreCase);
                System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo(traditional ? "zh-TW" : "zh-CN");
                return traditional ? LanguageTags.ZhTW : LanguageTags.ZhCN;
            }

            if (name.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                return LanguageTags.EnUS;
            }
        }

        return LanguageTags.EnUS;
    }
}

public sealed class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            var state = new ChatAppState(new BrowserChatStorage());
            state.LoadConversations();
            single.MainView = new A2UIChatApp(state);
        }

        base.OnFrameworkInitializationCompleted();
    }
}

/// <summary>The browser's local storage (the a2ui-store module of main.js); a blocked storage works in memory.</summary>
internal sealed partial class BrowserChatStorage : IChatStorage
{
    private readonly MemoryChatStorage _fallback = new();

    public string? Read(string key)
    {
        var value = Get(key);
        return value.Length == 0 ? _fallback.Read(key) : value;
    }

    public void Write(string key, string value)
    {
        if (!Set(key, value))
        {
            _fallback.Write(key, value);
        }
    }

    public void Remove(string key)
    {
        Delete(key);
        _fallback.Remove(key);
    }

    public IEnumerable<string> Keys(string prefix)
        => List(prefix).Split('\n', StringSplitOptions.RemoveEmptyEntries).Concat(_fallback.Keys(prefix)).Distinct().ToList();

    [JSImport("languages", "a2ui-store")]
    internal static partial string Languages();

    [JSImport("get", "a2ui-store")]
    private static partial string Get(string key);

    [JSImport("set", "a2ui-store")]
    private static partial bool Set(string key, string value);

    [JSImport("remove", "a2ui-store")]
    private static partial void Delete(string key);

    [JSImport("keys", "a2ui-store")]
    private static partial string List(string prefix);
}
