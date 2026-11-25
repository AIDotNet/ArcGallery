---
title: Diff editor
summary: "Compare two texts like Monaco's diff editor: side by side or inline, changed lines and characters highlighted, moved blocks, unchanged regions collapsed, and arrows that revert a change."
---

<Demo name="Preview" />

The left side is the original text and the right side the modified one. Changed lines are tinted, the changed characters inside them stronger, and the lines of both sides stay aligned while you scroll. Edit the right side and the diff follows.

## When to use

- Reviewing a change before it is saved, committed or applied: a generated edit, a merge result, a settings migration.
- Showing what changed between two versions of a file, a query or a configuration.
- Letting people take back parts of a change: each change has an arrow that reverts it.

## When not to use

- Editing one text with no version to compare against: use `CodeEditor`.
- A short before / after of a sentence or a value: show both, or mark the words that changed in ordinary text.
- Many files at once: the diff editor compares one pair of texts.

## Usage

`DiffEditor` is in the `TKCode.Arc.Editor.Controls` package, next to `CodeEditor`, and `UseArcCodeEditor()` registers both:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

Give it the two texts and a language for the highlighting. `OriginalText` is read-only; `ModifiedText` binds both ways, so what the user types comes back to the view model.

```xml
<arc:DiffEditor Height="400"
                LanguageId="csharp"
                OriginalText="{Binding SavedSource}"
                ModifiedText="{Binding Source}" />
```

The diff is Monaco's: lines first, then the characters inside changed lines, with blocks that moved detected and connected. It is computed off the UI thread from snapshots of the two texts and again shortly after each edit; a very large pair gives up after a time limit and shows the lines it found. `DiffComputed` is raised each time with the number of changes, the lines added and removed and the moved blocks.

```csharp
Diff.DiffComputed += (_, e) => Summary.Text = e.IsIdentical ? "No changes" : $"{e.ChangeCount} changes";
```

### Options

<Demo name="Options" />

- `RenderSideBySide` shows the texts side by side (on); off, one editor shows the modified text with the removed lines between its lines.
- `HideUnchangedRegions` collapses long runs of unchanged lines into a bar that says how many lines are hidden; click it to show them (off).
- `IgnoreTrimWhitespace` ignores changes of leading and trailing whitespace, as Monaco does by default (on).
- `IsReadOnly` keeps the modified text from changing too; the original text never changes.

### Keyboard

| Keys | Action |
|---|---|
| F7 | Go to the next change |
| Shift+F7 | Go to the previous change |

Inside each side, the keys of `CodeEditor` apply: find, multiple cursors, folding and the rest.

## API reference

<!-- api: DiffEditor -->

## Accessibility

- Name the diff editor with `AutomationProperties.Name`, saying what is compared, for example "Changes to Program.cs".
- Changes are shown by color and by the + and − signs in the gutter, not by color alone.
- The two sides are code editors and read their text to screen readers; there is no list of the changes for screen readers yet.

## Motion

- Nothing moves on its own. Going to a change scrolls both sides to it.

## Responsive behavior

- The diff editor fills the width it is given and splits it between the two sides. On a narrow screen, turn `RenderSideBySide` off: the inline view needs only one column.
- Give it a height: it scrolls its content and does not grow with it.

## Related

- `CodeEditor`: edit one text.
- `CodeBlock`: show code without editing it.
