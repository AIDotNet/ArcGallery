---
title: Sand text
summary: Large letters built from grains of sand that pour in from above, scatter where the pointer digs, and fill themselves back in.
---

<Demo name="Preview" />

## When to use

`SandText` draws one short line of large text out of small square grains. When it appears, the grains rain down and
pack into each letter from the bottom up. Moving the pointer through the letters digs a channel and throws the grains
along the pointer's path; they tumble, slide off each other and pile up under the text. Once nobody has dug for a moment
the letters fill back in and the spilled sand disappears grain by grain. Giving it a new `Text` turns the old letters
into falling sand that collapses into small dunes while the new word pours in.

- **A hero headline or a section title** that invites people to play with it.
- **A short word that follows a choice**, such as a product name, a plan or a theme picked next to it.

## When not to use

- **Body text or long headings**: keep it to one short line; use `TextReveal` for headlines that should simply arrive.
- **Text that must stay readable while people work near it**, such as form labels or values: digging scatters it.

## Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:SandText Text="Mirage" FontSize="72" />
</UserControl>
```

`SandText` is a control in the Special package. Register the package with `UseArcSpecial()` next to `UseArcControls()`.
It fills the width it is given and is 1.55 em tall: the room under the letters is where the piles form. The text is
centered and shrunk to 94 % of the width when it would not fit, so a long word still shows whole on a phone.

### Size and words

Set `FontSize` for a fixed size, or `FontSizeRatio` to size the letters from the control's width (the preview uses
`0.18`, so the headline grows and shrinks with its card). Changing `Text` drops the old letters and pours in the new
ones; grains that already sit where the new letters are stay put.

### Grains and digging

`GrainSize` is the side of one grain (2 px). `DigRadius` is how far around the pointer the sand is dug away (14 px), and
`IsDigEnabled` turns digging off. The grains take the text color (`Foreground`, the accent by default) and each one is
lightened or darkened a little; `Palette` gives a list of colors to pick from instead:

```csharp
sand.Palette = [Color.Parse("#E8B04B"), Color.Parse("#D9893A"), Color.Parse("#F2D49B")];
```

### From code

`IsDissolved` lets the letters go: every grain falls and the sand disappears, and setting it back pours the letters in
again. `Dissolve()`, `Gather()` and `Toggle()` do the same from code. `Dissolved` fires when the last grain has gone and
`Gathered` when the letters are whole again after a pour (the first show, a new text, `Gather()` or healing).

<Demo name="Code" />

### Best practices

**Do**

- Keep the text to one short word or phrase and give it room: at least 40 px letters read well as sand.
- Leave the control its full width so the piles have space under the letters.

**Avoid**

- Several sand headlines on one screen; one invites play, many are noise.
- Relying on the grains to say anything: the text itself is what assistive technology reads.

### Common questions

**Does digging change the text?** No. Digging only moves grains; the text, its accessible name and its heading level
stay the same, and the letters heal on their own.

**Why do small letters look chunky?** A grain stays `GrainSize` px at every size, so small text has fewer grains per
letter. Use larger text, or a smaller `GrainSize`.

## API reference

<!-- api: SandText -->

## Accessibility

- The text is exposed as a heading (level 2 by default, set by `HeadingLevel`; 0 reports plain text) named by `Text`,
  so screen readers and search read it as text whatever the grains are doing.
- The grains are decoration and never change the accessible text.
- The control takes no keyboard focus and has no keyboard interaction: it is a headline, not a control to operate.

## Motion

- The first show pours the letters in over about one second: grains fall from the top under gravity, straight through
  any sand above their cell, and the lowest rows fill first.
- A new `Text` lets the old letters fall two grain rows a frame as whole shapes; they slide into 45° dunes while the
  new word pours in over about 0.75 s.
- Dug grains fly with a share of the pointer's speed, then fall and slide like the rest. About 0.7 s after the last grain
  was dug out the letters heal, and the loose sand outside them disappears within about two seconds.
- Each grain keeps its shade as it moves. Frames run only while something moves and pause while the control is out of
  view.

| Change | Full | Reduced | Off |
|---|---|---|---|
| First show | Grains pour in | Letters appear formed | Letters appear formed |
| New `Text` | Old letters collapse, new ones pour in | Swapped at once | Swapped at once |
| Pointer over the letters | Digs | Nothing | Nothing |
| `Dissolve()` / `Gather()` | Sand falls and disappears / pours back | At once | At once |

## Responsive behavior

- The control is as wide as its parent and 1.55 em tall; with `FontSizeRatio` the letters follow the width, and text
  wider than 94 % of the width is shrunk to fit, so a headline works down to phone widths.
- Resizing rebuilds the grid and shows the letters formed at once.
- On touch screens dragging a finger digs; the finger's taps do nothing else.

## Related

- `TextReveal` brings a headline in as it scrolls into view.
- `TextMorph` changes one label into another without breaking it apart.

| Component | Choose it when |
|---|---|
| `SandText` | A headline should be fun to touch and can come apart |
| `TextReveal` | A headline should arrive and then stay still |
| `TextMorph` | A short label changes to a new label |
