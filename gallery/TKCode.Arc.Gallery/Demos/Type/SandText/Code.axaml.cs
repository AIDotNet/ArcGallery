using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class SandTextCodeDemo : UserControl
{
    public SandTextCodeDemo()
    {
        InitializeComponent();
    }

    private void OnLetGo(object? sender, RoutedEventArgs e)
    {
        Status.Text = "Falling...";
        Sand.Dissolve();
    }

    private void OnPourBack(object? sender, RoutedEventArgs e)
    {
        Status.Text = "Pouring...";
        Sand.Gather();
    }

    private void OnDissolved(object? sender, RoutedEventArgs e) => Status.Text = "Gone";

    private void OnGathered(object? sender, RoutedEventArgs e) => Status.Text = "Standing";
}
