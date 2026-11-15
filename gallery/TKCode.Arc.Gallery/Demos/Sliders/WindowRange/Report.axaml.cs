using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Motion;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindowRangeReportDemo : UserControl
{
    private static readonly DateTime s_firstDay = new(2026, 3, 2);
    private static readonly CultureInfo s_culture = CultureInfo.GetCultureInfo("en-US");

    private readonly double[] _sessions = SampleSessions();
    private readonly DispatcherTimer _live = new() { Interval = TimeSpan.FromMilliseconds(2250) };
    private readonly Random _random = new(11);
    private readonly MotionValue<double> _total;
    private IDisposable? _changeColor;
    private IDisposable? _iconColor;
    private int _lastSign;
    private bool _syncingPresets;

    public WindowRangeReportDemo()
    {
        InitializeComponent();
        _total = new MotionValue<double>(Total, 0, MotionVectors.Double);
        _total.Changed += value => Total.Text = Math.Round(value).ToString("N0", s_culture);
        var last = _sessions.Length - 1;
        Range.Data = (double[])_sessions.Clone();
        Range.ValueFormatter = index => s_firstDay.AddDays(index).ToString("MMM d", s_culture);
        Range.SpanFormatter = span => $"{span + 1:N0} days";
        Range.Ticks = MonthTicks(last);
        Range.Start = last - 29;
        Range.End = last;
        _live.Tick += (_, _) => AddLiveVisits();
        Card.GetObservable(BoundsProperty).Subscribe(new BoundsObserver(this));
        UpdateReport(animate: false);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _live.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _live.Stop();
    }

    /// <summary>Today's count keeps climbing while the page is open.</summary>
    private void AddLiveVisits()
    {
        _sessions[^1] += _random.Next(4, 46);
        Range.Data = (double[])_sessions.Clone();
        UpdateReport(animate: true);
    }

    private void OnRangeChanged(object? sender, WindowRangeValueChangedEventArgs e) => UpdateReport(animate: true);

    private void OnPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingPresets || Presets.SelectedValue is not string days)
        {
            return;
        }
        var length = int.Parse(days, CultureInfo.InvariantCulture);
        var last = _sessions.Length - 1;
        Range.SetRange(last - length + 1, last);
    }

    private void UpdateReport(bool animate)
    {
        var start = (int)Range.WindowStart;
        var end = (int)Range.WindowEnd;
        var length = end - start + 1;
        var total = Sum(start, end);
        if (animate)
        {
            _ = _total.AnimateTo(total, MotionSpec.Spring(SpringSpec.FromPhysical(150, 28)));
        }
        else
        {
            _total.Jump(total);
        }
        if (start - length < 0)
        {
            SetSign(0);
            Change.Text = "";
            Compare.Text = "Nothing earlier to compare with";
        }
        else
        {
            var previous = Sum(start - length, start - 1);
            var change = previous > 0 ? (total - previous) / previous : 0;
            SetSign(change >= 0 ? 1 : -1);
            var percent = Math.Abs(change).ToString("P1", s_culture).Replace(" ", "", StringComparison.Ordinal);
            Change.Text = (change >= 0 ? "+" : "−") + percent;
            Compare.Text = $"vs the previous {length:N0} days";
        }
        SyncPresets(start, end);
    }

    private void SetSign(int sign)
    {
        if (sign == _lastSign && TrendIcon.Data is not null)
        {
            return;
        }
        _lastSign = sign;
        TrendIcon.IsVisible = sign != 0;
        Change.IsVisible = sign != 0;
        if (sign == 0)
        {
            return;
        }
        TrendIcon.Data = sign > 0 ? ArcIcons.TrendingUp : ArcIcons.TrendingDown;
        _changeColor?.Dispose();
        _iconColor?.Dispose();
        var kind = sign > 0 ? ArcDesignTokenKind.Success : ArcDesignTokenKind.Danger;
        _changeColor = ArcTokens.Bind(Change, TextBlock.ForegroundProperty, kind);
        _iconColor = ArcTokens.Bind(TrendIcon, ArcGlyph.ForegroundProperty, kind);
    }

    /// <summary>A quick range shows as chosen only while the window is exactly the latest 7, 30 or 90 days.</summary>
    private void SyncPresets(int start, int end)
    {
        var last = _sessions.Length - 1;
        var length = end - start + 1;
        var match = end == last && length is 7 or 30 or 90 ? length.ToString(CultureInfo.InvariantCulture) : null;
        _syncingPresets = true;
        try
        {
            if (match is null)
            {
                Presets.SelectedIndex = -1;
            }
            else
            {
                Presets.SelectedValue = match;
            }
        }
        finally
        {
            _syncingPresets = false;
        }
    }

    private double Sum(int from, int to)
    {
        var sum = 0.0;
        for (var i = Math.Max(0, from); i <= Math.Min(_sessions.Length - 1, to); i++)
        {
            sum += _sessions[i];
        }
        return sum;
    }

    /// <summary>Narrow cards put the quick ranges under the total.</summary>
    private void Arrange(double width)
    {
        var narrow = width < 460;
        Grid.SetColumn(Presets, narrow ? 0 : 1);
        Grid.SetRow(Presets, narrow ? 1 : 0);
        Presets.HorizontalAlignment = narrow ? Avalonia.Layout.HorizontalAlignment.Left : Avalonia.Layout.HorizontalAlignment.Right;
        Presets.Margin = narrow ? new Thickness(0, 16, 0, 0) : default;
    }

    private static WindowRangeTick[] MonthTicks(int last)
    {
        var ticks = new List<WindowRangeTick>();
        for (var i = 1; i <= last; i++)
        {
            var day = s_firstDay.AddDays(i);
            if (day.Day == 1)
            {
                ticks.Add(new WindowRangeTick(i, day.ToString("MMM", s_culture)));
            }
        }
        return ticks.ToArray();
    }

    /// <summary>
    /// Six months of daily sessions for a small product: a slow climb, quiet weekends, a launch in early June that spikes
    /// and leaves a higher plateau, and today's count still partial.
    /// </summary>
    private static double[] SampleSessions()
    {
        var random = new Random(23);
        var values = new double[182];
        const int launch = 97;
        for (var i = 0; i < values.Length; i++)
        {
            var day = s_firstDay.AddDays(i).DayOfWeek;
            var weekday = day switch
            {
                DayOfWeek.Saturday => 0.7,
                DayOfWeek.Sunday => 0.66,
                DayOfWeek.Monday => 1.04,
                DayOfWeek.Friday => 0.94,
                _ => 1.0
            };
            var trend = 1500 + i * 4.2;
            var lift = i < launch ? 0 : 480 + 900 * Math.Exp(-(i - launch) / 2.2);
            var noise = 1 + (random.NextDouble() - 0.5) * 0.12;
            values[i] = Math.Round((trend + lift) * weekday * noise);
        }
        values[^1] = Math.Round(values[^1] * 0.34);
        return values;
    }

    private sealed class BoundsObserver(WindowRangeReportDemo owner) : IObserver<Rect>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Rect value) => owner.Arrange(value.Width);
    }
}
