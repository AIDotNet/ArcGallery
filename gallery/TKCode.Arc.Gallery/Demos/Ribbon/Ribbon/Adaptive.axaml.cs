using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RibbonAdaptiveDemo : UserControl
{
    public RibbonAdaptiveDemo()
    {
        InitializeComponent();
        WidthSlider.ValueChanged += (_, e) => Frame.Width = e.NewValue;
    }
}
