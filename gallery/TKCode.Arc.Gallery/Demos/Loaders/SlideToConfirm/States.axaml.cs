using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class SlideToConfirmStatesDemo : UserControl
{
    public SlideToConfirmStatesDemo()
    {
        InitializeComponent();
        Failing.ConfirmAction = UnlockAsync;
    }

    // A made-up request that never reaches the door.
    private static async Task UnlockAsync()
    {
        await Task.Delay(900);
        throw new InvalidOperationException("The door controller did not answer.");
    }
}
