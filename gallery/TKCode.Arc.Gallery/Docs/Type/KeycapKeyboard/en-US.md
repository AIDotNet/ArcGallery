---
title: Keycap keyboard
summary: Sculpted mechanical keycaps that sink and glow as you type or click, with the typed text above the board.
---

<Demo name="Preview" />

## When to use

`KeycapKeyboard` draws a board of mechanical keycaps with the typed text above it. Real typing and clicks press the
matching cap into its housing, light a soft glow under it and add the character to `Value`.

- **Try-it moments**: landing pages and empty states for keyboard-first products and developer tools.
- **Teaching keys**: onboarding that shows which keys a shortcut uses.
- **Small pads**: a few big keys that type a word or run a macro, built with `Rows`.

## When not to use

- **Form input**: use a text field; the keyboard is a toy, not an editor.
- **On-screen keyboards for accessibility**: use the platform's.
- **Layouts other than a US keyboard**: caps are placed and labelled as on a US board.

## Usage

`KeycapKeyboard` is in the Special package; register it with `UseArcSpecial()` next to `UseArcControls()`. With no
settings it shows a compact 60% board and listens to keys while it has focus. `Tracking="Window"` hears typing anywhere
in the window, except in text fields and shortcuts with Ctrl, Alt or the Command key. `Value` holds the text (two-way)
and `ValueChanged` reports every change. `MaxLength` (280) drops the oldest characters, and `Placeholder` shows while
the text is empty.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:KeycapKeyboard Tracking="Window" Placeholder="Press any key to try it" />
</UserControl>
```

Caps match real keys by their place, so any keyboard layout lights the right cap; the typed character is the one the
system gives. Delete removes the last character, Return starts a new line, and a clicked Shift shifts the next
character, digits and symbols included.

### Letters

`Layout="Letters"` shows three rows of letters with Shift, Delete and a space bar. The space bar is the accent key, as
Return is on the compact board.

<Demo name="Letters" />

### Custom rows

`Rows` replaces the layout with your own rows. Write each row's keys separated by spaces: single characters, or
`space`, `backspace`, `enter`, `shift`, `tab` and `caps`. `ShowsText="False"` hides the text area when you show `Value`
yourself.

<Demo name="MacroPad" />

### Shortcuts

`Shortcut` turns one combination's keys to the accent, written as "Ctrl+Shift+P", "Cmd+K" or "⇧⌘P". Put
`KeycapBinding` items in the keyboard: while exactly a binding's modifiers are held (`ActiveModifiers`), its key prints
its label; with bindings, a clicked modifier holds until the next key. `Platform` prints Windows words on the modifier
keys (ctrl, win, alt; the default is ctrl, opt, cmd), `ShowsFunctionRow` adds Esc and F1 to F12, and `KeyInvoked`
reports every pressed key with its modifiers and binding.

<Demo name="Shortcut" />

### Best practices

**Do**

- Use window listening on a page with nothing else to type into, so the board feels alive at once.
- Keep custom rows short: a pad of three to six keys reads at a glance.
- Name the board with `Label` when it has a job, like "Macro pad".

**Avoid**

- Window listening on screens with passwords: the keys show what is typed.
- Using the text as form input; read `Value` only for show.

## API reference

<!-- api: KeycapKeyboard -->

<!-- api: KeycapRow -->

<!-- api: KeycapBinding -->

## Accessibility

- The board is a group named by `Label`. Every cap is a button named by its character or function (Space, Delete,
  Return, Shift) and is its own tab stop; Enter or Space presses the focused cap.
- The text area is a read-only text box, first in the tab order; focusing it lets real keys type while listening on
  focus.
- Shortcuts with Ctrl, Alt or the Command key pass through untouched, and window listening never types what is meant for
  a text field. The board's help text reads the lit shortcut and the bindings shown.
- Keyboard focus draws a square ring around the focused cap or the text area; pointer focus draws none.

### Keyboard interactions

| Key | Action |
|---|---|
| Any printable key | Types the character and holds its cap down while the key is held. |
| Backspace | Deletes the last character; holding it repeats. |
| Enter | Starts a new line. |
| Shift, Tab, Caps, modifiers | Light their caps. Tab still moves focus. |
| Tab to a cap, then Enter or Space | Presses that cap as a button. |

## Motion

- A cap drops most of its travel in 35 ms when pressed and comes back in 200 ms.
- A soft glow lights the plate under a pressed cap and fades over 700 ms, so fast typing leaves a trail; the cap's rim
  tints with it and its legend brightens.
- New characters fade in out of a slight blur over 150 ms. The caret holds still while you type and blinks again after
  600 ms.
- Switching boards morphs the plate on a spring while the new rows fade in.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Key press | Cap sinks, glow fades in and out | No travel; glow and legend change at once | Cap drops at once |
| New character | Fades in out of a blur | Appears | Appears |
| Caret | Blinks when idle | Steady | Steady |
| Board change | Plate morphs, rows fade in | Jumps | Jumps |

## Responsive behavior

- One key unit sets every measure (gaps, travel, corners and legends) and grows with the width up to 54 px, so the
  widest row always fits; `KeySize` caps it lower.
- The text shrinks from 32 px to 20 px on narrow widths and keeps two lines, scrolled to the newest.
- A tap presses a cap on touch down; it stays down until the finger lifts. Hover is not shown for touch.

## Related

- `ShortcutRecorder` records a shortcut that people type.
- `TypewriterTerminal` types a scripted session on its own.

| Component | Choose it when |
|---|---|
| `KeycapKeyboard` | A playful board that answers typing and clicks |
| `ShortcutRecorder` | Letting people set their own shortcut |
| `TypewriterTerminal` | Playing a terminal session without input |
