---
title: Mobile radio cards
summary: A grid of selectable cards for a choice that reads better as pictures or short descriptions, with a press state and no hover state.
---

<Demo name="Basic" />

## When to use

`RadioCards` lays out one choice as a grid of cards, so each option can show an icon, a swatch or a line of description.

- **A plan, a delivery speed, a layout**: options that need a sentence each to be told apart.
- **A shape, a color, a style**: options that are easier to recognize by sight than by name.
- **A choice that is part of a settings screen**: put the grid in a `ListCell` of a `ListGroup`.

The live demos run inside a phone frame, a desktop simulation; touch feedback on a real device has not been verified here.

## When not to use

- **Options that are only short labels**: use `MobileSegmented` (two to four) or `MobileRadio` (a list).
- **A setting whose value shows in a row**: use `MobilePicker`.
- **Any number of options at once**: radio cards hold exactly one choice; use `MobileCheckbox`.

## Usage

Write `RadioCardOption`s in the group, or bind `ItemsSource` to a list of options. Each option has a `Value`, a `Label`, an optional `Description` and an optional `Icon`. `SelectedValue` (two-way) is the selected option's `Value`; `SelectedIndex` and `SelectedItem` work as on any Avalonia selecting control, and `SelectionChanged` reports every change of the selection.

```xml
<arcm:RadioCards xmlns="https://github.com/avaloniaui"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                 xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                 SelectedValue="{Binding Plan}">
    <arcm:RadioCardOption Value="free" Label="Free" Description="3 projects and 1 GB of storage." Icon="{x:Static arc:ArcIcons.Zap}" />
    <arcm:RadioCardOption Value="pro" Label="Pro" Description="Unlimited projects and priority support." Icon="{x:Static arc:ArcIcons.Star}" />
    <arcm:RadioCardOption Value="enterprise" Label="Enterprise" Description="Talk to sales first." IsEnabled="False" />
</arcm:RadioCards>
```

The cards share the width in `Columns` equal columns (2 by default), and the cards of one row are as tall as the tallest of them. An `Icon` that is a geometry, such as a Lucide icon from `ArcIcons`, is drawn as a 22 px stroke glyph; any other content (an `Ellipse` swatch, a shape) is shown as it is. A finger on a card shades it and, with full motion, presses it in slightly; lifting the finger on the card chooses it, and a scroll that starts on a card does not.

`IsEnabled="False"` dims an option and keeps it from being chosen; `IsVisible="False"` hides it, and it takes no room. The cards follow later changes of their options. Data items that are not options can be shown with an `ItemTemplate`; the card's label is then the template.

### Compact choices

For swatches and shapes, turn the radio indicator off with `ShowIndicator="False"` and use three columns: the ring that glides to the selected card marks the choice by itself.

<Demo name="Swatches" />

```xml
<arcm:RadioCards Columns="3" ShowIndicator="False" SelectedValue="{Binding Color}">
    <arcm:RadioCardOption Value="violet" Label="Violet">
        <arcm:RadioCardOption.Icon>
            <Ellipse Width="28" Height="28" Fill="#6E56CF" />
        </arcm:RadioCardOption.Icon>
    </arcm:RadioCardOption>
</arcm:RadioCards>
```

### Tokens

`RadioCardsToken` (identity `MobileRadioCards`) holds `ColumnGap` and `RowGap` (10) and `RingThickness` (2). `RadioCardToken` (identity `MobileRadioCard`) holds a card's `CardPadding` (12), `IconExtent` (22), `LabelMargin`, `DescriptionMargin`, `IndicatorSize` (22), `IndicatorBorderWidth` (1.5), `DotSize` (10), `IndicatorMargin`, `DisabledOpacity` (0.5) and the focus ring's `FocusRingMargin` and `FocusRingRadius`. The identities carry the `Mobile` prefix because the desktop package has radio cards of its own. Colors, radii and type come from the ArcDesign tokens: the ring and the dot use `ControlOn`, as a mobile radio does.

## API reference

<!-- api: MobileRadioCards -->

<!-- api: MobileRadioCard -->

<!-- api: MobileRadioCardOption -->

## Accessibility

The group is one tab stop: Tab lands on the selected card (or the first one that can be chosen). The arrow keys move the selection through the grid, Left and Right in reading order (mirrored right to left, and wrapping), Up and Down by a row; disabled and hidden cards are stepped over. Home and End jump to the first and last card, and Enter or Space choose the focused card. Keyboard focus shows a 2 px focus ring around the card. A card is named by its label, and its description is its help text. The selected card is marked by the ring and by the filled radio dot, not by color alone; with `ShowIndicator="False"` the thicker ring remains.

## Motion

One ring glides from card to card on a spring when the selection changes, drawn in render so the layout never moves; the first time it appears it fades in place. The radio dot springs in with a short fade. A pressed card shades on the fast curve and, with full motion only, scales to 0.97 on a snappy spring. With reduced motion the ring jumps, nothing scales and the dot and colors change at once.

## Responsive behavior

- `RadioCards` has no breakpoint: `Columns` (2 by default) equal columns share whatever width the group gets, and the
  cards of one row take the height of the tallest one.
- On a phone, keep cards with an indicator and text to two columns; three or four columns suit icons and swatches with
  short labels and `ShowIndicator="False"`. Labels and descriptions wrap, so a card grows taller rather than wider.
- A hidden option takes no room, so the cards after it move up into the grid.
- Nothing depends on hover: a finger shades a card and lifting it chooses the card.

## Related

- `MobileRadio`: one choice as a list of rows.
- `MobileSegmented`: two to four short options in one row.
- `MobilePicker`: a row that shows the value and opens the choices in a sheet.
- `MobileListGroup`: put the grid in a `ListCell` to place it among rows.
