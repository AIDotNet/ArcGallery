---
title: Mobile text field
summary: A 44 px filled text field with an optional icon and a clear button, whose border takes the accent color while focused.
---

<Demo name="Basic" />

## When to use

`TextField` is a single-line text input for a mobile form: names, emails, short answers.

- **Forms on a phone**: the field is 44 px high, so it is easy to hit with a thumb.
- **Fields with a meaning**: an `Icon` before the text says what the field is for.
- **Fields that people correct often**: the clear button empties it in one tap.

The live demo runs inside a phone frame, a desktop simulation; the soft keyboard on a real device has not been verified here.

## When not to use

- **Search**: use `Searchbar`, which has the search glyph, a pill shape and a Search key.
- **Long, multi-line text**: a text area is the right control, and this one is single line.
- **A choice from a few values**: use a switch, a segmented control or a sheet with a list.

## Usage

`TextField` derives from Avalonia's `TextBox`, so `Text`, selection, `IsReadOnly` and binding work as they do there. Show a hint with `PlaceholderText`.

```xml
<arcm:TextField xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                PlaceholderText="Name"
                Icon="{x:Static arc:ArcIcons.User}"
                Text="{Binding Name}" />
```

The clear button shows only while the field has focus and text, and not when it is read only or `ShowClearButton` is false. It empties the field and keeps the focus. A field has no hover state: a finger does not hover.

### Long-press menu

A long press on the field opens a `TextEditMenu` above it: Cut, Copy, Paste and Select All, as on a phone. The menu
shows only the actions that apply. Cut and Copy need a selection, so a password field (`PasswordChar` set) never offers
them, and a read-only field offers neither Cut nor Paste. Select All leaves once everything is selected, and after Select
All the menu comes back for the new selection. Dragging a selection handle opens the menu over the selection. The menu
does not take focus, so the soft keyboard stays up; a tap outside, typing or moving the focus away closes it. Its text
follows the app language (en-US, zh-CN, zh-TW).

The theme sets the menu as the field's `ContextFlyout`, and `Searchbar` has it too. Set your own `ContextFlyout` or
`ContextMenu` on a field to replace it, or set `<arcm:TextEditMenu />` as the `ContextFlyout` of another `TextBox` to
give it the same menu.

On iOS, add the `TKCode.Arc.Mobile.iOS` package and derive the app delegate from `ArcAppDelegate<App>`. A long press
then opens the system edit menu (iOS 16 and later), the keyboard's "Paste from …" suggestion pastes into the field, and
pasting does not bring up the "Allow Paste" prompt. Without it, the Arc menu shows on iOS as everywhere else.

```csharp
[Register("AppDelegate")]
public partial class AppDelegate : ArcAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseArc(arc => arc.UseArcMobile());
}
```

### Tokens

`TextFieldToken` holds the geometry: `FieldHeight` (44), `ContentPadding` (12 at the sides), `BorderWidth` (1), `GlyphExtent` (18), `IconMargin` and `DisabledOpacity` (0.5). `TextEditMenuToken` holds the menu's `ItemHeight` (44), `ItemPadding` (14 at
the sides) and `AnchorGap` (8, between the menu and the field). Colors, radii and type come from the ArcDesign tokens.

## API reference

<!-- api: MobileTextField -->

## Accessibility

Give every field a visible name next to it or an icon plus a placeholder, so the hint is not the only label once the person starts typing. The field is at least 44 px high and the clear button is a 44 px touch target.

## Motion

The border changes color on the fast curve when the field gains or loses focus. The clear button pops in from scale 0.6 with a fade (P26 through `PresenceHost`) and leaves faster than it arrived. With reduced motion the border changes at once and the clear button appears and disappears at once.

## Responsive behavior

- `TextField` has no breakpoint. It fills the width it is given, and its height is at least 44 px (`FieldHeight`), so
  let it stretch rather than setting a fixed `Width`.
- Text does not wrap: `TextWrapping` is `NoWrap` and the horizontal scroll bar is hidden, so a long value scrolls
  sideways inside the field.
- The clear button (`ShowClearButton`) shows only while the field has focus and text, and it is a 44 px touch target.
- Give each field a visible name next to it, since `PlaceholderText` disappears once the person starts typing.

## Related

- `MobileSearchbar`: a text field made for search.
- `MobilePage`: the screen a form sits in.
- `MobileButton`: the usual way to submit a form.
