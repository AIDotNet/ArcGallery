---
title: Code magic move
summary: A code block that moves between versions of a snippet: unchanged words glide to their new place, removed ones lift away, and new code types in behind a caret.
---

<Demo name="Preview" />

## When to use

`CodeMagicMove` shows how a snippet changes from one version to the next. Each step is a version of the code with a
short title. When the walkthrough moves on, words that stay glide to their new line and column, words that went away
fade and lift out, and the new code is typed in one character at a time behind an accent caret, while the block grows or
shrinks to the new number of lines. Lines the step introduced keep a faint band with an accent rule, and the header
counts the lines it added and removed. The step dots, the arrow keys and a sideways drag walk through the steps, and by
default the walkthrough plays on its own.

- **Docs pages and landing sections** that teach an API by building a snippet up step by step.
- **Changelogs and upgrade guides** that show exactly what changed between two versions of a call or a config.

## When not to use

- **A single snippet people will copy**: use `CodeBlock`.
- **Large files or code review**: use a proper diff view. This is for short snippets that fit on screen.

## Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:CodeMagicMove x:Name="Walkthrough" FileName="client.ts" Language="ts" Label="Quick start" />
</UserControl>
```

```csharp
Walkthrough.Steps.Add(new CodeMagicMoveStep("Open a connection", "const db = connect();"));
Walkthrough.Steps.Add(new CodeMagicMoveStep("Pass your settings", """
    const db = connect({
      url: process.env.DATABASE_URL,
    });
    """, "Read the address from the environment."));
```

`CodeMagicMove` is a control in the Special package. Register the package with `UseArcSpecial()` next to
`UseArcControls()`. Its content is `Steps`, a list of `CodeMagicMoveStep`: the `Code` of that version, a `Title`, an
optional `Description` under it and an optional `FileName` that replaces the header's `FileName` for that step. Code is
easiest to write in C# raw string literals; tabs become two spaces and the indentation every line shares is removed.

Words are colored like `CodeBlock` when the highlighting package is installed (`UseArcHighlighting()`, set
`Language`, "tsx" by default); without it the code is plain text in the foreground color.

### Steps and value

`Value` is the index of the step shown, two way bindable; setting it moves to that step, and `ValueChanged` reports the
old and new index when a move starts toward another step, or when a drag commits to one. `Next()`, `Previous()` and
`GoTo(index)` move from code. Bind `Value` to keep the walkthrough in step with prose beside it, such as a list of steps
that highlights as the code moves.

### Autoplay

With `AutoPlay` (true) the walkthrough rests on each step for `Interval` (3.2 s), filling the current dot meanwhile,
then moves on, looping from the last step back to the first. It holds while a mouse is over it, and stops for good once
someone steps, drags or presses pause: `IsPaused` turns true. The play button clears it and moves on at once.
`IsPlaying` reports whether autoplay runs. `Speed` multiplies the pace of every move; at 1 a typical move takes about a
second, a little longer when there is much to type or words travel across the whole file.

### Best practices

**Do**

- Keep each step a small, meaningful change with a short title, and say why in the description.
- Wrap long calls: lines over about 50 characters make the code shrink on a phone.
- Give the walkthrough a `Label`; the code area announces it with the step position.

**Avoid**

- Steps that rewrite everything at once: nothing can glide, so the move turns into a retype.
- Using it as the only place a snippet can be copied from; pair the final version with a `CodeBlock`.

### Common questions

**How are the words matched between versions?** Along the longest common sequence of words first; words left over on
both sides with the same text then pair by distance, so a line that moved as a whole glides instead of being typed again.

**Which lines get a band?** Lines holding at least one word that is new in that step compared with the step before. The
header's counts are a line diff that ignores blank lines.

## API reference

<!-- api: CodeMagicMove -->

<!-- api: CodeMagicMoveStep -->

<!-- api: CodeMagicMoveValueChangedEventArgs -->

## Accessibility

- The code area is a focusable group named "Label, step n of total" and described by the keyboard hint. The full code
  of the current step is in the tree as plain text, so assistive technology reads real code rather than the drawn
  layer.
- Previous, next, every step dot ("Step n: title") and play or pause are real buttons with specific names.
- A polite live region announces each step someone chooses with its title and how many lines it added and removed.
  Autoplay is not announced, and the pause button stops it.
- Change is never shown by color alone: the header writes the line counts, the caption names the step, and changed lines
  carry a rule as well as a tint.
- Keyboard focus draws the focus ring along the code area; pointer focus draws none.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Focus the code area, then the buttons and dots |
| `Left` / `Up` | Previous step |
| `Right` / `Down` | Next step |
| `Home` / `End` | First / last step |

## Motion

- Removed words fade and lift 0.28 of a line over 262 ms on the standard curve.
- Kept words glide on the in-out curve for 720 ms (longer when they travel far: 600 ms plus 35 ms per line travelled),
  starting 100 ms in when something was removed and 12 ms later on each lower line. The height and the line numbers
  follow the line count on the same curve.
- At 60% of the glide the new code types in, at about 28 characters a second (a long change types faster, so typing
  never takes longer than 1.5 s); punctuation and the first character after a gap take a little longer. Each word fades
  in over 50 ms; the caret waits 50 ms after the last character and fades over 300 ms.
- A new step's bands fade in over 220 ms as typing reaches them; the old ones fade with the removed words.
- Every move is a function of its progress: a drag scrubs it, past a third of the way (or on a flick) it commits,
  otherwise it rewinds; stepping back mid-move reverses it, and a new step mid-move starts from where every word is. At
  either end the code rubber-bands and settles back on a critically damped spring.
- One frame loop writes the words' positions, and only while something moves, autoplay rests or the code settles; it
  stops off screen.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Step | Glide, lift and type | Changes at once | Changes at once |
| Autoplay | Runs | Off | Off |
| Bands and counts | Fade in | Shown at once | Shown at once |

## Responsive behavior

- The code size fits the widest step to the width it is given, from 13 px down to 10.5 px, so lines never wrap and
  words never collide.
- Below 520 px the caption takes its own row and the step controls spread across the width, with 44 px tall dots.
- Dragging is horizontal only, so vertical scrolling on touch keeps working.

## Related

- `CodeBlock` presents a snippet with copy access.
- `TypewriterTerminal` plays a terminal session line by line.

| Component | Choose it when |
|---|---|
| `CodeMagicMove` | The change between versions is the point |
| `CodeBlock` | People need to read or copy one snippet |
| `TypewriterTerminal` | A command-line session should play out |
