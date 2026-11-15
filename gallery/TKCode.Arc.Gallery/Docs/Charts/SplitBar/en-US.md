---
title: Split bar
summary: A single bar that adds up to 100%: point at a part to widen it and read its figures, switch parts off from the legend and the others close up, with labels inside the parts or in a lane underneath.
---

<Demo name="Signups" />

## When to use

`SplitBar` shows what one total is made of, in a single row that fits a card or the top of a dashboard. Each part is
as wide as its share. Resting the pointer on a part (or reaching it with the arrow keys) widens it a little, enough to
show its own name when it is small, while the other parts make room and fade back; the readout above the bar then
gives its share and its exact value. The legend under the bar switches parts off and on: a part that is off closes up
where it was and the others spread out to fill the bar, their percentages counting to the new shares. New values for
the same parts reflow the bar in one motion, so switching a period or a filter reads as a change, not a redraw.

- **Where traffic or signups come from**: channels, sources, campaigns.
- **Budgets and costs**: spending by category, with the exact amount on demand.
- **Storage and quotas**: space by file type against the size of the drive.
- **Survey answers and mixes**: devices, plans, regions, two to eight categories.

## When not to use

- **Comparing absolute values across many categories**: a `BarChart` puts each value on its own bar.
- **The total is the headline**: a `DonutChart` keeps the total in the middle; a `WaffleChart` counts units.
- **Shares over time**: when how each share moves matters more than today's split, use a stacked chart over time.
- **One goal**: a `Progress` bar says "how far" more plainly than a split.

## Usage

Add `SplitBarSegment` items with a `Label` and a `Value`, in the order they should appear. Shares are worked out over
the parts that are on; zero and negative values take no room. Set `Unit` (or `ValuePrefix`, `ValueDecimals` and
`ValueSuffix`) so the readout gives the exact figure, not only the percentage.

```xml
<arc:SplitBar Title="Signups by channel" Description="New accounts on the web app" Unit="signups">
    <arc:SplitBarSegment Label="Newsletter" Value="19670" />
    <arc:SplitBarSegment Label="Partners" Value="12100" />
    <arc:SplitBarSegment Label="Search ads" Value="9350" />
    <arc:SplitBarSegment Label="Unknown" Value="720" />
</arc:SplitBar>
```

The first four parts take the chart series colors; the fifth and sixth take two neutrals, meant for catch-all parts
such as "Other", so put those last. Give a segment a `Brush` when its color carries meaning.

### New data

Change the `Value` of the same segments (or bind `ItemsSource` to a collection and change the values in place) and the
bar reflows from wherever its edges are, even in the middle of a move. The demo above switches between a week, a month
and a quarter this way; parts that are switched off stay off.

```csharp
for (var i = 0; i < values.Length; i++)
{
    Bar.Segments[i].Value = values[i];
}
```

### Switching parts off

Each legend entry is a toggle. A part that is off shows a hollow swatch, a struck-through name and the word Off; it
takes no room in the bar and the readout's total leaves it out. `SplitBarSegment.IsHidden` holds the state (bind it to
keep a filter in your view model) and `HiddenChanged` reports every change. The last part that is on stays on: its
entry gives a short shake and screen readers hear why.

### Highlight and readout

The highlighted part is `HighlightedIndex` (-1 for none); pointing at a part or its legend entry sets it and leaving
clears it. The readout swaps between `TotalLabel` with the total and the highlighted part's color, name, share and
value. A click on a part raises `SegmentInvoked`; Enter or Space on the focused bar raises it too and then, unless a
handler marks it handled, switches the part off.

### Labels inside and in the lane

A part shows its name and share inside when they fit, its share alone when only that fits, and otherwise moves its
label to a lane under the bar, joined to the part by a curved leader line. Lane labels are pushed apart so they never
overlap. The fit is worked out again whenever the bar's width changes, so a wide card shows more labels inside than a
phone. Keep labels short: long names only fit inside wide parts.

### A fixed total

With a `Total` larger than the sum, the parts keep their share of that total and the rest of the bar stays empty; the
legend names it `RemainderLabel` ("Remaining" by default, null to leave it out) and the readout adds "of" the total.

<Demo name="Storage" />

## API reference

<!-- api: SplitBar -->

<!-- api: SplitBarSegment -->

<!-- api: SplitBarSegmentEventArgs -->

<!-- api: SplitBarHiddenChangedEventArgs -->

## Accessibility

- The bar is a group described as a split bar and named with every share ("Signups by channel: Newsletter 40.8%,
  Partners 25.1%, ... Use the arrow keys to read each share."), followed by the parts that are off.
- Moving through the parts reads each one in a polite live region ("Partners, 25.1%, 12,100 signups."); switching a
  part off or on is read too.
- Legend entries are toggle buttons named with the part and its share, or "hidden". A part that is off is also shown
  without color: a hollow swatch, a struck-through name and the word Off.
- The labels on the bar, the lane and the rolling digits are hidden from screen readers; the bar's name and the
  readout carry the same numbers as text. Every part is named in the legend, so color is never the only key.

### Keyboard

| Keys | Action |
|---|---|
| Tab | Focuses the bar, then each legend entry |
| Right / Down | On the bar: highlights the next part that is on, widening it and reading it out (wraps round) |
| Left / Up | On the bar: highlights the previous part that is on |
| Home / End | On the bar: highlights the first or the last part that is on |
| Enter / Space | On the bar: switches the highlighted part off. On a legend entry: switches its part off or back on |
| Escape | Clears the highlight |

## Motion

| Change | Full | Reduced | Off |
|---|---|---|---|
| New shares, a part switched off or on | Every edge springs to its place (about 0.4 s, no overshoot, keeps its speed when interrupted) | Jumps | Jumps |
| Highlight | The part widens on the same spring; the others fade to half over 0.2 s and its fill brightens | At once | At once |
| Inside label | The name opens or closes beside the share as the part grows or shrinks | At once | At once |
| Lane labels | Glide to their places and fade in or out; the lane opens and closes smoothly | At once | At once |
| Percentages, values, total | Digits roll in the direction they changed | Change in place | Change in place |
| Readout | The old line rises out with a little blur while the new one comes up | Swaps | Swaps |
| Last part refused | The legend entry shakes | Nothing moves | Nothing moves |

The first data shows at once. The frame loop runs only while something moves.

## Responsive behavior

- The bar fills its width and fits its labels to it: the same data shows more labels inside on a wide card and moves
  more of them to the lane on a phone.
- The readout sits on the right of the title and wraps under it on narrow screens; the legend wraps onto as many lines
  as it needs.
- On touch screens a tap on a part highlights it and a tap elsewhere clears it.

## Related

| Component | Choose it when |
|---|---|
| `DonutChart` | The total in the middle is the headline |
| `WaffleChart` | You count units out of a hundred |
| `BarChart` | You compare absolute values across many categories |
| `Progress` | You show how far one task has come |
