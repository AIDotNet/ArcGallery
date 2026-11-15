---
title: Table of contents progress
summary: A table of contents for a long article whose line steps into each heading level, fills in as you read and marks the current section, with the minutes left beside the title; choosing a heading glides the article there.
---

<Demo name="Preview" />

## When to use

`TocProgress` sits beside a long article, a guide or a policy page and keeps pace with the reader. A thin line runs down
the list of headings and steps sideways into each level's indent with a short S curve. The part already read fills in
with the accent, section by section, and a small marker rides its tip: when it reaches a heading, that heading lights
up, the ones above it read as done and the ones below stay muted. The minutes left to read sit beside the title and end
on Finished. Choosing a heading glides the article there while the line traces the way.

- **Documentation and guides**: people jump between sections and want to know where they are.
- **Engineering write-ups and long reads**: knowing how much is left helps readers commit to the rest.
- **Articles in their own scroll container**: a help panel, a reader view or a dialog.

## When not to use

- **Short pages** with fewer than four headings: a plain list of links is enough.
- **Sections that are alternatives** rather than parts of one document: use `Tabs`.
- **Steps the reader must do in order**: use `Stepper`, which shows completion, not position.
- **Only an overall progress hint** without navigation: show a plain `Progress` bar.

## Usage

`TocProgress` lives in the Special package: register it with `UseArcSpecial()` next to `UseArc()`. Point `Target` at the
article's `ScrollViewer` and mark the headings inside it with `TocProgress.Heading` (and `TocProgress.Level` 2 or 3 for
sub-sections). The list collects the marked elements in document order; the line moves one lane right per level.

```xml
<Grid ColumnDefinitions="*,224" ColumnSpacing="16" Height="400">
    <ScrollViewer x:Name="Article">
        <StackPanel Spacing="12">
            <TextBlock Text="Install" arc:TocProgress.Heading="Install" FontSize="17" />
            <TextBlock Text="Add the package to your project." TextWrapping="Wrap" />
            <TextBlock Text="Options" arc:TocProgress.Heading="Options" arc:TocProgress.Level="2" FontSize="15" />
            <TextBlock Text="Every option has a sensible default." TextWrapping="Wrap" />
        </StackPanel>
    </ScrollViewer>
    <arc:TocProgress Grid.Column="1" Target="{Binding #Article}" ReadingOffset="28" ReadingTime="5" />
</Grid>
```

A heading counts as reached when it crosses the reading line. Set `ReadingOffset` to the height of any sticky header plus
a little air (16 to 32px); chosen headings then scroll to exactly that line. Without it the line sits a quarter of the
viewport down (`ActivationRatio`) and chosen headings stop `ScrollMargin` below the top. Near the end of the article the
line eases down to the bottom of the viewport, so short last sections are still reached when the scroll bottoms out.

`ActiveIndex` and the `ActiveChanged` event report the section the marker is on (the example uses it for the footer);
`Progress` is the share of the article scrolled. `ReadingTime` sets the minutes for the whole article; without it the
control counts the words in the article at 230 words a minute. Set `ShowsTimeLeft` to false to show the percentage read
instead (`ShowsPercent`), and use `TocProgressEntry` children when the list's words should differ from the headings.

### Compact

`DisplayMode` Compact shows one pill button with a progress ring and the current section's title; it opens the same list
in a popup. The default, Auto, switches to it when the viewport is narrower than `ArcViewport.CompactWidth` (640px); set
`DisplayMode` to List to keep the list, for example above the article on phones as in the first example.

<Demo name="Compact" />

### Anatomy

1. **Header**: the title in 13px medium text and the minutes left in 12px muted tabular figures; at the end a check and
   Finished.
2. **Line**: a 1.5px track, one lane per level 12px apart, with a 14px S bend wherever two levels meet; the read part in
   the accent at 2px and a 7px marker at its tip with a 2px clear gap around it.
3. **Rows**: 13px titles with 4px padding, 22px in for the first level and 12px more per level. Upcoming titles are muted,
   read titles secondary and the current one in the full text color; long titles wrap.

### Content guidelines

- Keep to two levels for most articles and three at most; alternating levels on every row makes the line zigzag.
- Keep titles short; they wrap, and the line follows each row's height.
- Pass `ReadingTime` when you know it; the estimate counts every word in the article, code included.

## API reference

<!-- api: TocProgress -->

<!-- api: TocProgressEntry -->

<!-- api: TocProgressLabels -->

<!-- api: TocProgressPosition -->

<!-- api: TocProgressActiveChangedEventArgs -->

## Accessibility

- The control is a navigation landmark named by its title, "On this page" (`Labels.Title`). The list is a list of links:
  each row is named by its title, the current row's help text says "current section" and read rows say "read". Read,
  current and upcoming rows also differ in text contrast, never by the line alone.
- The line, the fill and the marker are drawing only; the time left is plain text and is not announced on every change.
- Every row is in the tab order. Keyboard focus shows a quiet fill and the shared 2px focus ring; pointer focus shows
  neither.
- The compact button is named with the current section and the percentage, "Contents, Results, 42% read".

### Keyboard

| Key | Action |
|---|---|
| Tab | Moves through the rows, each one a link |
| Up / Down | Moves to the previous / next row |
| Home / End | Moves to the first / last row |
| Enter / Space | Scrolls the article to the section |
| Escape | Closes the compact popup and returns to its button |

## Motion

- The reading position is a section index plus how far through that section the reader is, from where its heading
  crosses the reading line to where the next one does. It follows the scroll on a critically damped spring (half way in
  about 100ms, settled in about 0.4s), so a wheel step glides and the marker never points past the current section.
- Each section fills the stretch of line from its heading to the next, bends included, so the fill meets a heading
  exactly when that heading is reached.
- A chosen heading glides into place on the smooth spring and the line traces there through every section in between;
  any wheel or scroll bar input takes over at once. If the article ends before the heading reaches the line, it stays
  current until the reader scrolls on.
- Row colors change over 240ms. The minutes left roll to each new value, sliding 8px with a short blur, and end on
  Finished.
- With reduced motion the fill and the marker follow the scroll directly, chosen headings jump into place, and the time
  left crossfades. Motion off changes everything at once.

## Responsive behavior

- The list takes the width it is given and titles wrap; the line follows each row's measured height, so wrapped titles
  keep the bends between rows.
- The rows, the line and the heading positions are measured again whenever the list, the article or its scroll viewer
  resizes.
- Rows span the full width of the list for touch. On narrow screens place the list above the article, or let `DisplayMode`
  Auto fold it into the compact button.

## Related

- `Progress`: a plain progress bar.
- `Stepper`: progress through steps the user completes in order.
- `ScrollArea`: a scroll container with Arc scroll bars to hold the article.
- `Tabs`: switches between views instead of moving within one.
