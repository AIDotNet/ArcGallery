using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class CelebrateButtonPreviewDemo : UserControl
{
    public CelebrateButtonPreviewDemo()
    {
        InitializeComponent();
        // A made-up release: 0.9 s of work, then the confetti.
        Ship.Action = () => Task.Delay(900);
    }
}
