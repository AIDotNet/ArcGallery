using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace TKCode.Arc.Gallery.Android;

/// <summary>
/// The Chinese font of the Android app: Noto Sans SC Regular (SIL OFL 1.1) cut down to the characters the gallery uses plus the
/// 3755 common ones, the file the WebAssembly host serves (<c>tools/fonts/subset-noto-sans-sc.py</c>, about 1 MB), kept as a
/// resource of the app. The phone's own Chinese font is a font collection (<c>NotoSansCJK-Regular.ttc</c>) that Skia on Android
/// cannot make a glyph typeface from: the text layout asks for it for the first Han character and throws "Could not create
/// glyphTypeface. Font family: Noto Sans CJK SC". A fallback of the app's own is asked before the system's.
/// </summary>
internal static class CjkFont
{
    /// <summary>The key of the collection and the family inside it, as a font family string.</summary>
    private const string Family = "fonts:ArcCjk#Noto Sans SC";

    /// <summary>
    /// Adds the Chinese font as the fallback for Han and CJK punctuation: text in Inter or Geist that has no glyph for a
    /// character asks it. Pair it with <c>WithArcFonts()</c>.
    /// </summary>
    public static AppBuilder WithCjkFont(this AppBuilder builder) => builder
        .ConfigureFonts(fontManager => fontManager.AddFontCollection(
            new EmbeddedFontCollection(new Uri("fonts:ArcCjk"), new Uri("avares://TKCode.Arc.Gallery.Android/Fonts"))))
        .With(new FontManagerOptions
        {
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily(Family),
                    UnicodeRange = UnicodeRange.Parse("U+2010-2027,U+2030-205E,U+2E80-2FDF,U+3000-303F,U+3400-4DBF,U+4E00-9FFF,U+F900-FAFF,U+FF00-FFEF")
                }
            ]
        });
}
