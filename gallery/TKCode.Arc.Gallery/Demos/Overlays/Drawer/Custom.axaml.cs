using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class DrawerCustomDemo : UserControl
{
    public DrawerCustomDemo()
    {
        InitializeComponent();
    }

    private void OnPlayerClick(object? sender, RoutedEventArgs e) => PlayerDrawer.IsOpen = true;

    private void OnSheetClick(object? sender, RoutedEventArgs e) => SheetDrawer.IsOpen = true;
}
