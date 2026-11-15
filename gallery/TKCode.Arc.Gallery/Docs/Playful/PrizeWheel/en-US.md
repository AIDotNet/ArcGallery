---
title: Prize wheel
summary: A prize wheel you flick or wind up from its hub, with real momentum, a pointer that bends against every peg, weighted prizes, and the won slice lifting into a reward card with a code to copy.
---

<Demo name="Preview" />

## When to use

`PrizeWheel` turns handing out a reward into a small moment. A visitor flicks the wheel, or presses and holds the metal
hub to wind it back and lets go, and the wheel keeps the speed it was thrown with. A pointer at the top bends against
every peg and snaps back, the wheel slows into a long, suspenseful settle, and it lands on a prize drawn by weight. The
won slice fills with the accent and lifts into a reward card with a code to copy, beside a list of every prize and its
chance.

- **Referral and launch pages** that reward a signup with a discount code.
- **In-product moments** that grant one of several perks, such as a milestone reward.
- **Events and giveaways** where the spin itself is part of the fun.

## When not to use

- **Everyone gets the same reward**: show a plain coupon card or a `CopyButton`.
- **Serious flows** such as billing or checkout, where chance feels out of place.
- **Draws with legal weight**: decide those on a server with an audited process, then let the wheel show the answer.
- **More than about ten prizes**: labels no longer fit. Group small prizes into one slice.

## Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:PrizeWheel SizesByWeight="False" MaxWidth="880">
        <arc:PrizeWheelSegment Label="10% off" Weight="50" Detail="On your first order" Code="HELLO-10" />
        <arc:PrizeWheelSegment Label="Free delivery" Weight="35" Detail="On any order this month" Code="HELLO-SHIP" />
        <arc:PrizeWheelSegment Label="Gift box" Weight="15" Detail="Added to your next order" Code="HELLO-GIFT" IsFeatured="True" />
    </arc:PrizeWheel>
</UserControl>
```

`PrizeWheel` is a control in the Special package. Register the package with `UseArcSpecial()` next to
`UseArcControls()`. Its content is the slices, clockwise from the top: `PrizeWheelSegment`s written inline or added to
`Segments` from code. Each has a `Label` drawn on the wheel (about 14 characters at most), a `Detail` and a `Code` for
the reward card, a `Weight` and, for the rare top prize, `IsFeatured`, which tints its slice and label with the accent and
puts a dot after it in the odds list.

The wheel sits beside a panel: a `Title`, a `Description` and a gesture hint before the first spin, the reward card after
it, and the odds list. The panel sits beside the wheel when the control is at least 736 px wide and under it otherwise.
Set `IsPanelVisible` to false for the wheel alone, and `IsOddsVisible` to false to hide the odds.

### Odds and slice widths

`Weight` is a slice's odds relative to the others (1 by default): a slice with weight 30 wins thirty times as often as
one with weight 1, and a slice with weight 0 never wins. The odds list shows each slice's share of the total, rounded.
`SizesByWeight` is true by default, which makes every slice as wide as its odds; set it to false for equal slices, as
the examples here do. Slices too thin for a label drop it, and the odds list still names them.

### Spinning

A visitor can spin the wheel three ways:

- **Flick it**: the wheel follows the pointer around its centre, and the speed of the last 100 ms becomes the spin. A
  flick slower than about 260 degrees per second does not spin: the wheel coasts back into its drift and the hint turns
  to `WeakHintText`.
- **Hold the hub**: a ring fills over 1.1 seconds while the wheel winds back up to 14 degrees, and letting go throws it
  harder the longer it was held. Enter and Space held on the focused wheel do the same.
- **Spin again** in the reward card, or assistive technology invoking the wheel, spins at a medium strength.

Before the first spin the wheel drifts slowly; `DriftSpeed` scales the drift (0 holds it still) and `IsDriftPaused`
stops it. While it turns, `IsSpinning` is true and other spins are ignored. When it rests, `ResultIndex` and `Result`
hold the winner, `SelectionChanged` and `SpinCompleted` fire, and the reward card appears. Copying the code raises
`CodeCopied`. `Ticked` fires each time a peg passes the pointer, for a click sound or a haptic tick.

`Spin()` and `SpinTo(index)` start a spin from code. These follow `Duration` (4.8 s) and turn at least `Turns` times
(5); gestures last as long as their speed carries them. `ClearResult()` takes the result and the card away.

### A winner chosen by your server

For real prizes, draw the result on a server so the app cannot choose its own reward. Give the wheel a `WinnerPicker`
that returns the server's answer: the visitor still spins it, and it lands there. With `IsSingleSpin` the wheel allows
one spin: it locks on the prize, the hub reads `ClaimedText` and "Spin again" is hidden until `ClearResult()`.

<Demo name="Server" />

```csharp
wheel.WinnerPicker = segments => drawFromServer;
```

### The wheel alone

With `IsPanelVisible` off the control is only the wheel, square, up to 480 px. With `IsInteractive` off nobody can spin
it, so a wheel for show can land on a winner chosen elsewhere with `SpinTo`.

<Demo name="WheelOnly" />

### Best practices

**Do**

- Keep labels short and the wheel to six to ten slices.
- Mark only the rarest prize as featured.
- Keep the odds visible for promotions that must disclose chances.
- Draw valuable prizes on the server, with `WinnerPicker` and `IsSingleSpin`.

**Avoid**

- Slices whose width does not match their odds without saying so: people read width as chance.
- Making people spin to get something they were already promised.

### Common questions

**Is the spin random?** Without `WinnerPicker` the wheel draws with `Random.Shared` by the slices' weights. For
anything that matters, draw on the server.

**Why does the wheel land in a slightly different place each time?** It stops at a random spot inside the inner three
quarters of the winning slice, so it never looks staged.

**How do I keep the old wheel-only look?** Set `IsPanelVisible` to false.

## API reference

<!-- api: PrizeWheel -->

<!-- api: PrizeWheelSegment -->

<!-- api: PrizeWheelResultEventArgs -->

<!-- api: PrizeWheelSelectionChangedEventArgs -->

## Accessibility

- The wheel is a button named by `Label` ("Prize wheel" by default); it is the only Tab stop until a prize lands, and the
  reward card's Copy and "Spin again" buttons follow it. Assistive technology can invoke it, which spins at a medium
  strength.
- The wheel's drawing is an image named with every prize and its chance, so the odds are never visual only.
- A polite live region announces `SpinningText` when a spin starts, then `WonAnnouncement` with the prize and
  `CodeAnnouncement` with the code spelled out letter by letter, and the result of a copy.
- The winner is never shown by color alone: the card names it, its odds row lights up and the announcement repeats it.
- Keyboard focus shows a round focus ring around the hub; pointer focus shows none.

### Keyboard

| Keys | Action |
|---|---|
| `Tab` | Focus the wheel, then the card's buttons |
| `Enter` / `Space` (hold) | Wind the wheel up while held; let go to spin |

## Motion

- A spin slows on a cubic ease out, so its speed fades smoothly and the last turn crawls past the final pegs. A flick
  leaves the hand at exactly its speed; a spin from the hub, a key or "Spin again" builds its speed over 100 ms.
- The landing is the spot in the winning slice nearest the distance the throw would carry: at least two and a half
  turns, and further, and longer, the faster the throw (up to about seven seconds).
- The pointer is a tapered capsule on a stiff, lightly damped spring. It moves only when a peg touches it, bends up to
  about 30 degrees until the peg clears its tip, then snaps back with a short wobble; fast spins rattle it and the
  settle ticks peg by peg.
- Holding the hub fills the ring and winds the wheel back with an ease in and out, and presses the hub to 95 %.
- When the wheel rests the won slice fills with the accent and the other labels dim over 360 ms, the idle text fades
  out, and the reward card flies from the slice to its place on a spring with a little life.
- One frame loop drives the wheel, the pointer and the card; it stops when everything rests and pauses while the wheel
  is out of view.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Drift | Slow turn before the first spin | None | None |
| Spin | Turns and slows to the winner | Lands at once | Lands at once |
| Pointer | Bends and wobbles on the pegs | Still | Still |
| Won slice | Fills over 360 ms | Fills over 360 ms | Filled at once |
| Reward card | Flies out of the slice | Appears in place | Appears in place |

## Responsive behavior

- The panel sits beside the wheel from 736 px of the control's own width, and under it below that, where the odds list
  folds into two columns when it has room.
- The wheel scales with its width up to 480 px; its lines, pegs and labels scale with it. The hub's word is a little
  smaller on wheels under 420 px.
- On a narrow card the Copy button moves under the code.
- The wheel captures the pointer while it is dragged; the rest of the page scrolls normally.

## Related

- `SlotText` rolls text and numbers into place on slot machine reels.
- `CopyButton` copies a value with an in-place confirmation.
- `TicketTear` is another playful way to hand over a reward.

| Component | Choose it when |
|---|---|
| `PrizeWheel` | A reward should feel won, with suspense |
| `CopyButton` | Everyone gets the same code |
| `TicketTear` | A reward is redeemed with a gesture |
