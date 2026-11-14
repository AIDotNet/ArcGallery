using Avalonia.Controls;
using Avalonia.Interactivity;
using TKCode.Arc;

namespace TKCode.Arc.Gallery.Demos;

public partial class PrizeWheelServerDemo : UserControl
{
    private int _serverDraw;

    public PrizeWheelServerDemo()
    {
        InitializeComponent();
        Draw();
        // The wheel asks for the winner when the visitor spins; here a stand-in for the answer your server sent.
        Wheel.WinnerPicker = _ => _serverDraw;
    }

    private void Draw()
    {
        // Stands in for a draw made on the server, weighted like the slices.
        var roll = Random.Shared.Next(100);
        _serverDraw = roll < 40 ? 0 : roll < 65 ? 1 : roll < 85 ? 2 : roll < 99 ? 3 : 4;
        Readout.Text = "The server has drawn a prize.";
    }

    private void OnSpinCompleted(object? sender, PrizeWheelResultEventArgs e) =>
        Readout.Text = $"Claimed: {e.Segment.Label}";

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        Wheel.ClearResult();
        Draw();
    }
}
