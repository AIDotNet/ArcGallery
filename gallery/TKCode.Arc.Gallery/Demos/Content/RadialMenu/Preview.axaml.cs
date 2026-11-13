using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RadialMenuPreviewDemo : UserControl
{
    public RadialMenuPreviewDemo()
    {
        InitializeComponent();
        Tools.ItemInvoked += (_, e) => Status.Text = $"Tool: {e.Container.Header}";
    }
}
