using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace TKCode.Arc.A2UI.Chat.Browser;

/// <summary>
/// The Chinese font of the WebAssembly host (the gallery's Noto Sans SC subset, SIL OFL 1.1, linked in as fonts/). A browser
/// gives Skia no system fonts, so without it every Chinese character is a box. It is fetched the first time a Chinese
/// character is drawn (synchronously: the text layout asks for the glyph while measuring), or ahead of the first frame for a
/// visitor who starts in Chinese (<see cref="PreloadAsync"/>), so the page is not held up. The same design as the gallery's
/// host (gallery/TKCode.Arc.Gallery.Browser/CjkFont.cs).
/// </summary>
internal sealed partial class CjkFontCollection : FontCollectionBase
{
    /// <summary>The key of the collection and the family inside it, as a font family string.</summary>
    public const string Family = "fonts:A2UICjk#Noto Sans SC";

    private const string File = "fonts/NotoSansSC-Subset.otf";
    private static readonly Uri s_key = new("fonts:A2UICjk");
    private static byte[]? s_preloaded;
    private bool _loadAttempted;

    public override Uri Key => s_key;

    public override bool TryGetGlyphTypeface(string familyName, FontStyle style, FontWeight weight, FontStretch stretch,
                                             [NotNullWhen(true)] out GlyphTypeface? glyphTypeface)
    {
        EnsureLoaded();
        return base.TryGetGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface);
    }

    public override bool TryGetFamilyTypefaces(string familyName, [NotNullWhen(true)] out IReadOnlyList<Typeface>? familyTypefaces)
    {
        EnsureLoaded();
        return base.TryGetFamilyTypefaces(familyName, out familyTypefaces);
    }

    private void EnsureLoaded()
    {
        if (_loadAttempted)
        {
            return;
        }

        _loadAttempted = true;
        var bytes = s_preloaded ?? Fetch(File);
        s_preloaded = null;
        if (bytes.Length > 0)
        {
            TryAddGlyphTypeface(new MemoryStream(bytes), out _);
        }
    }

    /// <summary>Fetches the font ahead of the first text that needs it, without blocking the page.</summary>
    public static async Task PreloadAsync()
    {
        await FetchAhead(File);
        var bytes = TakeFetched(File);
        s_preloaded = bytes.Length > 0 ? bytes : null;
    }

    [JSImport("file", "a2ui-files")]
    private static partial byte[] Fetch(string path);

    [JSImport("fetchFile", "a2ui-files")]
    [return: JSMarshalAs<JSType.Promise<JSType.Void>>]
    private static partial Task FetchAhead(string path);

    [JSImport("takeFile", "a2ui-files")]
    private static partial byte[] TakeFetched(string path);
}

internal static class CjkFontAppBuilderExtensions
{
    /// <summary>Adds the Chinese font as the fallback for Han and CJK punctuation that Inter and Geist do not have.</summary>
    public static AppBuilder WithCjkFont(this AppBuilder builder) => builder
        .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CjkFontCollection()))
        .With(new FontManagerOptions
        {
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily(CjkFontCollection.Family),
                    UnicodeRange = UnicodeRange.Parse("U+2010-2027,U+2030-205E,U+2E80-2FDF,U+3000-303F,U+3400-4DBF,U+4E00-9FFF,U+F900-FAFF,U+FF00-FFEF")
                }
            ]
        });
}
