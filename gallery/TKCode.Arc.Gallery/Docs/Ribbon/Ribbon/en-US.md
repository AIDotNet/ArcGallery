---
title: Ribbon
summary: "An Office-style ribbon: tabs over groups of large, medium and small commands that shrink step by step and collapse into drop-downs as the window narrows, with toggle, drop-down and split buttons, rich tooltips, a collapsible ribbon and a quick access toolbar."
---

<Demo name="Preview" />

## When to use

- Desktop apps with many commands: modeling and diagram editors, document and spreadsheet editors, CAD and design tools.
- Commands that fall into a few task areas (Home, Insert, View), each with its own tab of labelled groups.
- Windows that change size a lot: the ribbon keeps every command reachable by shrinking and collapsing groups instead of cutting them off.

## When not to use

- A handful of actions for one view: use a toolbar of `Button`s or a `ButtonGroup`.
- Actions on a selection that appear next to it: use `ContextMenu` or the Selection toolbar block.
- Finding and running any command by name: use the Command palette block (a ribbon and a palette work well together).
- Switching between views of the same content: use `Tabs`.

## Usage

Add the package and register it in `UseArc`:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcRibbon());
```

A `Ribbon` holds `RibbonTab`s; each tab holds `RibbonGroup`s; each group holds commands. A command has a `Label`, an `Icon` (a 24x24 Lucide geometry such as `{x:Static arc:ArcIcons.Copy}`, or any control) and runs its `Command` and `Click` like any button:

```xml
<arc:Ribbon>
    <arc:RibbonTab Header="Home">
        <arc:RibbonGroup Header="Clipboard" Icon="{x:Static arc:ArcIcons.ClipboardPaste}">
            <arc:RibbonButton Label="Paste" Icon="{x:Static arc:ArcIcons.ClipboardPaste}" Command="{Binding PasteCommand}" />
            <arc:RibbonButton Label="Copy" Icon="{x:Static arc:ArcIcons.Copy}" Size="Medium" Command="{Binding CopyCommand}" />
        </arc:RibbonGroup>
    </arc:RibbonTab>
</arc:Ribbon>
```

`ItemInvoked` reports every command the user runs, from the ribbon, from a collapsed group or from the quick access toolbar; `Item` is the button and `Entry` the menu row when the command came from a menu. `SelectionChanged` reports the selected tab (`SelectedIndex`, `SelectedTab`). `LeftContent` and `RightContent` put app-level items at the ends of the tab row, such as a File menu or a search box.

### Sizes

`Size` lays a command out as `Large` (a large icon over the label, the full height of the group; the default), `Medium` (a small icon beside the label) or `Small` (the icon alone; the label moves to the tooltip). Medium and small commands stack three to a column. A `RibbonSeparator` draws a line and starts a new column. Any other control, such as an Arc `Select` or `NumberField`, stacks like a medium command.

<Demo name="Sizes" />

### Command kinds

`RibbonToggleButton` stays on or off (`IsChecked`, `IsCheckedChanged`) and is filled with the accent while on. `RibbonDropDownButton` opens a menu of `MenuEntry` rows (with separators and submenus) instead of running a command; `ItemsSource` and `ItemTemplate` build the rows from data. `RibbonSplitButton` does both: its icon runs the default action and its chevron part opens the alternatives. A chevron marks every command with a menu. `Description` and `Shortcut` add a sentence and a key to the tooltip, under the label.

<Demo name="CommandKinds" />

### Adaptive groups

When the ribbon is narrower than its groups, they shrink one step at a time: large commands become medium, then everything becomes small, and finally a group collapses into one button that shows the whole group in a drop-down. Each command stops at its `MinSize`, and a step that would not make a group narrower is skipped. Groups with a higher `ReductionPriority` shrink all the way first; groups of the same priority take turns from the right. `CanCollapse="False"` keeps a group out of the last step. The layout depends only on the width, so widening the window brings back exactly the same layout. `IsCollapsed` and `IsDropDownOpen` say what a group shows now.

<Demo name="Adaptive" />

`IsLauncherVisible` adds the small button at the end of a group's header that opens the group's full dialog; handle `LauncherClick` or set `LauncherCommand`.

### Quick access toolbar

A `QuickAccessToolBar` keeps a few commands visible whichever tab is selected. Place it where the app wants it (above or below the ribbon, or in an `ArcWindow` title bar through `TitleBar.LeftContent`) and link it with `Ribbon`. Its `Items` are its own commands, shown small. Commands of the ribbon with a `QuickAccessKey` can be pinned: right-click the command, or use the chevron at the end of the toolbar. `Keys` lists the pinned keys in order; save it to keep the user's choice and fill it again at startup. A pinned command shows as a small copy that runs the ribbon command (opens its menu, flips its toggle) and follows its icon and enabled state.

<Demo name="QuickAccess" />

```csharp
// Restore the user's choice, and save it when it changes.
quickAccess.Keys.AddRange(settings.QuickAccessKeys);
quickAccess.Keys.CollectionChanged += (_, _) => settings.QuickAccessKeys = quickAccess.Keys.ToArray();
```

### Collapsed ribbon

`IsMinimized` collapses the ribbon to its tab row. A click on a tab then opens its groups over the content, and running a command, a click outside or Escape closes them. Users collapse and pin the ribbon with the button at the end of the tab row, with Ctrl+F1 or by double-clicking a tab.

<Demo name="Minimized" />

### Contextual tabs

`IsContextual` marks a tool tab that belongs to the current selection (Table design, Node tools); its header is drawn in the accent. Show and hide it with `IsVisible`: when the selected tab is hidden, the ribbon selects the first visible one.

<Demo name="Contextual" />

## API reference

<!-- api: Ribbon -->

<!-- api: RibbonTab -->

<!-- api: RibbonGroup -->

<!-- api: RibbonButton -->

<!-- api: RibbonToggleButton -->

<!-- api: RibbonDropDownButton -->

<!-- api: RibbonSplitButton -->

<!-- api: RibbonSeparator -->

<!-- api: QuickAccessToolBar -->

## Accessibility

- The tab row is one tab stop: the selected tab is focusable with Tab, and the other tabs are reached with the arrow keys. Screen readers hear each header as a tab.
- Every command's `Label` is its accessible name, also when only the icon shows. A toggle button reports its on or off state; a group's launcher is named "<Header> options".
- A collapsed group's button carries the group's name and opens the group in a drop-down; Escape closes it and returns focus to the button.
- The tooltip repeats the label, the shortcut and the description, so small commands are never icon-only for sighted users either.

### Keyboard

| Key | Where | Action |
|---|---|---|
| ArrowLeft / ArrowRight | Tab row | Select the previous or next tab (hidden and disabled tabs are skipped). |
| Home / End | Tab row | Select the first or last tab. |
| ArrowDown | Tab row | Move into the selected tab's commands (on a collapsed ribbon, open them first). |
| Enter / Space | Tab row | On a collapsed ribbon, open the selected tab's commands. |
| ArrowLeft / ArrowRight | Commands | Move to the previous or next command. |
| Escape | Commands | Return to the selected tab; in a drop-down, close it. |
| Enter / Space | Command | Run it; on a drop-down button, open its menu with the first row highlighted. |
| ArrowDown / F4 | Drop-down, split or collapsed group button | Open the menu or the drop-down. |
| Ctrl+F1 | Anywhere in the ribbon | Collapse the ribbon to its tab row, or pin it open again. |
| Shift+F10 / Menu | Command with a `QuickAccessKey` | Offer to add it to, or remove it from, the quick access toolbar. |

## Motion

- The accent underline glides from the old tab to the new one on a spring.
- The new tab's groups fade in and slide 8px from the side of travel (160 ms). With reduced motion they only fade; with motion off they appear at once.
- Commands change their fill and color in one short step; menus, drop-downs and the collapsed ribbon's pop-up open and close with Arc's pop-up motion.

## Responsive behavior

- Groups shrink step by step to fit the width (see Adaptive groups); every command stays reachable, through a collapsed group's drop-down if need be.
- Tabs that do not fit in the tab row are cut off at its end; keep tab names short, or collapse the ribbon on small windows with `IsMinimized`.
- The ribbon is made for pointer and keyboard on desktop. On phones, use a bottom toolbar or a menu instead.

## Related

- `Tabs`: switch between views of the same content.
- `ButtonGroup` and `SplitButton`: a few related actions without a ribbon.
- `DropdownMenu` and `ContextMenu`: the menus the ribbon's buttons open.
- `DockLayout` (`TKCode.Arc.Dock`): the panes and documents under the ribbon of an IDE-style app.
