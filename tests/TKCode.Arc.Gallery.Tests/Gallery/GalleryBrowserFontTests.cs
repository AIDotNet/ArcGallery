using System.Buffers.Binary;
using System.Text;

namespace TKCode.Arc.Tests.Gallery;

/// <summary>
/// The Chinese and emoji fonts of the WebAssembly host (gallery/TKCode.Arc.Gallery.Browser/wwwroot/fonts): a browser has no system
/// fonts for Skia, so a character the fonts lack is drawn as a box.
/// </summary>
public class GalleryBrowserFontTests
{
    [Fact]
    public void Every_Chinese_Character_The_Gallery_Draws_Is_In_The_Browser_Font()
    {
        var root = RepositoryPaths.Gallery;
        var font = File.ReadAllBytes(Path.Combine(root, "gallery", "TKCode.Arc.Gallery.Browser", "wwwroot", "fonts", "NotoSansSC-Subset.otf"));
        var covered = Codepoints(font);
        covered.ShouldContain(0x4E2D, "the font has no Chinese at all");

        var missing = new SortedDictionary<int, string>();
        foreach (var path in TextFiles())
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var drawn = c is >= '㐀' and <= '鿿' or >= '豈' and <= '﫿' or >= '　' and <= '〿' or >= '＀' and <= '￯';
                if (drawn && !covered.Contains(c))
                {
                    missing.TryAdd(c, path.StartsWith(RepositoryPaths.Gallery, StringComparison.Ordinal) ? Path.GetRelativePath(RepositoryPaths.Gallery, path) : Path.GetRelativePath(RepositoryPaths.Arc, path));
                }
            }
        }
        // Run: /tmp/ftvenv/bin/python tools/fonts/subset-noto-sans-sc.py (see the script for the setup)
        missing.ShouldBeEmpty("characters the browser font lacks (a box in the WebAssembly host); run tools/fonts/subset-noto-sans-sc.py:\n" +
                              string.Join("\n", missing.Select(m => $"{(char)m.Key} U+{m.Key:X4} in {m.Value}")));
    }

    [Fact]
    public void Every_Emoji_The_Gallery_Draws_Is_In_The_Browser_Emoji_Font()
    {
        var font = File.ReadAllBytes(Path.Combine(RepositoryPaths.Gallery, "gallery", "TKCode.Arc.Gallery.Browser", "wwwroot", "fonts", "NotoEmoji-Subset.ttf"));
        var covered = Codepoints(font);
        covered.ShouldContain(0x1F44D, "the font has no emoji at all");

        // The demos' data and the libraries' pickers: a reaction is a string in C# or AXAML, so look at the code as well as the texts.
        var missing = new SortedDictionary<int, string>();
        foreach (var folder in new[] { Path.Combine(RepositoryPaths.Gallery, "gallery"), Path.Combine(RepositoryPaths.Arc, "src") })
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                var normalized = path.Replace('\\', '/');
                if (normalized.Contains("/obj/") || normalized.Contains("/bin/") || normalized.Contains("/GeneratedFiles/") ||
                    !(path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".axaml", StringComparison.Ordinal) ||
                      path.EndsWith(".md", StringComparison.Ordinal) || path.EndsWith(".xlf", StringComparison.Ordinal)))
                {
                    continue;
                }
                var text = File.ReadAllText(path, Encoding.UTF8);
                for (var i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
                {
                    var code = char.ConvertToUtf32(text, i);
                    // The emoji blocks of the supplementary planes, which no other font of the host has.
                    if (code is >= 0x1F1E6 and <= 0x1F64F or >= 0x1F680 and <= 0x1F6FF or >= 0x1F900 and <= 0x1F9FF && !covered.Contains(code))
                    {
                        missing.TryAdd(code, Path.GetFileName(path));
                    }
                }
            }
        }
        missing.ShouldBeEmpty("emoji the browser font lacks (a box in the WebAssembly host); run tools/fonts/subset-noto-emoji.py:\n" +
                              string.Join("\n", missing.Select(m => $"{char.ConvertFromUtf32(m.Key)} U+{m.Key:X} in {m.Value}")));
    }

    /// <summary>The Markdown and XLIFF texts of the gallery, A2UI Chat and the libraries, the catalogs that hold Chinese titles and A2UI Chat's interface strings.</summary>
    private static IEnumerable<string> TextFiles()
    {
        foreach (var folder in new[] { Path.Combine(RepositoryPaths.Gallery, "gallery"), Path.Combine(RepositoryPaths.Gallery, "a2ui"), Path.Combine(RepositoryPaths.Arc, "src") })
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                var normalized = path.Replace('\\', '/');
                if (normalized.Contains("/obj/") || normalized.Contains("/bin/") || normalized.Contains("/GeneratedFiles/"))
                {
                    continue;
                }
                if (path.EndsWith(".md", StringComparison.Ordinal) || path.EndsWith(".xlf", StringComparison.Ordinal) ||
                    (Path.GetFileName(path).Contains("Catalog", StringComparison.Ordinal) || path.EndsWith("Strings.cs", StringComparison.Ordinal))
                    && path.EndsWith(".cs", StringComparison.Ordinal))
                {
                    yield return path;
                }
            }
        }
    }

    /// <summary>The code points of an OpenType font's character map (formats 4 and 12).</summary>
    private static HashSet<int> Codepoints(byte[] font)
    {
        ushort U16(int at) => BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(at));
        uint U32(int at) => BinaryPrimitives.ReadUInt32BigEndian(font.AsSpan(at));

        var tables = U16(4);
        var cmap = -1;
        for (var i = 0; i < tables; i++)
        {
            var entry = 12 + 16 * i;
            if (Encoding.ASCII.GetString(font, entry, 4) == "cmap")
            {
                cmap = (int)U32(entry + 8);
            }
        }
        cmap.ShouldBeGreaterThan(0, "the font has no cmap table");

        var result = new HashSet<int>();
        var subtables = U16(cmap + 2);
        for (var i = 0; i < subtables; i++)
        {
            var start = cmap + (int)U32(cmap + 4 + 8 * i + 4);
            switch (U16(start))
            {
                case 4:
                    var segments = U16(start + 6) / 2;
                    var ends = start + 14;
                    var starts = ends + segments * 2 + 2;
                    for (var s = 0; s < segments; s++)
                    {
                        for (int code = U16(starts + 2 * s), end = U16(ends + 2 * s); code <= end && code < 0xFFFF; code++)
                        {
                            result.Add(code);
                        }
                    }
                    break;
                case 12:
                    var groups = (int)U32(start + 12);
                    for (var g = 0; g < groups; g++)
                    {
                        var group = start + 16 + 12 * g;
                        for (var code = U32(group); code <= U32(group + 4); code++)
                        {
                            result.Add((int)code);
                        }
                    }
                    break;
            }
        }
        return result;
    }
}
