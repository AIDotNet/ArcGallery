using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TKCode.Arc.Gallery.Demos;

public partial class MonthCalendarPreviewDemo : UserControl
{
    public MonthCalendarPreviewDemo()
    {
        InitializeComponent();
        var today = DateOnly.FromDateTime(DateTime.Now);
        Calendar.WorkspaceName = MonthCalendarDemoData.Workspace;
        Calendar.CurrentUserId = MonthCalendarDemoData.Me;
        Calendar.Calendars.AddRange(MonthCalendarDemoData.CreateCalendars());
        Calendar.People.AddRange(MonthCalendarDemoData.CreatePeople());
        Calendar.Events.AddRange(MonthCalendarDemoData.CreateEvents(today));
        SizeChanged += (_, _) => Fit();
    }

    // A phone-wide preview (the gallery's Mobile view) shows the screen at its own width; otherwise the desktop screen is
    // scaled down like a 1440 × 900 frame.
    private void Fit()
    {
        var phone = Bounds.Width > 0 && Bounds.Width < 600;
        Stage.Stretch = phone ? Stretch.None : Stretch.Uniform;
        Calendar.Width = phone ? Bounds.Width : 1440;
        Calendar.Height = phone ? 844 : 900;
    }
}
