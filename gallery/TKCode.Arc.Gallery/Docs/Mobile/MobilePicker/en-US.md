---
title: Mobile picker
summary: A list row that shows the current value at its end and opens an action sheet of choices with the current one checked.
---

<Demo name="Basic" />

## When to use

`Picker` is the settings-screen way to choose one value from a short list: the row shows what is chosen, and a tap opens the choices from the bottom edge.

- **A setting with a few named values**: language, text size, a default calendar.
- **A form field whose choices fit in a sheet**: a country of a short list, a currency, a size.
- **A row among other rows**: it sits in a `ListGroup` next to `ListItem`s and switches, and looks like them.

The live demos run inside a phone frame, a desktop simulation; touch feedback and the sheet's drag on a real device have not been verified here.

## When not to use

- **Two or three choices that should all stay visible**: use `MobileSegmented` or `MobileRadio`.
- **A choice that reads better as pictures**: use `MobileRadioCards`.
- **Many choices, or choices that need a search**: push a page with a `ListGroup` and a `Searchbar` instead; an action sheet with more than about eight rows scrolls.
- **Actions rather than a value**: use `MobileActionSheet` directly.

## Usage

Put the picker in a `ListGroup`. Its content is the row's title; the choices are `Items` written inline or `ItemsSource`. `SelectedIndex` and `SelectedItem` (both two-way) follow each other. A tap opens an action sheet of the choices with the current one checked; choosing a row selects it and closes the sheet, and Cancel closes it without a change.

```xml
<arcm:ListGroup xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                Header="General">
    <arcm:Picker Icon="{x:Static arc:ArcIcons.Languages}" Content="Language"
                 ItemsSource="{Binding Languages}" SelectedItem="{Binding Language}" />
    <arcm:Picker Content="Text size" SheetMessage="Applies to every screen">
        <arcm:Picker.Items>
            <x:String>Small</x:String>
            <x:String>Medium</x:String>
            <x:String>Large</x:String>
        </arcm:Picker.Items>
    </arcm:Picker>
</arcm:ListGroup>
```

The picker opens its sheet over the `Page` it is in, the way a sheet in `Page.Overlay` covers the page, so nothing has to be added to the page; the sheet keeps the page's bottom safe area free by itself. Outside a page it opens over the window. While nothing is selected the row shows `PlaceholderText` ("Not set") in a muted color. The sheet's title is `SheetTitle`, or the row's title when that is text; `SheetMessage` adds a line under it and `CancelText` labels the Cancel row (null hides it). `IsOpen` opens and closes the sheet from code. A disabled picker, or one with no choices, does not open.

A picker is a `ListItem`, so `Icon`, `Leading`, `Subtitle` and `ShowChevron` (on by default) work as on any row; `Trailing` is not shown, because the value is.

### Text and templates

A choice shows its `ToString()`. `DisplayMemberBinding` reads the text from a member of the choice, and `ItemTemplate` builds both the value at the row's end and each row of the sheet, for example a symbol and a name. In an app compiled for NativeAOT, give the template an `x:DataType` so its bindings are compiled:

<Demo name="Templates" />

```xml
<arcm:Picker Content="Currency" ItemsSource="{Binding Currencies}" SheetTitle="Show prices in">
    <arcm:Picker.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <TextBlock Text="{Binding Symbol}" FontWeight="SemiBold" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>
    </arcm:Picker.ItemTemplate>
</arcm:Picker>
```

### Events

`SelectionChanged` is raised whenever the selected item changes: a row chosen in the sheet, `SelectedItem` or `SelectedIndex` set in code or through a binding, or the selection cleared. Its arguments carry `OldItem`, `NewItem`, `OldIndex` and `NewIndex`. `ItemInvoked` is raised when the person chooses a row, even the one that is already selected, so it suits "close this screen after a choice".

```csharp
picker.SelectionChanged += (_, e) => Save((Currency?)e.NewItem);
```

An index set before the choices arrive waits for them. When the choices change, the selected item keeps its place and its index follows it; an item that is no longer among the choices is still shown, with index -1.

### Tokens

`PickerToken` holds `ValueMaxWidth` (160), the widest the value gets before it ends with an ellipsis, so the title keeps room. The rest of the row is a `ListItem` (`ListItemToken`), and the sheet is an `ActionSheet` (`ActionSheetToken`, `ActionSheetItemToken`). Colors come from the ArcDesign tokens: the value uses the secondary text color and the placeholder the muted one.

## API reference

<!-- api: MobilePicker -->

## Accessibility

The row is a button named by its title; its help text is the current value (or the placeholder), so a screen reader hears both. Tab reaches it, and Enter or Space opens the sheet; the checked row then takes the keyboard focus, Tab moves between the rows and Escape closes the sheet. When the sheet closes the focus goes back to the picker. A keyboard-focused picker is shaded like a pressed row. Every choice is a 56 px row, and the current one is marked with a check mark, not only with color.

## Motion

The sheet is an `ActionSheet` and moves by its physics: it rises on a smooth spring, a drag follows the finger and a release hands the finger's speed to the spring; with reduced motion it jumps. The row shades on press with a short fade, as a `ListItem` does. The check mark appears with the selection, without motion of its own.

## Responsive behavior

- `Picker` has no breakpoint. It is a full-width row of at least 48 px, and its sheet spans the page at every width,
  inset 8 px from each side.
- The value at the row's end is at most `ValueMaxWidth` (160 px) wide and ends with an ellipsis, so a long value never
  pushes the title out; the title takes the rest of the row.
- The sheet grows with its rows up to 90% of the page and then scrolls vertically, never sideways. It keeps the page's
  bottom safe area free, so the Cancel row clears the home indicator.
- Nothing depends on hover: a finger shades the row and a tap opens the sheet.

## Related

- `MobileActionSheet`: the sheet the picker opens, and its `IsChecked` rows.
- `MobileListGroup`: the group the picker sits in.
- `MobileRadioCards`: for a choice that reads better as pictures.
- `MobileSegmented`: for two to four choices that stay visible.
