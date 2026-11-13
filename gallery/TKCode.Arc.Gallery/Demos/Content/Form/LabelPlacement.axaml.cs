using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormLabelPlacementDemo : UserControl
{
    public FormLabelPlacementDemo()
    {
        InitializeComponent();
        Plans.ItemsSource = new[] { "Starter", "Team", "Enterprise" };
        DataContext = new FormProfileModel();
        Placement.SelectionChanged += (_, _) =>
            Profile.LabelPlacement = Placement.SelectedValue as string == "Top" ? FormLabelPlacement.Top : FormLabelPlacement.Left;
    }
}
