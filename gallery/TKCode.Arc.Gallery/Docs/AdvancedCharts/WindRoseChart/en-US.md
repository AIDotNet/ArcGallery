---
title: Wind rose chart
summary: Show how often the wind blows from each direction and how hard: petals stacked by speed on a percent or count axis, with the calm share in the centre.
---

<Demo name="Meteorological" />

## When to use

`WindRoseChart` shows a distribution over directions with a magnitude stacked on top. Each direction sector is a petal;
its length is how often the wind came from there, and its bands split that time by wind speed. Calm periods, which have
no direction, sit in the centre.

- **Climate and site studies**: the wind of a weather station over a year, a season or a month, for an airport runway,
  a wind farm, a harbour or the ventilation of a building.
- **Dispersal and safety**: where smoke, noise or a spill would drift most of the time, and how fast.
- **Other direction-by-strength data**: wave direction and height, the heading and speed of traffic, sound sources.

The chart reads best with 8 or 16 sectors and four to seven speed bins.

## When not to use

- **One value per direction without a magnitude to split**: use `RadarChart`.
- **Wind over time**: use `LineChart` or `RealtimeStream`, where the x axis is time.
- **Exact comparison of two directions**: petals at different angles are hard to compare precisely. Use `BarChart`.
- **Categories without a circular order**: the petals form a loop, so only data whose last category sits next to its
  first, such as compass directions or hours of the day, belongs in a rose.

## Usage

Give the chart rows (one direction with an amount per speed bin), records (one direction, bin and amount each) or your
own objects, and optionally the calm amount. With no `Bins` the chart uses six speed bins in m/s: 0.5–2, 2–4, 4–6, 6–8,
8–10 and 10 or more.

```xml
<arc:WindRoseChart Label="Harbour station, March" SectorCount="8" Calm="12">
    <arc:WindRoseRow Direction="N" Values="10 6 2 0 0 0" />
    <arc:WindRoseRow Direction="NE" Values="14 9 4 1 0 0" />
    <arc:WindRoseRow Direction="E" Values="5 3 1 0 0 0" />
    <arc:WindRoseRow Direction="SE" Values="4 2 0 0 0 0" />
    <arc:WindRoseRow Direction="S" Values="8 7 3 1 0 0" />
    <arc:WindRoseRow Direction="SW" Values="12 18 15 8 3 1" />
    <arc:WindRoseRow Direction="W" Values="9 12 8 4 1 0" />
    <arc:WindRoseRow Direction="NW" Values="6 5 2 1 0 0" />
</arc:WindRoseChart>
```

### Anatomy

1. **Petals**: one per direction sector, centred on its direction. Sector 0 is north at the top and the sectors run
   clockwise, like a compass.
2. **Speed bands**: each petal is a stack of speed bins from the centre outward, with a small gap between bands.
3. **Rings**: the radial axis, with values along the boundary between the first two sectors.
4. **Calm circle**: the share (or count) of calm readings in the centre.
5. **Direction labels**: compass points outside the outer ring, in the current language.
6. **Tooltip**: the direction, its total and every speed band with an amount.
7. **Legend**: each speed bin's color and range, under the rose.

### Data: rows, records or your own objects

`ItemsSource` takes any list of `WindRoseRow` and `WindRoseRecord`, mixed if you like; without it the chart reads its
inline rows (`Items`). Entries for the same sector and bin add up, and an observable collection is followed.

| Type | Member | Description |
|---|---|---|
| `WindRoseRow` | `Direction` | Degrees clockwise from north (a number), or a direction name such as `NNE`, `NbE` or one of `DirectionLabels` |
| `WindRoseRow` | `Values` | One amount per speed bin, inner to outer; in XAML `Values="1.2 3.4 0.8"` |
| `WindRoseRecord` | `Direction` | As for a row |
| `WindRoseRecord` | `Bin` | Index of the speed bin |
| `WindRoseRecord` | `Value` | The amount |
| `WindRoseBin` | `Minimum` | Lowest speed of the bin; the next bin's minimum is its upper bound and the last bin is open |
| `WindRoseBin` | `Label` | Name in the legend and the tooltip; by default the range with `Unit`, such as "2–4 m/s" |
| `WindRoseBin` | `Color` | Fixed color; by default the bin's step of the chart's ramp |

A direction belongs to the sector whose range contains it: with 16 sectors, sector 1 (NNE) covers 11.25° up to, but not
including, 33.75°, and 360° is north again. Amounts that are missing, negative or not a number count as zero, and a bin
index outside `Bins` is ignored.

Your own objects need no wrapper. Point the member bindings at them, in long form (one amount per item) or wide form
(one direction per item with a list of amounts):

```xml
<arc:WindRoseChart ItemsSource="{Binding Readings}"
                   DirectionMemberBinding="{Binding Heading}"
                   BinMemberBinding="{Binding SpeedClass}"
                   ValueMemberBinding="{Binding Hours}" />
```

### Binning raw observations

`WindRoseBinning.Bin` sorts raw readings (direction in degrees and speed) into sectors and speed bins. Readings slower
than the calm threshold (0.5 by default) count as calm whatever their direction; readings whose direction or speed is
not a number, or whose speed is negative, are skipped and counted in `Skipped`.

```csharp
var readings = station.Hours.Select(h => new WindObservation(h.DirectionDegrees, h.SpeedMetresPerSecond));
var wind = WindRoseBinning.Bin(readings, sectorCount: 16);
chart.ItemsSource = wind.Rows;
chart.Calm = wind.Calm;
```

Pass `binMinimums` to use other bins (and give the chart the same `Bins`), and `calmThreshold` to change what counts as
calm. A generic overload reads any items through two selectors: `WindRoseBinning.Bin(hours, h => h.Heading, h => h.Knots, 16)`.

### Sectors and orientation

`SectorCount` sets the number of directions. 8, 16 and 32 sectors get compass labels (N, NE …; N, NNE …; N, NbE …);
any other count from 4 to 72 is labelled in degrees, such as 36 sectors of 10°. `StartAngle` turns the rose so sector 0
sits at another screen angle, and `SweepDirection="CounterClockwise"` runs the sectors the other way. Neither changes
which data goes where: directions are always compass degrees.

<Demo name="SectorCounts" />

`DirectionLabels` replaces the labels from sector 0; an empty or missing entry keeps the default for that sector, and
data can name a sector by its custom label.

```csharp
chart.SectorCount = 4;
chart.DirectionLabels = ["Bow", "Starboard", "Stern", "Port"];
```

### Speed bins, colors and units

By default the bins take one hue from the theme's first chart color, light to dark on a light surface and dark to light
on a dark one, so faster wind always reads as stronger and the chart follows the accent color and the appearance. Give a
bin a `Color` to fix it, and set `Unit` for the default labels. Fixed colors stay the same in both appearances, so pick
ones that read on light and dark surfaces.

<Demo name="CustomBins" />

### Percent or count axis

With `RadialAxis="Percent"` (the default) a petal's length is its share of all readings, calm included, so the petals and
the calm circle add up to 100 %. `RadialAxis="Count"` draws the amounts as given. `RadialMaximum` fixes the value at the
outer ring (longer petals stop at the rim, and the tooltip still reads the real value) and `RadialStep` the distance
between rings; left as NaN, the chart picks a round maximum with at most five rings.

<Demo name="CountAxis" />

```xml
<arc:WindRoseChart RadialMaximum="20" RadialStep="5" ItemsSource="{Binding Months[0].Rows}" />
```

`ValueFormatter` formats every displayed value (rings, tooltip, centre and readings). It gets the percent (0 to 100) or
the amount.

### Calm

`Calm` is the calm amount in the same unit as the values: a count when the values are counts, or a percent when your
data are already percentages (the percent axis then keeps them as they are). The centre shows it as a share or a count;
when the calm circle is too small for the text, only the summary and the readings carry it.

### Live data

Replace `ItemsSource`, change the collection, or change `Calm`, `Bins` or `SectorCount`, and every band springs from
where it is on screen to its new length while the rings follow the new maximum. Changes to a collection within one
dispatcher turn are read once, so clearing and refilling it moves the rose in one go.

<Demo name="LiveUpdate" />

### Reading and choosing a direction

Hovering a petal (or its label), or moving with the arrow keys, sets `ActiveIndex` and shows the tooltip; the other
petals fade and the band under the pointer gets an outline. Hovering or focusing a legend entry highlights that speed bin
in every petal.

A click, a tap, Enter or Space raises `ItemInvoked` with the sector and the band under the pointer (`Bin` is -1 from the
keyboard). With `IsSelectionEnabled` the sector also becomes `SelectedIndex`, drawn with an outline, and
`SelectionChanged` reports `OldItem` and `NewItem`. It is raised whenever `SelectedIndex` changes, from a gesture, code or
a binding. `Sectors` lists every sector with its label, range, amounts, total and share.

<Demo name="Selection" />

### Binding to a view model

```xml
<arc:WindRoseChart Label="{Binding StationName}"
                   ItemsSource="{Binding Wind.Rows}"
                   Calm="{Binding Wind.Calm}"
                   SectorCount="{Binding Sectors}"
                   SelectedIndex="{Binding SelectedDirection, Mode=TwoWay}"
                   IsSelectionEnabled="True" />
```

- `ItemsSource` can be an array, a list or an observable collection. Changing a row's `Values` in place is not observed:
  replace the row, the list or the collection's item.
- `SelectedIndex` and `ActiveIndex` are two-way; -1 means none.

### Direction names and languages

The default labels come from the current Arc language: N, NNE, NE … in English, 北, 北东北, 东北 … in Simplified Chinese
and 北, 北北東, 東北 … in Traditional Chinese, and they change when the language does. "Calm" follows the language too.
Data can name directions in English abbreviations (`NNE`, `NbE`) or in the current language.

### Content guidelines

- **Label**: name the place and the period, such as "Harbour station, hourly wind in 2025".
- **Bins**: four to seven, in ascending order. Keep the ranges round and say the unit once through `Unit`.
- **Custom labels**: short; at small sizes they thin out to every second or fourth direction.

### Best practices

**Do**

- Say whether petals show the share of all readings or counts, and whether calm is included.
- Keep the same bins and a fixed `RadialMaximum` when roses are compared side by side.
- Bin raw readings with the same sector count the chart shows.

**Avoid**

- Comparing petal areas: read the lengths and the tooltip.
- Re-binning data made for 16 sectors into 8: half-way directions such as NNE fall into the next sector clockwise.
  Bin the raw readings again instead.

### Common questions

**Why do my petals look short?** In percent mode the calm readings count toward 100 %. Use `RadialMaximum` to zoom in, or
`RadialAxis="Count"`.

**Can I show 36 sectors?** Yes: `SectorCount="36"`; the labels show degrees and thin out to fit.

## API reference

<!-- api: WindRoseChart -->

<!-- api: WindRoseRow -->

<!-- api: WindRoseRecord -->

<!-- api: WindRoseBin -->

<!-- api: WindRoseSector -->

<!-- api: WindRoseBinning -->

## Accessibility

- The rose is a focusable group. Its automation name is the chart's `Label` with the key instructions; while a direction
  is being read it announces the direction, its total and each speed band, such as "SW: 14.1%. 0.5–2 m/s 1.0%, 2–4 m/s
  3.0%, …", and ", selected" for the selected one.
- With no reading, the help text summarises the calm share and every direction's total.
- Speed bins are named in the legend, the tooltip and the reading, and the default colors are ordered by lightness, so
  color is not the only cue. Each legend entry is a tab stop named after its bin and its share of the readings.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Focus the rose and read the selected direction (or the first), then move through the legend entries. A focused entry highlights its bin |
| `Right` / `Down` | Read the next direction clockwise on screen |
| `Left` / `Up` | Read the next direction counter-clockwise on screen |
| `Home` / `End` | Read the first or the last direction |
| `Enter` / `Space` | Choose the direction being read (`ItemInvoked`; selects it when `IsSelectionEnabled`) |
| `Escape` | Clear the reading; with no reading, clear the selection |

## Motion

- On first view the bands grow out of the calm circle on a gently bouncing spring.
- New data, a new axis mode or a new sector count springs every band to its new length; the rings follow the new
  maximum. With a new sector count, each new petal grows from the old petal that covered its direction.
- Reading a direction fades the other petals; the tooltip glides between directions.
- Reduced motion shows changes immediately and the first view is a short fade.

| Change | Full | Reduced | Off |
|---|---|---|---|
| First view | Bands grow from the centre | Short fade | Immediate |
| Data or axis changes | Bands and rings spring | Immediate | Immediate |
| Reading moves | Petals fade, tooltip glides | Immediate | Immediate |

## Responsive behavior

- The rose shrinks to fit the width and the height it is given and never grows past `Size`.
- Below 360 wide the label ring tightens, and at any size the direction labels thin out to every second, fourth or eighth
  direction when they would collide; the four cardinal points always stay, and the direction being read always shows.
- Ring labels thin out when the rings get close; the centre text hides when the calm circle is too small for it.
- The legend wraps onto more lines, and the tooltip stays inside the chart, beside the petal when there is room.
- A touch press reads the direction under the finger and a drag sweeps the others; a tap chooses it.

## Related

| Component | Choose it when |
|---|---|
| `RadarChart` | Each direction or measure has one value |
| `BarChart` | Directions should be compared exactly, or there is no circular order |
| `Sunburst` | Shares are nested in a hierarchy rather than spread around a circle |
| `LineChart` | Wind speed or direction over time |
| `ActivityHeatmap` | Two cyclic dimensions, such as hour of day by weekday |
