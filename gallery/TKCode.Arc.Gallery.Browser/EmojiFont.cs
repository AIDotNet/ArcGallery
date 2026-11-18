using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace TKCode.Arc.Gallery.Browser;

/// <summary>
/// The emoji font of the WebAssembly host, for the same reason as the Chinese one (<see cref="CjkFontCollection"/>): a browser
/// gives Skia no system fonts, so a reaction or a party popper is a box. The font is the monochrome Noto Emoji (SIL OFL 1.1) cut
/// down to the emoji blocks (<c>tools/fonts/subset-noto-emoji.py</c>, about 0.7 MB): outlines in the text colour, not the colour
/// emoji of a phone. It is fetched, synchronously, the first time an emoji is drawn, so a visitor who never sees one never
/// downloads it.
/// </summary>
internal sealed partial class EmojiFontCollection : FontCollectionBase
{
    /// <summary>The key of the collection and the family inside it, as a font family string.</summary>
    public const string Family = "fonts:ArcEmoji#Noto Emoji";

    /// <summary>The code points that ask this font; keep in step with <c>tools/fonts/subset-noto-emoji.py</c>.</summary>
    public const string Ranges = "U+1F1E6-1F1FF,U+1F300-1F64F,U+1F680-1F6FF,U+1F900-1F9FF,U+2600-27BF,U+2B50,U+2B55,U+200D,U+FE0F";

    private const string File = "fonts/NotoEmoji-Subset.ttf";
    private static readonly Uri s_key = new("fonts:ArcEmoji");
    private bool _loadAttempted;

    public override Uri Key => s_key;

    public override bool TryGetGlyphTypeface(string familyName, FontStyle style, FontWeight weight, FontStretch stretch,
                                             [NotNullWhen(true)] out GlyphTypeface? glyphTypeface)
    {
        EnsureLoaded();
        return base.TryGetGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface);
    }

    public override bool TryGetFamilyTypefaces(string familyName, [NotNullWhen(true)] out System.Collections.Generic.IReadOnlyList<Typeface>? familyTypefaces)
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
        var bytes = Fetch(File);
        if (bytes.Length > 0)
        {
            TryAddGlyphTypeface(new MemoryStream(bytes), out _);
        }
    }

    /// <summary>The bytes of a file of the site, or none when it has no such file.</summary>
    [JSImport("file", "arc-lazy")]
    private static partial byte[] Fetch(string path);
}
