using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace TKCode.Arc.Gallery.Generators;

/// <summary>One line of a <c>.knobs</c> file: a property of the playground's target the panel lets the visitor change.</summary>
internal sealed class KnobLine
{
    public KnobLine(string name, int line)
    {
        Name = name;
        Line = line;
    }

    public string Name { get; }

    /// <summary>1-based line number, for diagnostics.</summary>
    public int Line { get; }

    public double? Min { get; set; }

    public double? Max { get; set; }

    public double? Step { get; set; }

    /// <summary>The enum members to show (all of them when empty).</summary>
    public List<string> Only { get; } = new();

    /// <summary>Texts a string property offers as suggestions.</summary>
    public List<string> Presets { get; } = new();

    public string? Group { get; set; }

    /// <summary>Why the line cannot be used (null when it can).</summary>
    public string? Problem { get; set; }
}

/// <summary>
/// <c>Demos/&lt;Category&gt;/&lt;Component&gt;/&lt;Name&gt;.knobs</c>: the properties the playground panel next to the demo
/// <c>Component.Name</c> exposes, in order (design/engineering/gallery-playground.md section 4). One property per line:
/// <code>
/// # group: Appearance        a heading for the lines below
/// Variant                    a bool, an enum, a number or a string property of the demo's x:Name="Target" element
/// Variant only Primary, Ghost
/// Value 0..100 step 1        numbers need a range
/// Content presets "Save", "Delete"
/// </code>
/// </summary>
internal sealed class KnobFile
{
    public const string Folder = "Demos/";

    private static readonly Regex s_range = new(@"^(?<min>-?\d+(\.\d+)?)\.\.(?<max>-?\d+(\.\d+)?)", RegexOptions.CultureInvariant);
    private static readonly Regex s_step = new(@"\bstep\s+(?<step>\d+(\.\d+)?)", RegexOptions.CultureInvariant);
    private static readonly Regex s_only = new(@"\bonly\s+(?<list>[A-Za-z_][A-Za-z0-9_]*(\s*,\s*[A-Za-z_][A-Za-z0-9_]*)*)", RegexOptions.CultureInvariant);
    private static readonly Regex s_presets = new(@"\bpresets\s+(?<list>""(?:[^""\\]|\\.)*""(\s*,\s*""(?:[^""\\]|\\.)*"")*)", RegexOptions.CultureInvariant);
    private static readonly Regex s_quoted = new(@"""((?:[^""\\]|\\.)*)""", RegexOptions.CultureInvariant);

    private KnobFile(string path, string relativePath)
    {
        Path = path;
        RelativePath = relativePath;
    }

    public string Path { get; }

    /// <summary>Relative to the gallery project, with forward slashes.</summary>
    public string RelativePath { get; }

    public string Component { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>The demo's id, <c>Component.Name</c>.</summary>
    public string Id => Component + "." + Name;

    public List<KnobLine> Lines { get; } = new();

    /// <summary>Why the file cannot be used (null when it can).</summary>
    public string? Problem { get; private set; }

    public Location Location => Location.Create(Path, default, default);

    public static KnobFile? Parse(string path, string? text, string? projectDirectory)
    {
        var normalized = path.Replace('\\', '/');
        if (!normalized.EndsWith(".knobs", System.StringComparison.OrdinalIgnoreCase) || projectDirectory is not { Length: > 0 })
        {
            return null;
        }
        var root = projectDirectory!.Replace('\\', '/').TrimEnd('/') + "/";
        if (!normalized.StartsWith(root + Folder, System.StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        var file = new KnobFile(path, normalized.Substring(root.Length));
        var parts = file.RelativePath.Split('/');
        if (parts.Length != 4)
        {
            file.Problem = "knob files live in Demos/<Category>/<Component>/<Name>.knobs";
            return file;
        }
        file.Component = parts[2];
        file.Name = System.IO.Path.GetFileNameWithoutExtension(parts[3]);

        string? group = null;
        var lineNumber = 0;
        foreach (var raw in (text ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
        {
            lineNumber++;
            var line = raw.Trim();
            if (line.Length == 0)
            {
                continue;
            }
            if (line[0] == '#')
            {
                var comment = line.Substring(1).Trim();
                if (comment.StartsWith("group:", System.StringComparison.Ordinal))
                {
                    group = comment.Substring("group:".Length).Trim();
                }
                continue;
            }
            var split = line.IndexOfAny(new[] { ' ', '\t' });
            var name = split < 0 ? line : line.Substring(0, split);
            var rest = split < 0 ? string.Empty : line.Substring(split).Trim();
            var knob = new KnobLine(name, lineNumber) { Group = group };
            if (!IsIdentifier(name))
            {
                knob.Problem = "'" + name + "' is not a property name";
            }
            else
            {
                ParseOptions(knob, rest);
            }
            file.Lines.Add(knob);
        }
        return file;
    }

    private static void ParseOptions(KnobLine knob, string rest)
    {
        var range = s_range.Match(rest);
        if (range.Success)
        {
            knob.Min = Number(range.Groups["min"].Value);
            knob.Max = Number(range.Groups["max"].Value);
            rest = rest.Substring(range.Length).Trim();
        }
        var step = s_step.Match(rest);
        if (step.Success)
        {
            knob.Step = Number(step.Groups["step"].Value);
            rest = rest.Remove(step.Index, step.Length).Trim();
        }
        var only = s_only.Match(rest);
        if (only.Success)
        {
            knob.Only.AddRange(only.Groups["list"].Value.Split(',').Select(s => s.Trim()));
            rest = rest.Remove(only.Index, only.Length).Trim();
        }
        var presets = s_presets.Match(rest);
        if (presets.Success)
        {
            foreach (Match quoted in s_quoted.Matches(presets.Groups["list"].Value))
            {
                knob.Presets.Add(Regex.Unescape(quoted.Groups[1].Value));
            }
            rest = rest.Remove(presets.Index, presets.Length).Trim();
        }
        if (rest.Length > 0)
        {
            knob.Problem = "cannot read '" + rest + "' (expected min..max, step N, only A, B or presets \"a\", \"b\")";
        }
    }

    private static double Number(string text) => double.Parse(text, CultureInfo.InvariantCulture);

    private static bool IsIdentifier(string value)
        => value.Length > 0 && (char.IsLetter(value[0]) || value[0] == '_') && value.All(c => char.IsLetterOrDigit(c) || c == '_');
}
