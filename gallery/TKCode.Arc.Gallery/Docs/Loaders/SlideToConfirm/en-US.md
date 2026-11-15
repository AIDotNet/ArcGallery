---
title: Slide to confirm
summary: A slider you drag to the end to confirm an action: a liquid fill follows the thumb, an early release springs back, and the arrow turns into a check when the work is done.
---

<Demo name="Preview" />

## When to use

`SlideToConfirm` asks for a deliberate horizontal gesture before an action runs. A white thumb sits at the start of a
rounded track with the instruction beside it. As you drag, a liquid fill follows the thumb a moment behind, thins into a
neck when the thumb pulls ahead and catches up when it slows, while the track swells softly around the pressed thumb.
Let go before the end and the thumb springs back. Let go past the threshold and the track draws into the thumb, a
spinner shows while your task runs, and the arrow turns into a check with the confirmed text beside it.

- **Payments and transfers** on a phone-first screen, where a stray tap must not send money.
- **Unlocking or starting something** in a kiosk or a field app: a door, a shift, a machine run.
- **Any routine action that should not fire by accident** but does not need a dialog to explain what happens.

## When not to use

- **Destructive actions on a desktop screen**: use `HoldToConfirm`, where press and hold is more natural with a mouse.
- **Actions whose consequences people must read first**: use a dialog.
- **Routine actions that are easy to undo**: use a `Button`, with an undo toast if needed.
- **Inline confirmation next to other buttons**: use `ConfirmMorph`.

## Usage

Give the slider its instruction, the texts for the pending and confirmed states, and the work to run. The work is a
`Func<Task>`: while its task runs the slider shows the spinner and `PendingLabel`; when it finishes the check and
`ConfirmedLabel` show; if it throws, `ErrorLabel` replaces the instruction. Without an action the slider goes straight to
the check.

```xml
<arc:SlideToConfirm x:Name="Pay"
                    Label="Slide to pay $86"
                    PendingLabel="Paying"
                    ConfirmedLabel="Paid to Noor"
                    ErrorLabel="Not paid, slide to retry" />
```

```csharp
Pay.ConfirmAction = () => payments.SendAsync(order);
Pay.ResetAfter = TimeSpan.FromSeconds(2.6);
Pay.StateChanged += (_, e) =>
{
    if (e.NewState == SlideToConfirmState.Error)
    {
        log.Warn(e.Error);
    }
};
```

`SlideToConfirm` lives in the Special package: register it with `UseArcSpecial()` next to `UseArcControls()`. It takes
the width it is given (at least 200 px) and sets its own height.

### States

| State | What shows |
|---|---|
| `Idle` | The thumb at the start with the chevron, the instruction in the track |
| `Dragging` | The thumb held by the pointer, or gliding while Space or Enter is held |
| `Pending` | The track drawn into a circle at the end, a spinner on the thumb, `PendingLabel` beside it |
| `Done` | A circle in the fill color with a check, `ConfirmedLabel` beside it |
| `Error` | Back at the start, `ErrorLabel` in the danger color instead of the instruction, until the next slide |

`Confirmed` is raised once when a slide completes, before the action runs. `StateChanged` reports every change, with
the exception when the action failed. `Confirm()` completes a slide from code and `Reset()` returns a confirmed slider to
idle; with `ResetAfter` set it does that by itself.

<Demo name="States" />

### Threshold and hint

`Threshold` (0.9 by default, from 0.5 to 1) is how far along the track the thumb must be when you let go. Past the end
the thumb keeps following on a rubber band, so pulling too far never feels like hitting a wall. While the slider is idle
and nobody has touched it, `IsHintEnabled` nudges the thumb a short way to the right and back every few seconds, so
people see that it moves; a press anywhere on the track away from the thumb plays a smaller nudge.

### Accent

<Demo name="Accent" />

`AccentBrush` recolors the liquid and the success circle. Without it they use the theme's control fill, which follows the
selected accent. The text and the check on the liquid keep the theme's glyph color, so pick an accent dark or saturated
enough to carry them.

### Content guidelines

- **Label**: the action and its object, short enough to fit beside the thumb on a phone: "Slide to pay $86", "Slide to
  unlock door 3".
- **PendingLabel**: one word for the work in progress: "Paying", "Unlocking".
- **ConfirmedLabel**: the result, naming the object when it helps: "Paid to Noor", "Unlocked".
- **ErrorLabel**: what went wrong and that sliding again retries: "Door offline, slide to retry".

### Best practices

- Return the real request's task from `ConfirmAction`, so the pending and error states tell the truth.
- Keep `Threshold` high. Lowering it makes a sloppy drag count, which defeats the purpose.
- Use `ResetAfter` when the same action can run again (a door, a shift); leave it unset when the confirmed state is the
  end of the flow (a payment receipt).

## API reference

<!-- api: SlideToConfirm -->

<!-- api: SlideToConfirmState -->

<!-- api: SlideToConfirmStateChangedEventArgs -->

## Accessibility

- The slider is one focusable element with the slider role, named by `Label` (or `AutomationProperties.Name`). Its value
  is the thumb's position from 0 to 100; the help text says how to use it and reads "Paying" or "Paid to Noor" while
  pending or confirmed.
- A polite live region announces the pending, confirmed and error texts.
- While pending or confirmed the slider is read-only and ignores input.
- Success never relies on color: the arrow turns into a check and the confirmed text is written beside it. A failure
  replaces the instruction with the error text.
- Pointer focus draws no ring. Keyboard focus draws a 2 px capsule ring outside the track and tints the track.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Move focus to the slider |
| `→` `↑` / `←` `↓` | Move the thumb a tenth of the way |
| `Page Up` / `Page Down` | Move the thumb a quarter of the way, to the nearest tenth |
| `Home` | Return the thumb to the start |
| `End` | Confirm |
| Hold `Space` or `Enter` | Glide the thumb to the end and confirm; letting go early springs it back |

The arrow and Page keys never confirm on their own, even at the end.

## Motion

- The thumb follows the pointer on a stiff spring and keeps its speed when you let go, so a flick carries on for a
  moment before it springs back. At the start it presses into the edge, squashes and bounces back slightly.
- The liquid's tail chases the thumb on a softer spring. Between them a neck thins as they pull apart (about 15% of the
  gap), so fast drags stretch the liquid and it catches up when the thumb slows. On an early release the tail overtakes
  the returning thumb and is absorbed into it.
- Pressing swells the thumb by about a pixel and pushes the track outward around it on an underdamped spring, so the
  edge wobbles like a membrane. Speed adds to the swell and stretches the thumb into an ellipse along the drag (at most
  1.36 : 1).
- The label drifts 12 px to the right and fades to 30% as the thumb travels.
- Confirming: the thumb settles at the end, then the track draws into it from the left over about 600 ms. The
  instruction is gone a third of the way in, the pending or confirmed text slides in with the retreating edge, and the
  chevron fades into the spinner. On success the circle turns to the fill color in about 130 ms and the chevron's three
  points glide into a check over about 250 ms. A reset plays the same in reverse.
- One frame loop runs all of it and sleeps as soon as everything has settled. It stops when the slider is off screen.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Dragging | Spring follow, liquid lag and neck, swell, stretch | The thumb follows exactly; no liquid lag, swell or stretch | As reduced |
| Release, keys | Springs | At once | At once |
| Confirm, success, reset, error | Retreat, fades, morph | At once (a still spinner while pending) | At once |
| Idle nudge | Every few seconds | None | None |
| Hover tint | 150 ms | 150 ms | At once |

Holding Space or Enter glides the thumb at full and reduced motion; with motion off it confirms at once. After an
automatic reset the thumb always returns to the start.

## Responsive behavior

- The track is 56 px tall below 360 px of width and 64 px from 360 px on. The thumb keeps a 5 px inset and the label
  grows from 13 to 15 px on the tall track.
- The slider takes the width it is given, at least 200 px. Keep the label short enough to fit beside the thumb at the
  narrowest width you support.
- On touch, only a horizontal drag moves the thumb; a vertical swipe still scrolls the page.

## Related

- `HoldToConfirm` for destructive actions confirmed by holding.
- `ConfirmMorph` for a button that turns into an inline confirmation.
- `ElasticSlider` for a value slider that stretches past its ends.

| Component | Choose it when |
|---|---|
| `SlideToConfirm` | A deliberate horizontal gesture before a routine but important action |
| `HoldToConfirm` | A destructive action on desktop, confirmed by press and hold |
| `ConfirmMorph` | An inline confirm step next to other actions |
