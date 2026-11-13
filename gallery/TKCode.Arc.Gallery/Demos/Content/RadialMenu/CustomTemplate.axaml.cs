using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace TKCode.Arc.Gallery.Demos;

public partial class RadialMenuCustomTemplateDemo : UserControl
{
    public RadialMenuCustomTemplateDemo()
    {
        InitializeComponent();
        RadialMenuSwatch[] swatches =
        [
            new("Coral", "#F2665A"), new("Amber", "#F5A524"), new("Lime", "#7CC23D"), new("Teal", "#14A39A"),
            new("Sky", "#2E9BEA"), new("Indigo", "#5B5BD6"), new("Violet", "#9A55E0"), new("Rose", "#E5487D")
        ];
        // The swatches have no text of their own: the selector names them for the centre label and screen readers.
        Swatches.TextSelector = item => (item as RadialMenuSwatch)?.Name;
        Swatches.ItemsSource = swatches;
        Swatches.SelectedItem = swatches[4];
        Swatches.ItemInvoked += (_, e) =>
        {
            var swatch = (RadialMenuSwatch)e.Item!;
            Chip.Fill = swatch.Brush;
            ColorName.Text = swatch.Name;
        };
    }
}

/// <summary>A named color shown as a swatch in the radial menu.</summary>
public sealed class RadialMenuSwatch(string name, string color)
{
    public string Name { get; } = name;

    public IBrush Brush { get; } = new ImmutableSolidColorBrush(Color.Parse(color));
}
