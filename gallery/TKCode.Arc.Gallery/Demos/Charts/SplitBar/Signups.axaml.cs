using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class SplitBarSignupsDemo : UserControl
{
    // Signups per channel for each period, in the order of the bar's segments.
    private static readonly double[] s_week = [19670, 12100, 9350, 4720, 1640, 720];
    private static readonly double[] s_month = [84650, 46380, 48950, 26510, 4700, 2560];
    private static readonly double[] s_quarter = [277550, 174870, 124530, 56300, 21200, 7950];

    public SplitBarSignupsDemo()
    {
        InitializeComponent();
    }

    private void OnPeriodChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Bar is null)
        {
            return;
        }
        var values = (Period.SelectedValue as string) switch
        {
            "month" => s_month,
            "quarter" => s_quarter,
            _ => s_week
        };
        // The same segments get new values: the bar reflows in one motion, and parts switched off stay off.
        for (var i = 0; i < values.Length && i < Bar.Segments.Count; i++)
        {
            Bar.Segments[i].Value = values[i];
        }
    }
}
