using System.Globalization;
using System.Text;

namespace TKCode.Arc.Gallery.Docs;

/// <summary>
/// The XAML the playground panel shows for the demo's target: the element as the demo writes it, with the panel's
/// properties as they are now. A property equal to its default is left out, so the text is as short as it can be
/// (design/engineering/gallery-playground.md section 6). Plain string building; nothing is looked up by name.
/// </summary>
public static class GalleryPlaygroundSnippet
{
    private const int WrapAt = 44;

    /// <param name="current">The value of a knob's property now.</param>
    /// <param name="defaultOf">The default of a knob's property, left out of the text when the property has it.</param>
    public static string Build(GalleryPlayground playground, Func<GalleryKnob, object?> current, Func<GalleryKnob, object?> defaultOf)
    {
        var attributes = playground.Attributes.Select(a => (a.Name, a.Value)).ToList();
        foreach (var knob in playground.Knobs)
        {
            var value = current(knob);
            var index = attributes.FindIndex(a => a.Name == knob.Name);
            if (value is null || Equals(value, defaultOf(knob)) || Format(knob, value) is not { } text)
            {
                if (index >= 0)
                {
                    attributes.RemoveAt(index);
                }
            }
            else if (index >= 0)
            {
                attributes[index] = (knob.Name, text);
            }
            else
            {
                attributes.Add((knob.Name, text));
            }
        }
        return Write(playground.Tag, attributes, playground.Inner);
    }

    /// <summary>The XAML text of a knob's value, or null when it has none (an empty string).</summary>
    public static string? Format(GalleryKnob knob, object value)
    {
        switch (knob.Kind)
        {
            case GalleryKnobKind.Switch:
                return value is true ? "True" : "False";
            case GalleryKnobKind.Choice:
                foreach (var choice in knob.Choices)
                {
                    if (Equals(choice.Value, value))
                    {
                        return choice.Name;
                    }
                }
                return value.ToString();
            case GalleryKnobKind.Number:
                return Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("0.######", CultureInfo.InvariantCulture);
            default:
                var text = value.ToString();
                return string.IsNullOrEmpty(text) ? null : text;
        }
    }

    private static string Write(string tag, List<(string Name, string Value)> attributes, string? inner)
    {
        var close = string.IsNullOrEmpty(inner) ? " />" : ">";
        var inline = new StringBuilder("<").Append(tag);
        foreach (var (name, value) in attributes)
        {
            inline.Append(' ').Append(name).Append("=\"").Append(Escape(value)).Append('"');
        }
        inline.Append(close);
        var head = inline.ToString();
        if (inline.Length > WrapAt && attributes.Count >= 2)
        {
            var wrapped = new StringBuilder("<").Append(tag);
            foreach (var (name, value) in attributes)
            {
                wrapped.Append("\n    ").Append(name).Append("=\"").Append(Escape(value)).Append('"');
            }
            head = wrapped.Append(close).ToString();
        }
        if (string.IsNullOrEmpty(inner))
        {
            return head;
        }
        var body = string.Join("\n", inner!.Split('\n').Select(line => "    " + line));
        return head + "\n" + body + "\n</" + tag + ">";
    }

    private static string Escape(string value)
        => value.Replace("&", "&amp;", StringComparison.Ordinal).Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal).Replace(">", "&gt;", StringComparison.Ordinal);
}
