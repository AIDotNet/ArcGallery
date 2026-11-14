---
title: Month calendar
summary: A calendar app around a month, with event chips and bars packed into lanes and N more, a docked day panel with busy time, free gaps to block and event details, dragging to reschedule or plan across days, and upcoming, invitation and teammate views.
---

<Demo name="Preview" />

## When to use

`MonthCalendar` is a whole calendar screen: a sidebar, a month grid and a day panel. The grid shows every event of the
month at a glance; the panel shows the selected day in detail, with how much of it is booked, the free stretches you can
block for focus, and each event's details when you open it. People plan in place: drag an event to another day, drag
across empty days to plan a trip or a launch, answer invitations, and undo a move or a delete from the toast.

- **The month view of a calendar or scheduling app.** Load the calendars, the people and the events, and keep them in
  sync through the events the screen raises.
- **Team and content calendars.** Each calendar has its own color and icon and a toggle in the sidebar; teammates'
  months are one click away.
- **Planning across many events and multi-day stretches.** Trips, conferences and launch weeks show as bars across the
  days they cover, with the day's own events below them.

## When not to use

- **Hour by hour scheduling.** Use `WeekCalendar`, which lays events out on a time grid.
- **Picking one date in a form.** Use `Calendar` or `DatePicker`.
- **Project phases with dependencies.** Use `GanttTimeline`.

## Usage

The screen fills its container, so give it a height (a window, a page or 900 for a frame). Add the calendars, the people
and the events, and say who is signed in:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:MonthCalendar x:Name="Planner" Height="900" WorkspaceName="Tidewater" CurrentUserId="maya">
        <arc:MonthCalendar.Calendars>
            <arc:MonthCalendarSource Id="studio" Name="Studio" Tone="Teal" />
            <arc:MonthCalendarSource Id="home" Name="Home" Tone="Amber" />
        </arc:MonthCalendar.Calendars>
        <arc:MonthCalendar.People>
            <arc:MonthCalendarPerson Id="maya" Name="Maya Okafor" Email="maya@tidewater.studio" IsTeammate="False" />
            <arc:MonthCalendarPerson Id="jonah" Name="Jonah Reyes" Email="jonah@tidewater.studio" />
        </arc:MonthCalendar.People>
    </arc:MonthCalendar>
</UserControl>
```

```csharp
var day = new DateOnly(2026, 11, 12);
Planner.Events.Add(new MonthCalendarEvent("e1", "Daily sync", "studio", day, new TimeOnly(9, 15), new TimeOnly(9, 30))
{
    VideoProvider = "Video room",
    VideoUrl = "https://video.example/daily-sync",
});
Planner.Events.Add(new MonthCalendarEvent("e2", "Quarter planning", "studio", day) { EndDate = day.AddDays(1) });

Planner.EventCreated += (_, e) => api.Create(e.Event);
Planner.EventChanged += (_, e) => api.Update(e.Event);
Planner.EventDeleted += (_, e) => api.Delete(e.Event.Id);
```

### Events, calendars and people

A `MonthCalendarEvent` has an `Id`, a `Title`, a `CalendarId`, a `StartDate` and an optional inclusive `EndDate`. Without
a `StartTime` it lasts all day; with one it is a timed event that ends at `EndTime` (an hour later when it is not set).
All-day and multi-day events draw as tinted bars; timed events as chips with the time in the calendar's color. An event
can carry a `Location`, a video call (`VideoProvider` and `VideoUrl`), `Notes`, an `OrganizerId` and `Attendees`, each
a `MonthCalendarAttendee` with a person and a reply.

`Rsvp` is the signed-in person's reply. `Pending` marks an invitation: its chip is outlined, it is listed under
Invitations, and its details offer Going, Maybe and No. A declined event strikes its title and no longer counts as busy.

A `MonthCalendarSource` has a `Name`, a `Tone` (`Teal`, `Violet`, `Amber`, `Blue`, `Rose`, `Green`, `Neutral`, or
`Custom` with `CustomColor`) and an optional `Icon`, any 24 × 24 stroke glyph such as an `ArcIcons` icon; it defaults
to a calendar. `IsShown` is the sidebar toggle. People are `MonthCalendarPerson`s; the first five with `IsTeammate`
fill the sidebar's team calendars, and `CurrentUserId` names the signed-in one.

### The month

The grid shows the weeks the month spans, five or six, sharing the height. Each day packs its events into lanes: bars
first, the earlier and longer ones higher, then the timed events in time order. How many lanes fit follows the real row
height; when a day has more, its last lane reads "N more" and opens the day in the panel. A bar that runs into the next
week continues on the next row with square ends.

`Month` (any date in it) and `SelectedDate` are two-way and raise `MonthChanged` and `SelectedDateChanged`. Changing
the month cross-fades the grid with a 12 px slide in the direction you go. `FirstDayOfWeek` (Monday by default) and
`ShowWeekends` can also be switched from the account menu, and `Today` fixes the date treated as today.

Pick a day to show it in the panel, pick an event to open it, double-click a day to add an event. Drag an event 5 px to
pick it up: a card follows the pointer, the days it will cover light up, and dropping it moves it, keeping its times and
length. Drag across empty days to plan a span: the form opens with those days as an all-day event. Moves and deletes
show a toast with Undo for five seconds.

### The day panel

The panel shows the selected day: its title, how many events it has, the time booked and the time free, and a strip from
7 a.m. to 10 p.m. with a block per event and, today, a mark at the current time. Below are the all-day events and the
schedule, with the free stretches of at least `MinimumFreeGap` (an hour) between `WorkDayStart` and `WorkDayEnd` (9 to 6);
Block focus time fills one with a Focus time event. Invitations waiting for a reply count as busy but not as booked.

Opening an event shows its time, length, place and video link with a copy button, the reply (for invitations), the
guests and their replies, the notes and a calendar picker; the trash button deletes it. New event, N, the panel's footer
button and a double-click open the form: title, all day, start and end, location, a video link, guests and the calendar.
Saving raises `EventCreating`, where setting `ErrorMessage` keeps the form open with the message, and then
`EventCreated`.

From 1080 wide the panel docks beside the month and its left edge resizes it between 300 and 460 (`DayPanelWidth`);
the header button hides it (`IsDayPanelOpen`). Narrower, it is a sheet that opens when you pick a day or an event.

### Filtering and views

The header's filter dims the events whose titles do not match and counts the matches. In the sidebar each calendar
toggles its events and shows its count for the month; Only shows one calendar, and All brings the others back. Upcoming
lists the next four weeks by day; Invitations shows a card per invitation with Accept, Maybe, Decline and View in
calendar; a teammate's row shows their month. `View` and `TeammateId` follow these choices and can be set from code.

`Labels` takes a `MonthCalendarLabels` with every word, name and announcement, and the date patterns, to translate or
reword them; `Culture` formats the dates and times.

## API reference

<!-- api: MonthCalendar -->

<!-- api: MonthCalendarEvent -->

<!-- api: MonthCalendarSource -->

<!-- api: MonthCalendarPerson -->

<!-- api: MonthCalendarAttendee -->

<!-- api: MonthCalendarLabels -->

<!-- api: MonthCalendarEventEventArgs -->

<!-- api: MonthCalendarEventChangedEventArgs -->

<!-- api: MonthCalendarEventCreatingEventArgs -->

<!-- api: MonthCalendarRsvpChangedEventArgs -->

<!-- api: MonthCalendarDateChangedEventArgs -->

## Accessibility

- The month is a grid of rows and cells; each day button is named with its date, today, and its event count, and only
  the focused day is a Tab stop.
- Event chips are buttons named with the title, dates, times and the reply; "N more" says how many events it reveals and
  on which day.
- Calendar toggles in the sidebar read their calendar and count; the Only and All pills have their own names.
- Calendars are told apart by their icon and name as well as their color; invitations are outlined and declined events
  are struck through.
- Moves, deletes, replies and new events are announced in a polite live region and shown in the toast.

### Keyboard

| Keys | Action |
|---|---|
| Arrow keys | Move between days; leaving the grid turns the month |
| Enter or Space | Open the focused day in the panel |
| Page Up or Page Down | Previous or next month |
| T | Jump to today |
| N | New event on the selected day |
| / | Focus the event filter; Escape clears it |
| Alt + arrow keys | Move the focused event by a day or a week |
| Delete or Backspace | Delete the focused event, with Undo in the toast |
| Escape | Return the panel from an event or the form to the day |
| ? | Open the keyboard shortcuts |
| Ctrl + K or Cmd + K | Search the sidebar |

## Motion

| What | Full | Reduced | Off |
|---|---|---|---|
| Month change | Old grid fades out in 150 ms; new one fades in and slides 12 px in 180 ms; the title slides 6 px | Swaps | Swaps |
| Panel panes | Old pane fades out in 150 ms, new one in over 200 ms | Swaps | Swaps |
| Docked panel and sidebar width | Smooth spring (stiffness 300, damping 35); the panel fades in 150 ms | Jumps | Jumps |
| Sheets | Slide in 200 ms with the scrim | Slide in 200 ms | At once |
| Deleted and hidden events | Fade out in 150 ms where they were | Removed | Removed |
| A moved event | Fades in at its new day in 200 ms | Appears | Appears |
| Filter | Non-matching events dim to 28% in 150 ms | Dim in 150 ms | Dim at once |
| Answered invitation card | Fades in 135 ms while its height closes on the smooth spring | Removed | Removed |
| Toast | Rises 8 px and fades in over 200 ms; leaves in 150 ms | Fades | At once |

Other events change lanes at once, as in the reference.

## Responsive behavior

- From 1080 px the day panel sits beside the month; below that it is a sheet.
- Below 900 px the sidebar becomes a sheet opened from the menu button.
- At 760 px of main card or less the filter moves under the header.
- When the month's card is narrower than 560 px the month shows day numbers with a small load meter (one tick per event,
  up to four) and the selected day's list under it, and both scroll together. On a phone the header keeps the month,
  the arrows, a plus button and the panel button.

## Related

- `WeekCalendar`: a week on a time grid.
- `Calendar` and `DatePicker`: picking dates.
- `GanttTimeline`: phases and dependencies.
