using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class ArmingSwitchPreviewDemo : UserControl
{
    public ArmingSwitchPreviewDemo()
    {
        InitializeComponent();
        // Nothing ships: the "rollout" is 1.4 s of waiting that always succeeds.
        Switch.ConfirmAction = async () =>
        {
            await Task.Delay(1400);
            return true;
        };
    }
}
