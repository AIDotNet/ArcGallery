using Avalonia.Controls;
using Avalonia.Interactivity;
using TKCode.Arc;

namespace TKCode.Arc.Gallery.Demos;

public partial class PrizeWheelWheelOnlyDemo : UserControl
{
    private int _ticks;

    public PrizeWheelWheelOnlyDemo()
    {
        InitializeComponent();
    }

    private async void OnDraw(object? sender, RoutedEventArgs e)
    {
        Draw.IsEnabled = false;
        Readout.Text = "Drawing...";
        // Stands in for a request that returns the winner.
        await Task.Delay(400);
        _ticks = 0;
        Wheel.SpinTo(Random.Shared.Next(Wheel.Segments.Count));
    }

    private void OnTicked(object? sender, RoutedEventArgs e) => Readout.Text = $"{++_ticks} pegs";

    private void OnSpinCompleted(object? sender, PrizeWheelResultEventArgs e)
    {
        Readout.Text = $"Winner: {e.Segment.Label}";
        Draw.IsEnabled = true;
    }
}
