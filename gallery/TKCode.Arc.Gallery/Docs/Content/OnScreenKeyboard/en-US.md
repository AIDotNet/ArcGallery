---
title: On-screen keyboard
summary: A touch keyboard for kiosks and panels without a physical one: QWERTY, a number pad, a PIN pad or a layout of your own, typing into the focused field without taking its focus.
---

<Demo name="Preview" />

## When to use

`OnScreenKeyboard` draws a keyboard on the screen and types into the text input that has focus in the same window. Use
it where people only have a touch screen and the system keyboard cannot be relied on: it may not open on its own (a
Windows kiosk), or there is none at all (an embedded Linux panel).

- **Self-service kiosks**: check-in, ordering or ticket machines where a visitor types a name, an email or a booking code.
- **Touch panels and industrial HMIs**: an operator enters a batch number, a quantity or a set point on a panel PC with
  no keyboard attached.
- **PIN and code entry**: a door panel or a till asks for a PIN. The PIN pad can feed your own display, so the digits
  never appear as text.
- **Quantity and price fields**: the number pad has the culture's decimal separator, Backspace and caret keys, and
  makes a quantity field quick to fill with a thumb.
- **Special keypads**: a hex keypad, a pad of units or a keyboard for another language, built from the same layout model.

## When not to use

- **Phones and tablets**: the system keyboard is better there (prediction, voice input, accessibility settings). Ask
  for the right kind of keyboard with `TextInputOptions` on the text box instead.
- **Desktops with a physical keyboard**: a second keyboard on the screen only takes room.
- **Picking a value from a range**: use a `Slider` or the step buttons of `NumberField`, which need no typing at all.
- **Choosing from a known list**: use `Select`, `Combobox` or `SegmentedControl`; tapping an option beats typing it.

## Usage

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <DockPanel>
        <arc:OnScreenKeyboard DockPanel.Dock="Bottom" MaxWidth="720" />
        <StackPanel Spacing="12" Margin="24">
            <arc:Input Label="Name" />
            <arc:Input Label="Email" />
        </StackPanel>
    </DockPanel>
</UserControl>
```

Tap a field, then the keys. The keyboard types into whichever text input has focus in its window: an Arc `Input`,
`Textarea`, `PasswordField` or `NumberField`, an Avalonia `TextBox`, or a control of your own that handles text input.
Pressing a key never moves focus, so the caret stays where it was and Tab or a tap on another field moves on as usual.

How each key reaches the field:

- **Letters, digits, symbols and the space bar** are sent as text input, the same event a system keyboard or an input
  method raises. A text box inserts the text at the caret, replaces a selection, and keeps `MaxLength`, `IsReadOnly`
  and its undo history.
- **Backspace, Enter, Tab and the arrow keys** are sent as key presses. The text box handles them itself: Enter starts a
  new line in a multi-line box, and in a single-line box it bubbles on to a default button or your own `KeyDown`
  handler. Tab moves focus to the next field.

A key acts when it is pressed, not when it is released, so fast typing with two fingers comes out in the right order.

### Anatomy

1. **Tray**: the muted panel behind the keys, with a 1 px subtle border, a 20 px corner and 8 px of padding.
2. **Character keys**: the lightest keys, with a 1 px edge and a 2 px bottom lip, a 12 px corner and a 22 px label.
3. **Function keys**: Shift, Backspace, the page keys and the arrows, one step darker. Labels longer than one character
   ("123", "ABC") use the small medium-weight size.
4. **Enter**: the primary key, filled with the foreground color like a primary button.
5. **Rows**: every row is centred, so a row with fewer key units than the widest is indented on both sides (the QWERTY
   home row sits half a key in). Keys are 46 px tall with 6 px between keys and 8 px between rows.

### Number and PIN pads

`Preset` picks a built-in layout. `Numeric` is a number pad with the decimal separator of `Culture` (or the current
culture), Backspace, Left and Right to move the caret, and Enter; `SignedNumeric` adds a minus key; `Digits` is a PIN pad
with digits, Backspace and Enter only. A pad stretches its keys to the size you give it.

<Demo name="NumberPad" />

```xml
<arc:OnScreenKeyboard Preset="Numeric" Culture="de-DE" Width="280" Height="260" />
```

Give the pad the same culture as the field it fills: a `NumberField` parses with its own `Culture` (en-US unless you set
one), so a German comma would not be understood there.

### Handling keys yourself

`ItemInvoked` is raised for every key press, and for every repeat of a held key, before the keyboard acts on it. Its
`Item` is the key, `Text` the text it is about to type (with Shift applied), `Target` the element that would receive it
and `IsRepeat` whether the key is being held. Set `Handled` to true and the keyboard does nothing else: it types nothing
and leaves Shift and the page as they are. The PIN pad below feeds four dots and never shows the digits as text.

<Demo name="PinPad" />

```csharp
keypad.ItemInvoked += (_, e) =>
{
    e.Handled = true;
    switch (e.Item.Kind)
    {
        case OnScreenKeyboardKeyKind.Text:
            pin.Append(e.Text);
            break;
        case OnScreenKeyboardKeyKind.Backspace when pin.Length > 0:
            pin.Length--;
            break;
        case OnScreenKeyboardKeyKind.Enter:
            await UnlockAsync(pin.ToString());
            break;
    }
};
```

Handle a few keys and let the rest through when you only want to add something, such as a `Custom` key "Clear" that
empties the field. `Invoke(key)` presses a key from code, as if it were tapped.

### Opening a keyboard under a field

Instead of a docked keyboard, set `OnScreenKeyboard.Popup` on a field. When the field gets focus a keyboard of that kind
opens under it (or above it, when there is no room below) and types only into it. It closes when focus leaves the field,
after Enter in a single-line field, and on a `Hide` key; tapping the field opens it again. `OnScreenKeyboard.PopupLayout`
opens your own layout instead.

<Demo name="Popup" />

```xml
<arc:NumberField Label="Quantity" arc:OnScreenKeyboard.Popup="Numeric" />
<arc:PasswordField Label="Door code" arc:OnScreenKeyboard.Popup="Digits" />
```

### Custom layouts

A layout is plain data: an `OnScreenKeyboardLayout` holds pages, a page holds rows and a row holds keys. Each
`OnScreenKeyboardKey` says what it does (`Kind`), what it types (`Text`, and `ShiftText` while shifted), what it shows
(`Label`, or a 24 × 24 Lucide `Glyph`) and how wide it is (`Width`, in standard keys). Keys can be bound and changed while
shown: a disabled key is dimmed and does nothing, a hidden key takes no room and its row closes up.

<Demo name="CustomLayout" />

| Kind | What it does |
|---|---|
| `Text` | Types `Text`, or `ShiftText` (otherwise `Text` in upper case) while shifted |
| `Space` | Types a space |
| `Backspace` | Deletes the character before the caret or the selection; repeats while held |
| `Enter` | A new line in a multi-line box; otherwise bubbles on as a key press |
| `Tab` | Moves focus to the next field, or back with Shift on |
| `Shift` | Shifts the next key; a double tap locks capitals |
| `Left`, `Right`, `Up`, `Down` | Move the caret (with Shift on, extend the selection); repeat while held |
| `Page` | Switches to the page named by `TargetPage` |
| `Hide` | Raises `HideRequested`; a keyboard opened by `Popup` closes |
| `Key` | Sends `KeyCode` (Escape, Delete, Home...) as a key press |
| `Custom` | Nothing by itself: handle `ItemInvoked` |
| `Spacer` | An empty gap of `Width` units |

`OnScreenKeyboardLayout.Create(preset, culture)` and `CreateQwerty`, `CreateNumeric` and `CreateDigits` return the
built-in layouts as new objects you can change before you use them. `OnScreenKeyboardRow.FromCharacters("qwertyuiop")`
makes a row of text keys from a string.

### Other languages

The built-in QWERTY is English. Other languages are layouts like any other: start from the built-in one and replace its
letter rows, or write every page yourself. The keyboard puts letters in upper case with the rules of its `Culture`, so
German, Turkish or Greek capitals come out right.

<Demo name="Languages" />

```csharp
var layout = OnScreenKeyboardLayout.CreateQwerty();
var letters = layout.Pages[0].Rows;
letters[0] = OnScreenKeyboardRow.FromCharacters("qwertzuiopü");
letters[1] = OnScreenKeyboardRow.FromCharacters("asdfghjklöä");
keyboard.Culture = CultureInfo.GetCultureInfo("de-DE");
keyboard.Layout = layout;
```

Keys type text, so any script works that the text box can show. Languages that compose text from candidates, such as
Chinese pinyin or Japanese kana to kanji, need an input method and are not covered by the keyboard.

### Shift, caps lock and pages

One tap on Shift capitalises the next key (`ShiftState` is `On`); a second tap turns it off. Two taps in quick succession
lock capitals (`Locked`) until Shift is tapped again. Caps lock, like a physical one, changes letters only; one-shot Shift
also gives digits and symbols their `ShiftText`. The Shift key fills its glyph while on and adds a bar under it while
locked, so the state never rests on color alone. `ShiftState` is two-way bindable.

The QWERTY layout has three pages: letters, numbers and symbols ("123"), and more symbols ("#+="). `CurrentPage` names the
page shown (`OnScreenKeyboardLayout.LettersPage`, `SymbolsPage`, `MoreSymbolsPage`) and can be set or bound. Switching
pages turns one-shot Shift off and keeps caps lock.

### Target instead of focus

Set `Target` to send every key to one element, focused or not. Use it when the keyboard belongs to one field, or when a
press elsewhere on the page would otherwise take focus away. Without a target and with nothing focused, the keys only
raise `ItemInvoked`.

```xml
<arc:Input x:Name="Code" Label="Booking code" />
<arc:OnScreenKeyboard Target="{Binding #Code}" />
```

### Key repeat

Holding Backspace or an arrow key repeats it after `RepeatDelay` (500 ms) every `RepeatInterval` (50 ms) until it is
released. Other keys do not repeat; set `IsRepeatable` on a key to change that. The clock is `TimeProvider`, so a test can
drive repeats with a clock of its own.

### Sizing

Without a size the keyboard is as wide as its widest row at 52 px per key (about 530 px for QWERTY, tray included) and
as tall as its rows at 46 px each. Given more room, the keys grow: the width is shared by key units and the height by rows. On a wide screen set
a `MaxWidth` (about 720 to 900 px for QWERTY) so the keys keep a comfortable shape. Docked to the bottom of a window, set
`CornerRadius="0"` and `BorderThickness="0,1,0,0"` for a flat tray.

### Content guidelines

- **Labels**: keep key labels short. One character keeps the large size; longer labels ("Done", "Clear") use the small
  size and are cut with an ellipsis when the key is too narrow.
- **Names**: give keys that show a glyph or a symbol an `AutomationName` ("Minus", "Decimal separator", "Hide keyboard").
- **Field labels**: the field still needs its own label and hint; the keyboard does not say what to type.

| Instead of | Write |
|---|---|
| A key labelled "OK" that only closes | `Kind="Hide"`, named "Hide keyboard" |
| "Enter" spelled out on the key | The Enter glyph, or a verb such as "Done" or "Search" |
| A symbol key with no name | `AutomationName="Degree sign"` |

### Best practices

**Do**

- Give number and PIN pads a size that suits a thumb, about 260 to 300 px wide.
- Match `Culture` with the field's culture for decimal input.
- Handle `ItemInvoked` for PINs and codes that must not appear as text.
- Set `MaxWidth` on a keyboard that stretches across a wide screen.

**Avoid**

- Showing the on-screen keyboard on devices that have a system keyboard.
- Putting focusable controls inside a custom key template or around the keyboard that a press could focus by mistake.
- Using the keyboard for choices a list or a slider would make faster.

### Common questions

**Why does the keyboard type nothing?** Nothing has focus and no `Target` is set. Tap a field first, set `Target`, or
handle `ItemInvoked` when nothing has focus (the preview above starts in its first field).

**Does pressing a key steal focus from my text box?** No. The keyboard claims every press inside it before Avalonia's
focus handling sees it, and keys are not focusable.

**Do Backspace and the arrows work in a plain `TextBox`?** Yes, with a theme that gives `TextBox` a template (Fluent or
Simple). Arc's own text inputs work in every app; without a template, a plain `TextBox` still receives typed text.

**Can I listen for the letter keys in `KeyDown`?** Letters are sent as text input, not as key presses, because most
characters have no key code. Handle `ItemInvoked` instead.

**Can I make keys press on release?** No. Keys act on press so that fast typing keeps its order and held keys can repeat.

## API reference

<!-- api: OnScreenKeyboard -->

<!-- api: OnScreenKeyboardKey -->

## Accessibility

- The keyboard is a group named "Keyboard"; set `AutomationProperties.Name` to name it after its purpose ("PIN pad").
- Every key is a button that screen readers name after its `AutomationName`, its label, or its kind ("Shift",
  "Backspace", "Enter", "Space", "Left"); Invoke presses it. Shift is also a toggle that reports whether it is on.
- Shift changes are announced politely ("Shift on", "Caps lock on", "Shift off", from `ShiftOnText`, `CapsLockOnText` and
  `ShiftOffText`), and so is the page switched to (the page's `Label`, such as "Numbers and symbols").
- Shift and caps lock change the glyph as well as the fill, and disabled keys are dimmed and drawn in the muted text color.
- Keys are 46 px tall at their natural size and grow with the space they are given.

### Keyboard

The on-screen keyboard is the keyboard: its keys take no focus and it has no keyboard interaction of its own. A physical
keyboard keeps working in the focused field as usual.

| Key on screen | Sent to the field |
|---|---|
| Letters, digits, symbols, space | Text input at the caret |
| `Backspace` | `Back` key press (repeats while held) |
| `Enter` | `Enter` key press |
| `Tab` / Shift on + `Tab` | `Tab` / `Shift+Tab` key press |
| Arrows / Shift on + arrows | Arrow key press / with `Shift` (extends the selection) |

## Motion

- A pressed key sinks to 95 % on a snappy spring and takes its pressed fill; it springs back on release.
- Hover (mouse only) and press fills change with a quick fade.
- Labels, Shift and pages change at once.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Key press | Scale to 95 % on a snappy spring, fill fades (120 ms) | Fill fades, no scale | Immediate |
| Hover (mouse) | 120 ms fill fade | 120 ms fill fade | Immediate |
| Shift, caps lock, page | Immediate | Immediate | Immediate |
| `Popup` keyboard opening | The popup's rise and fade | Fade | Immediate |

## Responsive behavior

- The keys share the width the keyboard is given; at a phone width a QWERTY keeps all ten keys of its top row, with
  narrower keys whose corners shrink to stay key-shaped.
- The keyboard is as tall as its rows unless it is given a height, which the rows then share.
- Nothing depends on hover: every key works with a tap, and hover only adds a fill for a mouse.
- A `Popup` keyboard opens under its field, or above it when there is no room below.

## Related

- `NumberField` for numbers with step buttons, and `Input`, `Textarea` and `PasswordField` as the fields it types into.
- `OtpInput` (Blocks) for one-time codes, which a `Digits` pad can feed.
- `ShortcutRecorder` for recording key combinations from a physical keyboard.

| Component | Choose it when |
|---|---|
| `NumberField` | A bounded number set with steps, a wheel or scrubbing |
| `Slider` | An approximate value on a range |
| `Select` | One option from a known list |
| `OtpInput` | A one-time code in separate boxes |
| `ShortcutRecorder` | A key combination from a physical keyboard |
