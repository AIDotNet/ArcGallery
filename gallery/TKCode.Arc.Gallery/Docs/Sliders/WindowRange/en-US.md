---
title: Window range
summary: A movable window over a live area chart that you resize by its edges or fling to pan, with dates that roll as it moves.
---

<Demo name="Report" />

## When to use

`WindowRange` picks a stretch of a series, such as the days of a report, while the shape of the data stays in view. The
part inside the window is drawn in full color and the rest fades back, so people see what they include before they let go.

- **The date range of an analytics report**: sessions, sign-ups or revenue per day, with the totals above updating as the
  window moves.
- **Comparing periods**: slide a fixed-length window across usage or latency to find the busy weeks.
- **An overview strip** above a detailed chart or table that shows only the chosen stretch.

## When not to use

- **Exact dates picked from a calendar**, or a range that spans years: use `DateRangePicker`.
- **A window that zooms a large detailed chart** with events marked on it: use `BrushChart`.
- **A numeric range with no series behind it**: use `Slider` with `IsRange`, or `RangeHistogramSlider` when the values
  have a distribution worth showing.

## Usage

Give it the series in `Data`, one number per step, and say how to write a value with `ValueFormatter`. Values are in domain
units: by default `Minimum` is 0 and `Maximum` is the index of the last point, so a list of daily counts makes the domain
"day 0 to day N".

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:WindowRange Label="Report range"
                     Data="{Binding DailySessions}"
                     Start="{Binding From}"
                     End="{Binding To}"
                     MinSpan="6"
                     IsLive="True" />
</UserControl>
```

```csharp
var first = new DateTime(2026, 3, 2);
range.ValueFormatter = index => first.AddDays(index).ToString("MMM d", CultureInfo.InvariantCulture);
range.SpanFormatter = span => $"{span + 1:N0} days";
range.Ticks = [new WindowRangeTick(30, "Apr"), new WindowRangeTick(60, "May")];
```

`Start` and `End` are two-way. Left null, the window covers the last quarter of the domain. `WindowStart` and `WindowEnd`
always hold the window as it is drawn, after the step and the span limits. `ValueChanged` fires on every change, also
while a drag or a coast is running, so it is the place for live totals. `ValueCommitted` fires once the window has come
to rest after a drag, a flick or a key: fetch the report there.

`SpanFormatter` receives end minus start; add one for an inclusive count of days. Set `ShowSpan="False"` to drop the
middle readout in a tight column.

### Quick ranges

Buttons that set a range call `SetRange(start, end)`: both ends change at once, `ValueChanged` fires once, the frame glides
there and the readouts roll. The demo above marks a quick range as chosen only while the window is exactly that range.

### Limits and steps

Both ends snap to `Step`. `MinSpan` and `MaxSpan` limit the length of the window (by default one step and the whole
domain). Arrow keys move by `Step`; `LargeStep` (a tenth of the domain unless set) is used by Page Up, Page Down and Shift
with an arrow.

### Live data

Assign a new list to `Data` when the series changes: with the same number of points the line and the area reshape
smoothly, otherwise the chart is replaced. `IsLive` pulses a dot on the latest point; turn it on only while that point is
still changing.

## API reference

<!-- api: WindowRange -->

## Accessibility

- The control is a group named by `Label`. The start edge, the window and the end edge are three sliders, named
  "Report range, start", "Report range, window" and "Report range, end".
- The edges report the range they can reach, which respects `MinSpan` and `MaxSpan`, and speak their value through
  `ValueFormatter`. The window reports its start and speaks both ends, such as "Jun 2 to Jul 1".
- The chart, the frame and the readouts are decoration for assistive technology; the sliders carry every value.
- A press takes focus to the part it hit without a ring. Keyboard focus shows the focus ring: around the edge, or around
  the whole frame for the window.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Moves through the start edge, the window and the end edge |
| `Right` / `Up` | Moves the focused edge, or the whole window, by `Step` |
| `Left` / `Down` | Moves it back by `Step` |
| `Shift` + arrow, `PageUp` / `PageDown` | Moves by `LargeStep` |
| `Home` / `End` | Window: to the start or end of the domain. Edge: as far as the span limits allow |
| A key at a limit | The value stays; the frame bumps toward the key and springs back |

## Motion

- Dragging an edge or the window follows the pointer one to one. Past the end of the domain the window gives up to 18 px
  and an edge past its span limit up to 10 px, then they spring back.
- Releasing a pan keeps its speed: the window lands where the flick would carry it in about 0.2 s, snapped to the step,
  and decelerates there on a spring that starts at that speed. Grabbing it while it coasts stops it where it is.
- A press outside the window glides it to center on the pointer, and it keeps following while the pointer moves.
- Keys, quick ranges and new values glide the frame on the same spring as the coast. The readouts roll each changed digit,
  and a month name as one word, in the direction of the change; during a drag or a coast they change in place.
- While the window is held, a faint tint fills it and both grips stretch; a held edge stretches its own grip further.
- New data reshapes the line and the area on a soft spring, and the live dot sends out a ring every 2.4 s.
- With reduced motion the frame and the readouts jump to each value, with no coast, stretch, roll, reshape or pulse. With
  motion off everything is immediate.

## Responsive behavior

The plot and the frame fill the width they are given, and the readouts share one row with the length in the middle. The
control works from about 320 px; hide the length with `ShowSpan` in tighter spaces, and pass fewer ticks when the labels
would touch. The edges take presses 14 px beyond the handle, so they are easy to catch by touch, and the control needs no
hover.

## Related

| Component | Choose it when |
|---|---|
| `DateRangePicker` | People pick exact dates from a calendar |
| `BrushChart` | The window zooms a detailed chart |
| `RangeHistogramSlider` | A numeric range over a distribution |
| `Slider` | A range with no data to show |
