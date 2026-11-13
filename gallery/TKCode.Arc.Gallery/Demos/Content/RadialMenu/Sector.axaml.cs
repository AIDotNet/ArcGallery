using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RadialMenuSectorDemo : UserControl
{
    public RadialMenuSectorDemo()
    {
        InitializeComponent();
        Insert.ItemInvoked += (_, e) => Status.Text = $"Added: {e.Container.Header}";
    }
}
