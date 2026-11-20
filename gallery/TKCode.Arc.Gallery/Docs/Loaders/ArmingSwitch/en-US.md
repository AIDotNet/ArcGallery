---
title: Arming switch
summary: A cockpit-style guarded switch for risky actions: lift the translucent cover, throw the chrome toggle until it snaps over and the lamp lights, then hold the lamp to run the action.
---

<Demo name="Preview" />

## When to use

`ArmingSwitch` turns one dangerous action into a short ritual. A brushed aluminium plate carries a chrome bat toggle
under a translucent safety cover, and a lamp with a ring around it. Lift the cover on its hinge, throw the toggle up
until it snaps over centre and the lamp flickers on, then press and hold the lamp until the ring fills. The action runs,
the lamp turns green, and the cover falls shut again, throwing the toggle back to safe on its way down. Closing the
cover at any time makes an armed switch safe, as on a real panel.

- **Production releases, migrations and other one-way operations** that should never start from a stray click.
- **A danger zone in settings** where a single guarded control carries the whole decision and there is room for it.
- **A launch moment or a product page** that shows how seriously a tool treats destructive actions.

## When not to use

- **Quick confirmations in dense lists or toolbars**: use `HoldToConfirm` or `SlideToConfirm`.
- **When people must prove they know what they are removing**: use a dialog that asks them to type its name.
- **Ordinary settings that are safe to flip back and forth**: use a `Switch`.

## Usage

Name the action, say what it touches, and give the work to run. `ConfirmAction` is a `Func<Task<bool>>`: while its task
runs the status is `Running` (the lamp pulses and the ring spins); `true` is a success, `false` or an exception is a
failure. Without an action the hold succeeds at once.

```xml
<arc:ArmingSwitch x:Name="Purge"
                  Title="Purge the archive"
                  Description="cold-storage, 1.2 TB in 18 buckets"
                  HoldLabel="Hold to purge"
                  RunningStatusText="Purging"
                  DoneStatusText="Purged"
                  ArmedHint="Press and hold the lamp to purge the archive."
                  DoneHint="Archive purged. This cannot be undone." />
```

```csharp
Purge.HoldDuration = TimeSpan.FromMilliseconds(1800);
Purge.ConfirmAction = async () =>
{
    var result = await storage.PurgeAsync(archive.Id);
    return result.Succeeded;
};
Purge.StatusChanged += (_, e) =>
{
    if (e.NewStatus == ArmingSwitchStatus.Failed)
    {
        log.Warn(e.Error);
    }
};
```

`ArmingSwitch` lives in the Special package: register it with `UseArcSpecial()` next to `UseArcControls()`. It takes the
width it is given; the plate grows up to 560 px across.

### Statuses

| Status | What shows |
|---|---|
| `Safe` | The cover down, the switch safe, the lamp dark, the ready light beating |
| `Open` | The cover flipped up; the switch can be thrown |
| `Armed` | The switch up and the lamp lit amber, breathing; holding the lamp fills the ring |
| `Running` | The ring spins and the lamp pulses while the task from `ConfirmAction` runs |
| `Done` | The lamp green; with `IsAutoSafeEnabled` the cover drops a moment later and throws the switch to safe. Green stays until the cover is lifted again |
| `Failed` | The lamp blinks red, the panel shakes and the red ring drains; the switch stays armed, so holding again retries |

The chip beside the title pairs an icon with the status text, and the line under the panel says what to do next; both
can be renamed per status (`SafeStatusText` … `FailedStatusText`, `SafeHint` … `FailedHint`). `StatusChanged` reports
every change, with the exception when the action threw. `CoverOpenChanged` and `ArmedChanged` follow the cover and the
switch, including when a falling cover pushes the switch back. `Confirmed` is raised when the ring fills, before the
action runs.

`OpenCover()`, `CloseCover()`, `Arm()` and `MakeSafe()` drive it from code. `IsLocked` keeps every part focusable but
refuses them: attempts rattle the cover and the hint says the switch is locked.

### Accent and cover

The hold ring uses the theme's accent and the cover is tinted from it, kept in a lightness band so it stays
translucent in both themes. `AccentBrush` recolors the ring and `CoverBrush` the cover and the tinted shadow it casts,
for example a safety red for a delete.

### Content guidelines

- **Title**: the action, not the product: "Ship to production", "Purge the archive".
- **Description**: what it touches, in one line: the service and version, the regions, the size.
- **HoldLabel**: "Hold to" and the verb; it is also the lamp's accessible name.
- **Status texts and hints**: rename them together for actions other than a release, so the chip says "Purging" while
  the hint explains what is happening.

### Best practices

- It is friction, not authorization. Check permissions on the server inside `ConfirmAction` and return `false` when the
  action does not go through.
- Return the real task, and finish it only when the work has really finished, so `Running` tells the truth.
- Leave `IsAutoSafeEnabled` on for one-shot actions. Turn it off when people may run the action again right away.

## API reference

<!-- api: ArmingSwitch -->

<!-- api: ArmingSwitchStatus -->

<!-- api: ArmingSwitchStatusChangedEventArgs -->

## Accessibility

- The control is a group named by `Title`. The cover, the switch and the lamp are real focusable elements laid over the
  drawing, so pointer, touch, keyboard and screen readers reach the same three parts; the plate itself is hidden.
- The cover is a button named "Lift the safety cover" or "Close the safety cover". The switch is a toggle named after
  `ArmLabel` ("Arm switch") and reports whether it is armed. The lamp is named by `HoldLabel`.
- Each part's help text is the line under the panel, so each says what to do next. Refusals such as "Lift the cover
  first." replace that line for about two seconds and are announced.
- A polite live region announces every status change with the next step, for example "Armed. Press and hold the lamp
  to ship."
- The status never relies on color: the chip pairs an icon with the text, and its width does not change between them.
- Keyboard focus draws a 2 px rounded ring around the focused part.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Move between the cover, the switch and the lamp, in that order |
| `Enter` / `Space` on the cover | Lift the cover, or close it (closing makes an armed switch safe) |
| `↑` / `↓` on the cover | Lift or close the cover |
| `Enter` / `Space` on the switch | Arm the switch or return it to safe; under a closed cover the bat knocks the lid and the hint asks to lift the cover first |
| `↑` / `↓` on the switch | Arm or safe the switch |
| Hold `Enter` or `Space` on the lamp | Fill the ring for `HoldDuration`, then run the action; letting go early drains it |
| `Escape` | Make everything safe: end a hold, close the cover, safe the switch (ignored while the action runs) |

## Motion

- One frame loop moves every part on springs (at 240 Hz) and sleeps when nothing moves. The panel leans a few degrees
  towards the pointer on a soft spring; the light stays fixed to the room, so the streaks on the brushed plate drift,
  the glare slides down the lifting lid and the shadows slide and shorten.
- The cover is a translucent 3D box on a hinge. Dragging it keeps its lip under the pointer, rubber-bands past the open
  stop and keeps the release speed. It is spring-loaded into its stops: it snaps open in about 200 ms with a small
  rebound and falls shut with a short hop. Hovering or focusing it lifts it 5°, and it hops once when it first appears.
- The bat follows the pointer one to one until it passes centre, where the over-centre spring slams it into the far stop
  in about 65 ms. Let go before centre and it snaps back.
- Arming lights the lamp like a filament: two quick flickers, a bloom and a warm spill across the plate, then a slow
  breath. Holding presses the lens in, fills the ring with a bright leading dot and brightens the lamp.
- When the ring fills, a ring pops off the lamp and the panel nudges. Success crossfades the lamp to green with a wider
  ring and a glint across the plate; failure blinks it red, shakes the panel and drains a red ring.
- The loop stops when the control is off screen.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Cover, switch | Springs, rebounds, hops | Jump to the new pose | Jump |
| Panel lean, drift, ready light, breath | Yes | None | None |
| Lamp | Flicker, bloom, pulse while running | Fades, slower pulse | Steady |
| Hold ring | Fills with a short lag, drains on a spring | Fills, drains linearly | Fills, drains linearly |
| Bursts, glint, shake | Yes | None | None |

The hold still fills at every motion level, so the action always keeps its safeguard.

## Responsive behavior

- The plate is drawn in its own units, so the cover, the bat, the lamp, the screws and the engraving keep their
  proportions from a phone to a desktop card, up to 560 px across. The engraving never drops below 9 px.
- From 720 px of width the title, status and hint sit in a column beside the panel. Below that they stack, the card
  drops its frame and a line separates the hint; below 420 px the padding tightens.
- The stage keeps room above the plate for the flipped cover, so it never covers the heading.

## Related

- `HoldToConfirm` for a compact press-and-hold confirmation.
- `SlideToConfirm` for a deliberate horizontal gesture before a routine action.
- `ConfirmMorph` for a destructive button that turns into an inline confirmation.

| Component | Choose it when |
|---|---|
| `ArmingSwitch` | One costly, hard-to-undo action that deserves a two-step ritual and the space for it |
| `HoldToConfirm` | A destructive action confirmed by holding, in a small space |
| `SlideToConfirm` | A routine but important action on a touch screen |
