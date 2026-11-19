using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RibbonContextualDemo : UserControl
{
    public RibbonContextualDemo()
    {
        InitializeComponent();
        Selected.IsCheckedChanged += (_, _) =>
        {
            var on = Selected.IsChecked == true;
            NodeTools.IsVisible = on;
            if (on)
            {
                MainRibbon.SelectedTab = NodeTools;
            }
        };
    }
}
