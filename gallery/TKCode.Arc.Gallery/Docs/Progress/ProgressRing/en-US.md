---
title: Progress ring
summary: A circular progress and loading indicator supporting determinate progress and indeterminate spinning states.
---

<Demo name="Overview" />

## When to use

`ProgressRing` communicates ongoing activity or measurable completion in a compact circular footprint. Use it when a
horizontal bar would take too much width or when the task has no known total and needs a clear loading indicator.

- **Indeterminate loading states**: set `IsIndeterminate="True"` while fetching data, refreshing a view, or waiting
  for a background action inside a card, dialog or toolbar.
- **Compact determinate progress**: uploads, downloads, exports or sync tasks where a circular readout fits beside an
  item or centered in an empty state.
- **Inline activity markers**: use `Size="Small"` next to a status label or inside a table cell to indicate work in
  progress.

## When not to use

- **Full-width tasks in a form or list**: use `Progress` when horizontal space is available and a linear bar with a
  left-aligned label reads more clearly.
- **Page or card content placeholders**: use `Skeleton` to preserve the layout structure while content loads.
- **Dashboard KPI gauges with threshold bands**: use `Gauge` for a 270-degree metric dial with warning and danger
  thresholds.

## Usage

By default `ProgressRing` is determinate (`IsIndeterminate="False"`). Set `Value`, `Minimum` (default `0`) and
`Maximum` (default `100`). Set `IsIndeterminate="True"` for a continuous loading spinner.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <StackPanel Orientation="Horizontal" Spacing="24">
        <arc:ProgressRing IsIndeterminate="True" Label="Loading" />
        <arc:ProgressRing Value="68" IsValueVisible="True" Label="Uploading" />
    </StackPanel>
</UserControl>
```

### Sizes

Choose `ControlSize.Small` (20 px), `ControlSize.Medium` (40 px, default) or `ControlSize.Large` (64 px), or set
explicit `Width`, `Height` and `Thickness` values.

<Demo name="Sizes" />

### Determinate progress

One smooth spring drives both the clockwise arc and the center percentage. When `Value` reaches `Maximum`, the ring
turns to the success color and replaces the percentage with a completion checkmark when `IsValueVisible` is enabled.

<Demo name="Determinate" />

### Custom center content

Set `Content` (and optionally `ContentTemplate`) to place custom text, counts or icons inside the ring. Custom content
takes precedence over the built-in percentage readout.

<Demo name="CustomContent" />

## API reference

<!-- api: ProgressRing -->

## Accessibility

- `ProgressRing` is read-only and does not take keyboard focus (`Focusable="False"`).
- Its automation control type is `ProgressBar`.
- `Label` provides the accessible name; without a label it defaults to `"Loading"` in indeterminate mode and
  `"Progress"` in determinate mode.
- `AutomationProperties.ItemStatus` reports `"Loading"` when indeterminate and the current percentage (for example
  `"68%"`) when determinate.

## Motion

- **Determinate**: one smooth spring (`ArcSpring.Smooth`) moves the arc and percentage together; at 100% the arc
  transitions to `Success` after 200 ms and the completion check settles in.
- **Indeterminate**: a `LiveLoop`-driven comet arc rotates continuously while visible and in view, pausing off-screen.
- **Reduced motion**: determinate values jump immediately; indeterminate rings rest and breathe softly in opacity.
- **Motion off**: all transitions settle immediately and indeterminate rings display a static arc.

## Responsive behavior

`ProgressRing` sizes to its `Size` preset (`Small`, `Medium`, `Large`) or fits within an explicit `Width` and `Height`,
keeping the ring circular and centered above any optional `Label`.

## Related

- `Progress` for a horizontal progress bar with a title row.
- `Skeleton` for content layout placeholders while loading.
- `Gauge` for a dashboard metric dial with thresholds.
