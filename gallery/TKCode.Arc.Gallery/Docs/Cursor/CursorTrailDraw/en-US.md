---
title: Cursor trail draw
summary: A sketching board: ink that thins with speed or pen pressure, a laser pointer whose trail fades by itself, and rough boxes, circles, triangles and lines that spring into clean shapes when the pen lifts.
---

<Demo name="Preview" />

## When to use

`CursorTrailDraw` is a small whiteboard. Ink follows the pointer with a little smoothing and takes its width from pen
pressure, or from speed for a mouse or a finger, so quick lines come out thinner; every stroke tapers in and lifts off.
When the pen lifts, a rough circle, ellipse, box, square, triangle, four sided shape or straight line springs into a
clean shape, and a chip at the top offers to keep the stroke as it was drawn. The laser pointer draws nothing: it
leaves a short trail that thins and fades on its own, for pointing during a walkthrough.

- **Design reviews, planning and teaching**: people sketch boxes, flows and circles quickly and want them tidy.
- **Live walkthroughs and demos**: a presenter points with the laser and the board cleans up after itself.
- **Landing pages** that let visitors try a sketching feature right away.

## When not to use

- **Capturing a signature**: use `SignaturePad`, which exports the result.
- **Commenting on a screenshot or a design**: pin comments to the picture instead.
- **Full drawing tools** with selection, text, layers or an endless canvas: use a dedicated canvas editor.

## Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:CursorTrailDraw Label="Release plan sketch" />
</UserControl>
```

`CursorTrailDraw` is in the Special package; register it with `UseArcSpecial()` next to `UseArcControls()`. The card
shows `Label` as its heading with an item count, the board, and a floating toolbar: pen, laser pointer, one button per
ink, shape snapping, undo and clear. The board is 16:10 and turns 4:5 when the card is narrower than 560 px; give the
card a `Height` and the board fills the rest.

The first time the board comes into view with nothing on it, it plays a short sketch (`PlaysIntro`): a rough box and
circle snap clean, an accent line joins them, then the laser sweeps under the circle. Any press or key takes over at
once. `PlayIntro()` plays it again on an empty board.

### Saving a sketch

`ItemsChanged` reports every change: a stroke landed or snapped, a shape was kept as drawn, the last item was undone,
or the board was cleared. Its `Items` hold every item, each with an id, its shape, its ink, a width and its centerline
points in board units: the board is always 1000 units wide, so a saved sketch scales with any board. `SetItems` loads a
sketch back without raising the event. Turn `PlaysIntro` off where the board holds real work.

<Demo name="Review" />

```xml
<arc:CursorTrailDraw Label="Onboarding screen review" PlaysIntro="False">
    <arc:CursorTrailDraw.Inks>
        <arc:CursorTrailDrawInk Id="ink" Label="Ink" />
        <arc:CursorTrailDrawInk Id="accent" Label="Accent" />
    </arc:CursorTrailDraw.Inks>
</arc:CursorTrailDraw>
```

### Inks, tools and feel

`Inks` lists the toolbar's inks; the first is picked first. Without a `Brush`, an ink named `accent` uses the accent,
`red` the danger color and any other the foreground, so the inks follow the theme. With no inks the board offers Ink,
Accent and Red. `Tool`, `SelectedInkId` and `IsSnapEnabled` are the current choices and can be bound both ways.

`PenSize` is the width at half pressure (4 px; strokes range from about 2 to 5 px), `Smoothing` how far the ink
trails the pointer (0 raw, 1 very smooth), `LaserFade` how long the laser trail lives (700 ms), and `Speed` multiplies
the snap spring, the laser fade and the intro. `AccentBrush` recolors the accent ink and the laser; `Hint` is the line
on an empty board. `Undo()`, `Clear()` and `KeepAsDrawn()` do what the toolbar and chip do.

### Best practices

**Do**

- Name the board with `Label`; it is the heading and the accessible name.
- Save the items from `ItemsChanged` and load them with `SetItems`.
- Keep two or three inks that read clearly on the board.

**Avoid**

- Leaving the intro on where people open saved work.
- Relying on the laser to leave a mark; it is for pointing.

### Common questions

**Which shapes snap?** Lines (to 0, 45 and 90 degrees when within 7 degrees), circles and ellipses, rectangles and
squares (leveled when within 10 degrees), triangles and other four sided shapes. Anything else stays freehand.

**Can someone undo a snap?** Yes: the chip's button turns the shape back into the stroke as drawn on the same spring.
Undo removes the whole item.

## API reference

<!-- api: CursorTrailDraw -->

<!-- api: CursorTrailDrawInk -->

## Accessibility

- The card is a group named by `Label`. The board is a focusable drawing area ("Label drawing area") described as a
  whiteboard; its help text explains the keys and lists what is on the board ("Board contents: 1 circle, 1 line.").
- The toolbar is a toolbar: each button is a Tab stop and the arrow keys, Home and End move between them. Tools, inks
  and snapping are toggle buttons with specific names; Undo and Clear are dimmed while the board is empty.
- Snaps, strokes, undo, clear, tool and ink changes and pen down or up are announced in a polite live region. The chip's
  button is a real button.
- The drawing itself is not read out; the summary names every item by shape.

### Keyboard

| Keys | Action |
|---|---|
| Arrow keys | Move the cursor (hold two for diagonals) |
| `Shift` + arrows | Move slowly |
| `Space` / `Enter` | Put the pen down, lift it |
| `Escape` | Drop the stroke in progress |
| `P` / `L` | Pick the pen / the laser pointer |
| `S` | Turn shape snapping on or off |
| `Ctrl` / `Cmd` + `Z` | Undo |

## Motion

- The pen tip eases toward the pointer every frame (about 21 ms behind at the default smoothing); width follows
  pressure or speed and both ends taper.
- On release a recognized stroke is resampled and every point travels to the clean shape on a lightly bouncy spring
  (about 10% overshoot), then the board swaps in the exact shape. Keep as drawn runs the same spring back.
- The laser trail thins from head to tail and fades out within `LaserFade`; lifting lets it shrink away.
- The chip fades in and drops into place, stays about four seconds and fades out; removed items fade out.
- One frame loop drives the board; it runs only while something moves and pauses out of view.

| Change | Full | Reduced or paused | Off |
|---|---|---|---|
| Intro | Plays once | Finished sketch at once | Finished sketch at once |
| Snap | Spring into shape | Lands at once | Lands at once |
| Laser | Fading trail | Dot only | Dot only |
| Chip, hint, buttons | Fade and spring | Instant | Instant |

## Responsive behavior

- The board is 1000 units wide at any size, so items scale with it; new ink, the clean shapes, the cursor and the
  grid stay the same size in px.
- Below 560 px the board turns 4:5, the intro stacks its shapes, and the toolbar buttons shrink to 38 px; below
  360 px they shrink to 30 px.
- Touch draws only on the board, so the page still scrolls around it.

## Related

- `SignaturePad` captures a signature with smooth ink and exports it.
- `InkFluid` and `ImageTrail` are other surfaces that answer the pointer.

| Component | Choose it when |
|---|---|
| `CursorTrailDraw` | People sketch and point while they talk |
| `SignaturePad` | One person signs and the result is saved |
| `InkFluid` | The pointer should stir a decorative background |
