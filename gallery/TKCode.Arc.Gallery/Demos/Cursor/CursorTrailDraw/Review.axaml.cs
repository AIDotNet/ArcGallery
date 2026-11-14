using System.Globalization;
using System.Linq;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class CursorTrailDrawReviewDemo : UserControl
{
    public CursorTrailDrawReviewDemo()
    {
        InitializeComponent();
    }

    // Where a real app would save the sketch: items come in board units (1000 wide), so they scale with any board.
    private void OnItemsChanged(object? sender, CursorTrailDrawItemsChangedEventArgs e)
    {
        var shapes = string.Join(", ", e.Items.Select(i => string.Format(CultureInfo.InvariantCulture,
            "{0} ({1}, {2} pts)", i.Shape.ToString().ToLowerInvariant(), i.InkId, i.Points.Count)));
        Log.Text = string.Format(CultureInfo.InvariantCulture, "Saved after {0}: {1} item{2}{3}",
            e.Change switch
            {
                CursorTrailDrawChange.Added => "a new stroke",
                CursorTrailDrawChange.Snapped => "a snap",
                CursorTrailDrawChange.KeptAsDrawn => "keep as drawn",
                CursorTrailDrawChange.Undone => "undo",
                _ => "clear",
            },
            e.Items.Count,
            e.Items.Count == 1 ? string.Empty : "s",
            e.Items.Count == 0 ? "." : " — " + shapes);
    }
}
