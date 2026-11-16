using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class SlideToConfirmAccentDemo : UserControl
{
    public SlideToConfirmAccentDemo()
    {
        InitializeComponent();
    }

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        Green.Reset();
        Violet.Reset();
    }
}
