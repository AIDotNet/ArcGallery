using Avalonia;

namespace TKCode.Arc.Gallery.Docs;

/// <summary>The editor a playground property gets.</summary>
public enum GalleryKnobKind
{
    /// <summary>A <c>bool</c>: a switch.</summary>
    Switch,

    /// <summary>An enum: segments (up to four members) or a select.</summary>
    Choice,

    /// <summary>An <c>int</c> or <c>double</c> with a range: a slider and a number box.</summary>
    Number,

    /// <summary>A <c>string</c>: a text box, or a box with suggestions when it has presets.</summary>
    Text
}

/// <summary>One enum member a <see cref="GalleryKnob"/> offers.</summary>
public sealed record GalleryKnobChoice(string Name, object Value);

/// <summary>
/// A property the panel next to a playground demo edits, as listed in the demo's <c>.knobs</c> file and completed by the
/// generator (editor, enum members, the property field as a function: no reflection).
/// </summary>
/// <param name="Name">The property name, as written in XAML.</param>
/// <param name="Property">The Avalonia property; a function so that reading the list loads no control's assembly.</param>
/// <param name="IsNullable">A <c>Nullable&lt;T&gt;</c> property: the panel offers "not set".</param>
/// <param name="Group">The heading the line sits under, or null.</param>
/// <param name="Owner">The Arc type that declares the property (the key prefix of its API translation), or null for an Avalonia base type.</param>
public sealed record GalleryKnob(string Name, GalleryKnobKind Kind, Func<AvaloniaProperty> Property, bool IsNullable, string? Group, string? Owner)
{
    public double Min { get; init; }

    public double Max { get; init; }

    public double Step { get; init; } = 1;

    /// <summary>The value is an <c>int</c>.</summary>
    public bool IsInteger { get; init; }

    public GalleryKnobChoice[] Choices { get; init; } = [];

    public string[] Presets { get; init; } = [];
}

/// <summary>
/// The panel's data for a demo with a <c>.knobs</c> file (design/engineering/gallery-playground.md).
/// </summary>
/// <param name="DemoId"><c>Component.Name</c>, the demo whose element <c>x:Name="Target"</c> the knobs change.</param>
/// <param name="Tag">The target's tag as the demo writes it, such as <c>arc:Button</c>.</param>
/// <param name="Attributes">The target's attributes as written (without <c>x:Name</c>): the starting point of the XAML the panel shows.</param>
/// <param name="Inner">The target's child elements as written (the options of a select, say), or null when it has none.</param>
public sealed record GalleryPlayground(string DemoId, string Tag, (string Name, string Value)[] Attributes, string? Inner, GalleryKnob[] Knobs);

/// <summary>The playgrounds the generator collected from the gallery's <c>Demos/**/*.knobs</c> files.</summary>
public static class GalleryPlaygrounds
{
    private static readonly Dictionary<string, GalleryPlayground> s_playgrounds =
        GalleryPlaygroundData.Playgrounds.ToDictionary(p => p.DemoId, StringComparer.Ordinal);

    /// <summary>Every playground, ordered by demo id.</summary>
    public static IReadOnlyList<GalleryPlayground> All => GalleryPlaygroundData.Playgrounds;

    /// <summary>The playground of the demo <c>Component.Name</c>, or null when it has none.</summary>
    public static GalleryPlayground? Get(string demoId) => s_playgrounds.GetValueOrDefault(demoId);
}
