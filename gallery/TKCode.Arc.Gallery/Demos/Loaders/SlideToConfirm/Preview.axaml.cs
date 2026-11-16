using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class SlideToConfirmPreviewDemo : UserControl
{
    public SlideToConfirmPreviewDemo()
    {
        InitializeComponent();
        // A made-up payment: 1.1 s of work, then the card resets 2.6 s after the check shows.
        Slider.ConfirmAction = () => Task.Delay(1100);
        Slider.ResetAfter = TimeSpan.FromMilliseconds(2600);
    }
}
