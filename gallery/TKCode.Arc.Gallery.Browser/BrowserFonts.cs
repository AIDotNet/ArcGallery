using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace TKCode.Arc.Gallery.Browser;

public static class BrowserFontsAppBuilderExtensions
{
    /// <summary>
    /// Adds the fonts a browser lacks as the fallbacks of Inter and Geist: the Chinese font for Han and CJK punctuation
    /// (<see cref="CjkFontCollection"/>) and the emoji font for emoji (<see cref="EmojiFontCollection"/>), each fetched when
    /// the first character that needs it is drawn. Pair it with <c>WithArcFonts()</c>. The font manager takes one list of
    /// fallbacks, so both are set here.
    /// </summary>
    public static AppBuilder WithBrowserFonts(this AppBuilder builder) => builder
        .ConfigureFonts(fontManager =>
        {
            fontManager.AddFontCollection(new CjkFontCollection());
            fontManager.AddFontCollection(new EmojiFontCollection());
        })
        .With(new FontManagerOptions
        {
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily(CjkFontCollection.Family),
                    UnicodeRange = UnicodeRange.Parse("U+2010-2027,U+2030-205E,U+2E80-2FDF,U+3000-303F,U+3400-4DBF,U+4E00-9FFF,U+F900-FAFF,U+FF00-FFEF")
                },
                new FontFallback
                {
                    FontFamily = new FontFamily(EmojiFontCollection.Family),
                    UnicodeRange = UnicodeRange.Parse(EmojiFontCollection.Ranges)
                }
            ]
        });
}
