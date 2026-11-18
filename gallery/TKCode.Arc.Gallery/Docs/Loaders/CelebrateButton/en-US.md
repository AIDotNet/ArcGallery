---
title: Celebrate button
summary: A button for milestones: when its work succeeds, confetti flies out from its edges, the button kicks, and the label turns into a check.
---

<Demo name="Preview" />

## When to use

`CelebrateButton` is an accent capsule that marks a moment worth celebrating. Pressing it runs your work: a small
spinner and a pending text show while it runs. When it succeeds, confetti leaves from points all around the button's
outline, the button kicks up a little, and the label lifts out of a blur as a check with the success text, while the
width springs to fit. A failure shows an error text instead, with no confetti. A couple of seconds later the button
returns to rest.

- **Finishing a milestone**: publishing a site, shipping a release, completing setup, sending the first invoice.
- **Onboarding steps** where a small reward invites the next one.
- **Product demos** where the moment of success is the point.

## When not to use

- **Routine saves and form submits**: use a `Button` with its loading state.
- **Destructive actions**: use `HoldToConfirm` or `ConfirmMorph`.
- **Dense toolbars and tables**: the confetti needs room and would distract.
- **More than one per view**: the burst only feels special when it is rare.

## Usage

Give the button its resting content, the pending and success texts, and the work to run. The work is a `Func<Task>`:
while its task runs the button shows the spinner and `PendingLabel`; when it completes the confetti bursts and
`SuccessLabel` shows with a check; if it throws, `ErrorLabel` shows and a press runs the work again. Without an action a
press goes straight to the celebration. `Click` and `Command` still run first on every press the button accepts.

```xml
<arc:CelebrateButton x:Name="Ship"
                     Content="Ship release"
                     PendingLabel="Shipping"
                     SuccessLabel="Shipped" />
```

```csharp
Ship.Action = () => releases.ShipAsync(build);
Ship.StatusChanged += (_, e) =>
{
    if (e.NewStatus == CelebrateButtonStatus.Error)
    {
        log.Warn(e.Error);
    }
};
```

`CelebrateButton` lives in the Special package: register it with `UseArcSpecial()` next to `UseArcControls()`. It is
40 px tall and as wide as its label plus 20 px on each side.

### States

| Status | What shows | Presses |
|---|---|---|
| `Idle` | The button's content | Run the action |
| `Pending` | A spinner and `PendingLabel` (the content's text when unset) | Ignored |
| `Success` | A check and `SuccessLabel` ("Done"), after the burst | Ignored |
| `Error` | `ErrorLabel` ("Try again"), no burst | Run the action again |

`Status` is a two-way property. Set it yourself when the work starts somewhere else, such as a form submit: moving it
to `Success` plays the burst. `StatusChanged` reports every change, including the automatic return to rest, with the
exception when the action failed. `ResetAfter` (2.4 s) is how long the success or error state stays; zero keeps it until
you call `Reset()` or set `Status`.

### Confetti

44 pieces leave per burst (`ParticleCount`), at most 180 fly at once, and `Cooldown` (1.2 s) is the shortest time
between two bursts. `Celebrate()` plays a burst without changing the status. By default the confetti is mostly the accent
in three tones with a sunny yellow and a little of the foreground ink, read from the theme at the moment of the burst, so
it follows the accent and the light or dark appearance. `ConfettiColors` replaces that palette with your own colors,
picked evenly.

The pieces fly up to 190 px above the button, 150 px beside it and 110 px below it, drawn over whatever is there. Leave
that room around the button, or let the container clip them.

### Content guidelines

- **Content**: the milestone as a short verb phrase: "Ship release", "Launch beta", "Finish setup".
- **PendingLabel**: the same verb in progress: "Shipping", "Publishing".
- **SuccessLabel**: the result in one or two words: "Shipped", "Published", "All set".
- **ErrorLabel**: short, and it doubles as the retry prompt: "Try again", "Retry upload".

### Best practices

- Return the real task from `Action`, so the pending and error states tell the truth.
- Keep it for real milestones. A burst on every save stops meaning anything.
- Keep the labels close in length, so the width change stays small.

## API reference

<!-- api: CelebrateButton -->

<!-- api: CelebrateButtonStatus -->

<!-- api: CelebrateButtonStatusChangedEventArgs -->

## Accessibility

- It is a button: Tab reaches it and Enter or Space press it. Its accessible name is the text it shows now (the content,
  the pending text, the success or error text) unless you set `AutomationProperties.Name`.
- While the action runs the button reports itself as not enabled and ignores presses.
- A polite live region announces the success and error texts.
- Success never relies on color or confetti alone: a check and the success text replace the label.
- The confetti layer takes no input and is hidden from assistive technology.
- Keyboard focus draws a 2 px capsule ring 2 px outside the button; pointer focus draws none.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Move focus to the button |
| `Enter` / `Space` | Run the action (ignored while pending or after a success) |

## Motion

- Pressing scales the button to 0.96 in 100 ms; letting go returns it.
- A status change crossfades the labels: the old one fades out in 150 ms with a 2 px blur, drifting 4 px; the new one
  fades in over 200 ms out of the same blur and settles on a spring. The pending text drops in from above; the success
  text rises from below. The width follows on a critically damped spring.
- On success the button kicks to 1.07 in 130 ms and settles back in 330 ms as the confetti leaves.
- The pieces (ribbons, squares, discs and small triangles) leave from the button's rounded outline, more of them from
  the top edge, outward and slightly up at 260 to 600 px/s. They grow in over 100 ms, then fall under light gravity with
  air drag and a sideways flutter. Ribbons, squares and triangles tumble, squashing along one axis as they flip; discs
  stay round. Each piece fades over the last 350 ms of a life of 1.1 to 1.45 s.
- The pieces are drawn by one layer on one frame loop that stops when the last one has faded.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Press | Scale to 0.96 | None | None |
| Label swap | Fade, blur and drift | 100 ms crossfade | At once |
| Width | Spring | At once | At once |
| Success | Confetti and kick | Check and text only | Check and text only |
| Pending spinner | Turns | Rests and pulses | Still |

## Responsive behavior

- The button keeps its size at every width; only its label decides how wide it is.
- On a narrow screen the confetti reaches the edges of the view. Place the button where clipped pieces still look
  intentional, such as the middle of a card.
- Touch presses get the same press scale and burst as a mouse.

## Related

- `Button` for everyday actions, with its own loading state.
- `ConfirmMorph` for an action that asks for confirmation inline.
- `SlideToConfirm` for a deliberate gesture before a payment or transfer.

| Component | Choose it when |
|---|---|
| `CelebrateButton` | Finishing something worth celebrating |
| `Button` | Routine actions, saves and submits |
| `ConfirmMorph` | An inline confirm step next to other actions |
| `SlideToConfirm` | A deliberate gesture before an important action |
