using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindRoseChartLiveUpdateDemo : UserControl
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1.6) };
    private readonly ObservableCollection<WindRoseRow> _rows = new();
    private int _month;
    private bool _paused;

    public WindRoseChartLiveUpdateDemo()
    {
        InitializeComponent();
        var first = WindRoseBinning.Bin(WindRoseDemoData.Month(0), 16);
        foreach (var row in first.Rows)
        {
            _rows.Add(row);
        }
        // The chart follows the collection: replacing its rows springs every band to the new month.
        Chart.ItemsSource = _rows;
        Chart.Calm = first.Calm;
        Month.Text = WindRoseDemoData.MonthNames[0];
        _timer.Tick += (_, _) => Advance();
        Toggle.Click += (_, _) =>
        {
            _paused = !_paused;
            Toggle.Content = _paused ? "Play" : "Pause";
            if (_paused)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }
        };
    }

    private void Advance()
    {
        _month = (_month + 1) % 12;
        var month = WindRoseBinning.Bin(WindRoseDemoData.Month(_month), 16);
        // Changes made in one go are read once, so the rose moves in a single spring.
        for (var i = 0; i < month.Rows.Count; i++)
        {
            _rows[i] = month.Rows[i];
        }
        Chart.Calm = month.Calm;
        Month.Text = WindRoseDemoData.MonthNames[_month];
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_paused)
        {
            _timer.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }
}
