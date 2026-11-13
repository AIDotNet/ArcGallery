---
title: Mobile list
summary: Grouped rows for settings and menus: icon, title, subtitle, a value or control at the end and a chevron.
---

<Demo name="Basic" />

## When to use

`ListGroup` and `ListItem` lay out a screen of rows: settings, menus and account screens, where each row is a title the person can tap.

- **Settings**: group related rows under a short `Header`, with a `Footer` that explains them.
- **A menu of destinations**: rows with an `Icon` and a chevron that open other screens.
- **A value or a switch at the end of a row**: put it in `Trailing`.
- **A person or an account at the start of a row**: put the avatar in `Leading`.
- **Content that is not an action**: a QR code, a paragraph or a button of its own goes in a `ListCell`, a row that does not respond to a press.

The live demo runs inside a phone frame, a desktop simulation; touch feedback on a real device has not been verified here.

## When not to use

- **Long or endless data**: a virtualized list is the right tool, and this one is not yet built.
- **Cards with images and several actions**: build them as their own layout.
- **A desktop menu**: use the desktop menu controls; rows here have no hover state.

## Usage

Put `ListItem`s in a `ListGroup` and the group in a scrolling `Page`. The item's content is its title. `ListItem` is a button, so give it a `Command` or handle `Click`; a finger on it shades the row.

```xml
<arcm:Page xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
           xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
           IsScrollable="True">
    <arcm:ListGroup Header="Account">
        <arcm:ListItem Icon="{x:Static arc:ArcIcons.User}" Content="Profile" Subtitle="Ada Lovelace" ShowChevron="True" />
        <arcm:ListItem Content="Weekly summary" Trailing="Sundays" ShowChevron="True" />
    </arcm:ListGroup>
</arcm:Page>
```

The last row of a group has no separator under it.

### Content cells

`ListCell` is a row that holds any content and does not respond to a press: no shade, no hover, not focusable itself (its content can be). It has the frame of a `ListItem`, the same 48 px minimum height, side padding and separator, so cells and items mix in one group and the last row of the group, cell or item, has no separator. Use it for a QR code, a paragraph of text, a button of its own, or a `MobileRadioCards` grid among the rows.

<Demo name="Cells" />

```xml
<arcm:ListGroup Header="Invite code">
    <arcm:ListCell HorizontalContentAlignment="Center">
        <Image Source="{Binding QrCode}" Width="{arc:ArcDesignTokenResource MediaSizeMD}" />
    </arcm:ListCell>
    <arcm:ListCell>
        <arcm:Button Content="Copy code" HorizontalAlignment="Left" />
    </arcm:ListCell>
    <arcm:ListItem Content="Invite by email" ShowChevron="True" />
</arcm:ListGroup>
```

A `ListCell` written in the group, or returned as the root of the `ItemTemplate`, is the row as it is; it is not wrapped in a `ListItem`.

### Leading content

`Leading` takes any content before the title, such as an avatar. It shares the first column with `Icon` (and replaces the icon when both are set), so the title and the `Subtitle` stay together beside it. The leading content keeps its own size, and the row grows to fit it.

<Demo name="Leading" />

```xml
<arcm:ListItem Content="Ada Lovelace" Subtitle="Account, devices and sign-in" ShowChevron="True">
    <arcm:ListItem.Leading>
        <Border Width="{arcm:ListItemTokenResource AvatarSizeXL}"
                Height="{arcm:ListItemTokenResource AvatarSizeXL}"
                CornerRadius="{arc:ArcDesignTokenResource RadiusPill}"
                Background="{arc:ArcDesignTokenResource AccentSubtle}" />
    </arcm:ListItem.Leading>
</arcm:ListItem>
```

The avatar sizes are tokens of `ListItemToken`, for the leading slot and for profile rows and headers elsewhere on a screen:

| Token | Size | Use |
|---|---|---|
| `AvatarSizeSM` | 32 | a compact row, a comment |
| `AvatarSizeMD` | 40 | a contact or account row |
| `AvatarSizeLG` | 48 | a row that leads with a person |
| `AvatarSizeXL` | 64 | the profile row at the top of a settings screen |
| `AvatarSize2XL` | 88 | a profile header |

### Tokens

`ListItemToken` holds a row's geometry: `RowHeight` (48), `ContentPadding` (16 by 10), `IconExtent` (22), `IconGap` (12), `TrailingMargin`, `ChevronExtent` (16), `SeparatorInset` (16), `LeadingMargin` (12 after the icon or the leading content) and the avatar sizes above. `ListCellToken` holds a cell's: `RowHeight` (48), `ContentPadding` (16 by 12) and `SeparatorInset` (16). `ListGroupToken` holds the group's: `GroupPadding` (16 by 8), `CardBorderThickness` (1), `HeaderMargin` and `FooterMargin`. Colors, radii and type come from the ArcDesign tokens.

## API reference

<!-- api: MobileListGroup -->

<!-- api: MobileListItem -->

<!-- api: MobileListCell -->

## Accessibility

A row is a button named by its title and subtitle. Give every row a title, so an icon or an avatar is never its only name. Each row is at least 48 px tall, above the 44 px touch target. A `ListCell` is not a button and takes no focus; a control inside it (a button, radio cards) is reached and named on its own.

## Motion

A finger on a row shades its background on the fast curve; with reduced motion the shade appears at once. A `ListCell` has no motion of its own: it does not respond to a press.

## Responsive behavior

- `ListGroup` has no breakpoint. It is a full-width card with `GroupPadding` (16 by 8 px) around it, and its `Header`
  and `Footer` wrap (`TextWrapping="Wrap"`) instead of being cut off.
- Each `ListItem` row is at least 48 px high, and its subtitle is trimmed to one line with an ellipsis
  (`CharacterEllipsis`).
- The row's trailing content takes the space left after the title, so a long trailing value pushes the title shorter.
  Keep it short.
- Leading content keeps its own size and the row grows to fit it; a 40 px avatar makes a 60 px row. A `ListCell` grows
  with its content, so wrap long text in it rather than letting it run.
- A finger shades a row with a muted surface, and no row depends on hover. Tapping the row is the way to act on it.

## Related

- `MobilePage`: a scrolling page holds the groups.
- `MobileSwitch`: the usual `Trailing` control for a setting.
- `MobileNavbar`: the top bar, with a large title for a root settings screen.
- `MobilePicker`: a row that shows a value and opens its choices in an action sheet.
- `MobileRadioCards`: a card grid that fits in a `ListCell`.
