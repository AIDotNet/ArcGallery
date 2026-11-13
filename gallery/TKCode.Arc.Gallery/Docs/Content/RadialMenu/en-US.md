---
title: Radial menu
summary: "Commands in a ring around the pointer: right-click or hold to open it, then click an item, or hold, flick toward it and let go."
---

<Demo name="Preview" />

## When to use

A radial menu, or pie menu, lays a handful of commands out in a ring around the point where it opens. Every item is
the same short distance from the pointer, and each one sits in its own direction, so people who use the menu often
stop reading it: they hold the button, flick toward the command they want and let go. That makes it a good fit for
tools that people switch between all day, right where they are working.

- **Drawing and design tools**: switch between Select, Pen, Text, Shape and Image on a canvas without travelling to a
  toolbar. The ring opens at the pointer, so the hand stays where the work is.
- **Diagram and flow editors**: add a node, connect, comment or delete on the item under the pointer, with the most
  frequent command in the easiest direction.
- **CAD, 3D and level editors**: snapping modes, views and transform tools that experts reach with a flick, often
  dozens of times a minute.
- **Media and game tools**: quick actions on a timeline clip or a scene object, or a weapon and item wheel in a tool
  that previews games.
- **A compact "add" button**: a sector of four or five choices (photo, camera, file, link, note) that unfolds above a
  floating button at the bottom of a screen.

## When not to use

- **More than about eight commands, or commands in groups**: use `ContextMenu` or `DropdownMenu`. A ring with many
  items gets slices too thin to aim at, and it has no room for separators, group labels or shortcut hints.
- **Actions people must discover**: a menu that opens on a right press or a hold is invisible until someone tries.
  Keep the important commands in a visible toolbar or a `DropdownMenu` with a labelled trigger as well.
- **Choices that feed a form**: use `Select`, `RadioGroup` or `SegmentedControl`. They show the current value and
  take part in validation; a radial menu only runs commands.
- **Long or similar labels**: a sector has room for a word or two. If the commands need sentences to tell them apart,
  use a list.
- **One or two commands**: put them in buttons next to the content.

## Usage

`RadialMenu` derives from Avalonia's `ItemsControl`. Declare its items as `RadialMenuItem` children, each with a
`Header`, an `Icon` and usually a `Command`, and attach the menu to the control it belongs to with
`arc:RadialMenu.Menu`. By default a right press or a press and hold on that control opens the menu at the pointer.

```xml
<Border Height="300" CornerRadius="18" Background="{arc:ArcDesignTokenResource SurfaceMuted}">
    <arc:RadialMenu.Menu>
        <arc:RadialMenu Label="Canvas tools">
            <arc:RadialMenuItem Header="Select" Icon="{x:Static arc:ArcIcons.Move}" Command="{Binding SelectToolCommand}" />
            <arc:RadialMenuItem Header="Pen" Icon="{x:Static arc:ArcIcons.Pencil}" Command="{Binding PenToolCommand}" />
            <arc:RadialMenuItem Header="Text" Icon="{x:Static arc:ArcIcons.Type}" Command="{Binding TextToolCommand}" />
            <arc:RadialMenuItem Header="Delete" Icon="{x:Static arc:ArcIcons.Trash2}" Command="{Binding DeleteCommand}" />
        </arc:RadialMenu>
    </arc:RadialMenu.Menu>
</Border>
```

The attached menu takes the control as its logical parent, so its bindings see the control's `DataContext` even
before it opens. Attach a menu to one control at a time.

### Anatomy

1. **Plate**: the raised surface under the ring, in the raised surface color with a border and the floating shadow.
   In a full circle it is one disc; in a sector it is a rounded band plus a small disc at the centre.
2. **Sectors**: one per visible item, laid out clockwise and separated by `SectorGap` (4 px, the same width all
   along). A resting sector has a faint fill; the highlighted one takes the subtle accent fill and steps out a little.
3. **Icon** (`Icon`): a 24 by 24 Lucide geometry drawn as a 20 px stroke glyph, or any control.
4. **Label** (`Header`): one line of 12 px text under the icon, cut with an ellipsis when the sector is too narrow.
   Turn the labels off with `IsItemLabelVisible="False"` for an icon ring.
5. **Centre** (`InnerRadius`, 44 px): shows the highlighted item's text, or `CenterContent` while nothing is
   highlighted. It is also the gesture's dead zone.

The ring's size comes from `InnerRadius` and `OuterRadius` (124 px): the default ring is 260 px across including the
plate. Make it larger for long labels or many items, smaller for four icons.

### Choosing an item

There are three ways to choose, and they mix freely:

- **Click**: open the menu, then click a sector. Moving over the ring highlights the sector under the pointer; a press
  shows it pressed, and releasing on the same sector chooses it.
- **Marking gesture**: keep holding the button that opened the menu, move toward a sector and let go. The direction
  chooses, even far beyond the ring. Moving back into the centre and letting go cancels. Letting go without moving at
  all is a plain click, and the menu stays open for a click on an item.
- **Keyboard**: arrow keys or Tab move around the ring, a letter jumps to the next item it starts, and Enter or Space
  chooses the highlighted item.

Choosing an item raises `ItemInvoked`, runs the item's `Command` with its `CommandParameter`, runs the menu's
`ItemInvokedCommand` with the item, and closes the menu. Escape, a press outside the ring or a press in its centre
closes it without choosing.

```csharp
tools.ItemInvoked += (_, e) => status.Text = $"Tool: {e.Container.Header}";
```

`SelectedItem` is the highlighted item, and it is two-way: it changes as the pointer or the keyboard moves, raising
`SelectionChanged`, and after a choice it holds the chosen item. Set it before opening to start with an item
highlighted, such as the current tool.

### Circle and sector layouts

`SweepAngle` is the arc the items share: 360, the default, is a full circle, and less is a sector. `StartAngle` says
where the items start, in degrees clockwise from 12 o'clock. In a full circle the first item is centred on that
direction, so four items sit at north, east, south and west; in a sector the first item's leading edge sits on it.
A sector with `StartAngle="-90"` and `SweepAngle="180"` is the upper half of the ring, which suits a button at the
bottom of the screen.

<Demo name="Sector" />

```xml
<arc:Button Variant="Primary" AutomationProperties.Name="Add to note">
    <arc:ArcGlyph Data="{x:Static arc:ArcIcons.Plus}" Size="18" />
    <arc:RadialMenu.Menu>
        <arc:RadialMenu Label="Add to note" Trigger="Click" StartAngle="-90" SweepAngle="180">
            <arc:RadialMenuItem Header="Photo" Icon="{x:Static arc:ArcIcons.Image}" />
            <arc:RadialMenuItem Header="File" Icon="{x:Static arc:ArcIcons.File}" />
            <arc:RadialMenuItem Header="Link" Icon="{x:Static arc:ArcIcons.Link}" />
        </arc:RadialMenu>
    </arc:RadialMenu.Menu>
</arc:Button>
```

The items always divide the sweep equally. An item with `IsVisible="False"` takes no sector, and the others close up;
an item with `IsEnabled="False"` keeps its sector but cannot be highlighted or chosen.

### Marking menus

The gesture is what makes a radial menu fast. On a canvas, hold the right button (or press and hold any button or a
finger), flick toward a command and let go: the menu opens under the press and the release chooses. After a few days
people remember that up copies and left undoes, and the flick happens before they look at the ring. Keep the items in
the same order every time, so the directions stay put.

<Demo name="MarkingMenu" />

`Trigger` says which gestures open an attached menu. It is a set of flags:

| Trigger | Opens |
|---|---|
| `RightPress` | On a right-button press, at the pointer |
| `Hold` | After a 400 ms press and hold that stays within 8 px, at the press point (mouse, touch or pen) |
| `Click` | On a primary click or tap of the control (a button's `Click`, keyboard included), placed as `Placement` says |

The default is `RightPress, Hold`. The ContextMenu key and Shift+F10 on the focused control always open the menu, on
the control's centre with the first item highlighted.

### Commands and MVVM

Bind `ItemsSource` to the view model's items. The menu generates a `RadialMenuItem` for each one, with the item as its
`DataContext`, so a style can map the item's members to `Header`, `Icon`, `Command` and `IsEnabled`. Add
`x:DataType` to the style so its bindings are compiled. The demo below adds and removes tools at run time, and the
Delete tool's command cannot run while the canvas is locked, which disables its sector.

<Demo name="Mvvm" />

```xml
<arc:RadialMenu Label="Tools" ItemsSource="{Binding Tools}" ItemInvokedCommand="{Binding RememberCommand}">
    <arc:RadialMenu.Styles>
        <Style Selector="arc|RadialMenuItem">
            <Setter Property="Header" Value="{Binding Name}" />
            <Setter Property="Icon" Value="{Binding Icon}" />
            <Setter Property="Command" Value="{Binding Command}" />
        </Style>
    </arc:RadialMenu.Styles>
</arc:RadialMenu>
```

`ItemInvokedCommand` runs after every choice with the item as its parameter: the data item for `ItemsSource`, or the
`RadialMenuItem` for declared items. Use it when one command handles every item, and the item's own `Command` when
each item does something different. With `TextSelector` set and no style, the generated items show that text as their
label; it is a delegate, so it works without reflection and under trimming.

```csharp
menu.TextSelector = item => ((Tool)item!).Name;
```

### Custom item content

`ItemTemplate` replaces what a generated sector shows: a color swatch, a thumbnail or a badge. A templated sector shows
its template even with `IsItemLabelVisible="False"`, and `TextSelector` still gives each item the text for the centre
label and for screen readers.

<Demo name="CustomTemplate" />

```xml
<arc:RadialMenu Label="Highlight color" Trigger="Click" IsItemLabelVisible="False">
    <arc:RadialMenu.ItemTemplate>
        <DataTemplate>
            <Ellipse Width="26" Height="26" Fill="{Binding Brush}" />
        </DataTemplate>
    </arc:RadialMenu.ItemTemplate>
</arc:RadialMenu>
```

A declared `RadialMenuItem` can also take any control as its `Icon`, and `HeaderTemplate` formats its `Header`.

### Opening from code and placement

`OpenAt` opens the menu centred on a point in a control's coordinates, for example where a tool wants to offer the
next step. `Open(control)` and `Open()` open it on a control, or on `PlacementTarget` (by default the control it is
attached to), placed as `Placement` says: `Center`, the default, on the control's centre, or `Pointer`, where the
pointer last was over it. `IsOpen` is two-way, so a view model can open and close the menu. `Close` closes it and
gives focus back to where it was.

```csharp
menu.OpenAt(canvas, e.GetPosition(canvas)); // centred on a point
menu.Open(toolButton); // centred on a control
menu.Close();
```

The menu floats on the window's overlay layer, or on the nearest `ArcOverlayHost`. Near an edge of that area it shifts
so the whole ring stays inside, 8 px from the edge. A sector only needs its own extent inside, so a half ring that
opens upward can sit right above the bottom edge. A `RadialMenu` placed directly in a layout does not float: it shows
its ring in place, as a static dial.

### Content guidelines

- **Labels**: one or two words, a verb or a tool name in sentence case: "Pen", "Duplicate", "Add link". The centre
  repeats the label of the highlighted item, so a label that is cut in its sector is still read in full there.
- **Icons**: give every item an icon; the eye finds an icon in a ring faster than a word. Icon-only rings need a
  `Header` or an `AutomationProperties.Name` on every item.
- **Order**: put the most frequent command at the top, its opposite at the bottom (Copy and Paste, Undo and Redo), and
  keep the order stable between versions so the directions people learned keep working.
- **Count**: four to eight items. Four and eight map to compass directions, which are the easiest to flick.

| Instead of | Write |
|---|---|
| Item 1 | Select |
| Create a new comment here | Comment |
| Delete selected object(s) | Delete |
| Misc | Name the command |

### Best practices

**Do**

- Keep the same items in the same places, so people can learn the directions.
- Give every item an icon and a short label, and an accessible name when the label is hidden.
- Offer the same commands somewhere visible as well, such as a toolbar or a menu.
- Use a sector for a button near the edge of a screen, so the ring opens away from the edge.

**Avoid**

- More than eight items, or items that change with the selection. A ring that changes shape cannot be learned.
- Items whose order depends on data, such as recent files.
- Destructive commands in the easiest direction (straight up); put them where a flick will not reach them by accident.
- Interactive controls in an item template; the menu takes the pointer for choosing.

### Common questions

**How does the marking gesture tell a click from a flick?** If the pointer leaves the centre (the `InnerRadius`) before
the button is released, the release chooses the item in that direction, or cancels when the pointer came back to the
centre. If it never left the centre, the release was a click and the menu stays open.

**Why does the ring not open exactly under the pointer near an edge?** It shifts to stay inside the window. The
gesture still measures directions from the point where the press happened, so a flick works the same.

**Can a radial menu have submenus?** Not in this version. Keep the ring to one level of commands and open a second menu
from a command when needed.

**Can I open it on a left click on a canvas?** Set `Trigger="Click"` and `Placement="Pointer"`: a click or tap opens the
menu where it happened.

## API reference

<!-- api: RadialMenu -->

<!-- api: RadialMenuItem -->

## Accessibility

- The menu is announced as a menu named by `Label` (default "Radial menu"); each sector is a menu item named by its
  `AutomationProperties.Name`, the `TextSelector` text or its text `Header`, and it can be invoked by assistive
  technology. Disabled items report that they are not enabled.
- Keyboard navigation moves focus to the highlighted item, so screen readers announce it; pointer highlights do not
  move focus. The highlight is the keyboard position, so sectors draw no focus ring.
- When the menu closes, focus goes back to where it was before it opened; after a keyboard choice the focused control
  shows its focus ring.
- Icons are not read. Every item needs text, even in an icon-only ring.
- The marking gesture is a shortcut, not the only way: every item can also be clicked or reached with the keyboard.

### Keyboard

| Keys | Action |
|---|---|
| `ContextMenu` key / `Shift+F10` | Open the attached menu on the focused control, with the first item highlighted |
| `Right` / `Down` / `Tab` | Move clockwise to the next item (skipping disabled and hidden items, wrapping around) |
| `Left` / `Up` / `Shift+Tab` | Move counter-clockwise to the previous item |
| `Home` / `End` | First / last item |
| A letter | Next item whose text starts with it |
| `Enter` / `Space` | Choose the highlighted item |
| `Escape` | Close without choosing; focus goes back |

## Motion

- The ring grows from its centre on a snappy spring while it fades in, and shrinks a little as it fades out. The exit
  finishes before `Closed` is raised.
- The highlighted sector steps out from the centre on a snappy spring, and its fill and colors cross-fade.
- The centre label changes with a quiet fade.

| Change | Full | Reduced | Off |
|---|---|---|---|
| Open | Scale 0.86 to 1 on a snappy spring, 160 ms fade | 120 ms fade | Immediate |
| Close | Scale to 0.94 with a 120 ms fade | 120 ms fade | Immediate |
| Highlight and press colors | 160 ms fade | 120 ms fade | Immediate |
| Highlighted sector steps out | Snappy spring | None | None |
| Centre label | Label fade | 120 ms cross-fade | Immediate |

## Responsive behavior

- The ring keeps its size, 260 px across by default, and shifts to stay inside the window, so it fits any window
  wider than about 280 px. On a phone-sized view, prefer six items or fewer, or a sector that opens away from the
  nearest edge.
- Touch opens the menu with a press and hold; the finger can then flick and lift to choose, or lift and tap an item.
  There is no hover on touch, so nothing is highlighted until the finger moves or taps.
- Labels that do not fit their sector end with an ellipsis, and the centre shows the full label of the highlighted
  item.

## Related

- `ContextMenu` for a list of actions at the pointer, with groups, checks and shortcut hints.
- `DropdownMenu` for actions behind a visible, labelled trigger.
- `SegmentedControl` when the choice is a value that stays on screen.

| Component | Choose it when |
|---|---|
| `ContextMenu` | More than eight actions, groups or shortcut hints |
| `DropdownMenu` | The actions need a visible trigger |
| `SegmentedControl` | The choice is a value that stays visible |
| `CommandPalette` | People search for commands by name |
