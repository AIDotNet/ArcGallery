using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormErrorPlacementDemo : UserControl
{
    public FormErrorPlacementDemo()
    {
        InitializeComponent();
        DataContext = new FormProfileModel();
        Placement.SelectionChanged += (_, _) => Dense.ErrorPlacement = Placement.SelectedValue switch
        {
            "Inline" => FormErrorPlacement.Inline,
            "None" => FormErrorPlacement.None,
            _ => FormErrorPlacement.Tooltip
        };
    }
}
