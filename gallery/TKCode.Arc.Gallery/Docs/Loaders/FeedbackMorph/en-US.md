---
title: Feedback morph
summary: A feedback button that springs open into a small form with mood faces and a note, then folds into a thank-you pill, all in one shape.
---

<Demo name="Preview" />

## When to use

`FeedbackMorph` asks for quick feedback right where people are, without taking them out of the page. A small pill with
a speech bubble sits near the content. Click it and the pill grows into a panel: a question, a row of five faces from
upset to thrilled, a note and a send button. Its rows arrive one after another and the faces fan out from the middle.
While the note is sent the button spins; then the panel folds into a thank-you pill with a check that draws itself, and
after a few seconds the pill turns back into the button.

- **Feedback on a page or a feature** in context: a changelog entry, a new screen, a docs article.
- **Mood plus a note**, when a five-point scale says enough and the note is optional.
- **A light satisfaction check** after a task, without a survey or a dialog.

## When not to use

- **Forms that need contact details, attachments or required fields**: use a `Dialog` with a form.
- **A standalone score**: use a `Rating`.
- **Any other anchored content**: use a `Popover`.

## Usage

Give the morph a question and the work that sends the feedback. `SubmitHandler` receives a `FeedbackMorphSubmission`
with the picked mood (`Mood` is its `Value`) and the trimmed note. While its task runs the send button spins and the
note is read-only. When it completes, the thank-you pill shows; when it throws, the panel stays open with an error and
the draft. Closing the panel meanwhile cancels the token.

```xml
<arc:FeedbackMorph x:Name="Feedback"
                   Title="How do release notes read?"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Top" />
```

```csharp
Feedback.SubmitHandler = async (submission, cancellation) =>
{
    var body = JsonContent.Create(new { mood = submission.Mood, message = submission.Message });
    var response = await http.PostAsync("/api/feedback", body, cancellation);
    response.EnsureSuccessStatusCode();
};
```

`FeedbackMorph` lives in the Special package: register it with `UseArcSpecial()` next to `UseArcControls()`. The control
takes the button's size in the layout. The panel (352 px wide) and the thank-you pill grow past it over what is around
the control, so place it where they have room, above the content they cover.

### Direction and anchor

`Side` says which way the panel grows: `Bottom` (the default) keeps the button's top edge and grows down, `Top` keeps
its bottom edge and grows up, for a button near the bottom of a page. `Align` keeps the panel anchored to the button's
`Start`, `Center` (the default) or `End`, so a button near a side can open inward. The preview switches between a button
at the top that opens down and one at the bottom that opens up.

### Moods

Without `Moods` the morph shows five built-in faces (`FeedbackMood.CreateDefaults()`), from upset to thrilled. Pass your
own scale to change the faces, their names and the question each one asks in the note; five read best, three or more
work. A face is one of the built-in drawings (`Face`) or your own 24 × 24 stroked geometry (`Glyph`).

```xml
<arc:FeedbackMorph Title="Did this article help?">
    <arc:FeedbackMorph.Moods>
        <arc:FeedbackMoodCollection>
            <arc:FeedbackMood Value="no" Label="Not really" Face="Sad" Prompt="What were you looking for?" />
            <arc:FeedbackMood Value="some" Label="Somewhat" Face="Neutral" Prompt="What was missing?" />
            <arc:FeedbackMood Value="yes" Label="Yes" Face="Happy" Prompt="Anything we should add?" />
        </arc:FeedbackMoodCollection>
    </arc:FeedbackMorph.Moods>
</arc:FeedbackMorph>
```

### States

| State | What shows |
|---|---|
| `Closed` | The button: a speech bubble and `Label` |
| `Open` | The panel: `Title`, the faces with their caption, the note with its prompt, the status line and the send button |
| `Sending` | The send button spins and reads `SendingLabel`; the note is read-only and the faces dim |
| `Sent` | The thank-you pill with `SuccessLabel`, for `ResetAfter` (3.2 s), then the button again |

The status line beside the send button counts down the characters left once there is a note (`MaxLength`, 500 by
default). Sending with no face and an empty note does not send: the faces give a small nudge and `EmptyHint` shows in
the status line. A failed send shows `ErrorMessage` there until the note changes.

The draft (the picked face and the note) stays when the panel closes, and is cleared after a successful send.
`SelectedMood` and `Message` hold it and bind both ways; `SelectionChanged` reports a new face, `Submitted` a send, and
`StateChanged` every change of `State`, with the exception when a send failed. `Open()`, `Close()`, `Submit()` and
`ClearDraft()` do the same from code, and `IsOpen` opens and closes it.

### Content guidelines

- **Label**: an invitation of two or three words: "Give feedback", "Rate this page".
- **Title**: one short question about the thing next to the button: "How do release notes read?"
- **Prompts**: a question per face that asks for detail, warmer as the faces get happier: "What went wrong?", "What
  should we keep doing?"
- **SuccessLabel**: a short, human thank-you that fits on one line.

## API reference

<!-- api: FeedbackMorph -->

A `FeedbackMood` has a `Value` (what `FeedbackMorphSubmission.Mood` carries), a `Label` (the caption and the face's
accessible name), a `Prompt` (the note's question while it is picked), a `Face` (`Upset`, `Sad`, `Neutral`, `Happy` or
`Delighted`), an optional `Glyph` and a `Tag` for your own data.

## Accessibility

- The button is named by `Label`. While the panel is open the button leaves the tab order and the accessibility tree,
  and the panel is named by `Title`.
- The faces are a radio group (named by `MoodsLabel`) of radio buttons named by their labels, with one tab stop: the
  picked face, or the first one.
- The note is named by `Placeholder`, and its help text is the prompt it shows. The send button names Ctrl+Enter as its
  shortcut.
- A polite live region announces `SendingLabel`, `EmptyHint`, `ErrorMessage` and `SuccessLabel`.
- When the panel opens, focus moves to the picked face or the first face. Escape and the close button return it to the
  button. After a send the thank-you pill takes focus, and the button gets it back when the pill folds.
- Pointer focus draws no ring. Keyboard focus draws a 2 px ring, 2 px outside the button, a face or the send button.

### Keyboard

| Keys | Action |
|---|---|
| `Enter` / `Space` on the button | Open the panel |
| `→` `↓` / `←` `↑` | Pick the next or previous face (wrapping) |
| `Home` / `End` | Pick the first or last face |
| `Tab` | Move between the faces, the note and the send button; tabbing past the send button folds the panel |
| `Ctrl+Enter` (`⌘+Enter`) in the note | Send |
| `Escape` | Fold the panel back into the button, keeping the draft |

A press outside the panel also folds it.

## Motion

- One shape springs between the button's size, the panel's and the thank-you pill's (a critically damped 0.36 s spring)
  with 20 px corners all along, so the 40 px pill is fully round. Its shadow grows with it over 360 ms.
- Opening: the button's content fades and blurs away in 100 ms; the panel fades in 60 ms later, sharpening from a 4 px
  blur and growing from 0.92. Its rows follow 50 ms apart from 80 ms (title, faces, note, actions), each settling from
  10 px above (below when it opens up) out of a soft blur, and the faces grow from 0.7, from the middle outward 35 ms
  apart.
- Picking a face: a soft round highlight glides to it, the picked face grows to 1.14 while the others shrink to 0.92,
  and the caption and the prompt swap with a short rise. The remaining count rolls digit by digit.
- Sending: "Send" leaves upward as the spinner and "Sending" rise into its place. An empty send kicks the faces sideways
  by a few pixels.
- Success: the panel fades in 80 ms while the shape shrinks to the pill; the check's ring draws in over 280 ms, then the
  tick, while the words rise one by one 40 ms apart. Folding back fades the pill in 70 ms and brings the button in 60 ms
  later.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Shape size | Spring | At once | At once |
| Faces of the shape | Fade, blur, scale, staggered rows | Short crossfade | At once |
| Highlight, face scale | Spring | At once | At once |
| Labels, caption, status | Rise and fade | Fade | At once |
| Empty-send nudge | Kick | None | None |

## Responsive behavior

- The panel is 352 px wide, or the viewport less 16 px on each side when that is narrower, so it fits phones. Use `Side`
  and `Align` to keep it inside the viewport near the edges.
- The faces are 44 px targets on equal columns. Hovering a face lights it and names it with a mouse or a pen only.

## Related

- `ActionMorph` for a floating button that opens a menu of quick actions and their forms.
- `Rating` for a standalone score.
- `Popover` for other anchored content.

| Component | Choose it when |
|---|---|
| `FeedbackMorph` | Mood and an optional note, in place, with a thank-you |
| `ActionMorph` | Several quick actions behind one floating button |
| `Rating` | A star score on its own |
