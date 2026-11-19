---
title: 功能区
summary: "Office 式功能区：选项卡下是由大、中、小三种命令组成的分组，窗口变窄时逐级缩小并折叠为下拉；支持开关、下拉和拆分按钮、带说明的工具提示、可收起的功能区和快速访问工具栏。"
source-hash: 3bb67182c265
---

<Demo name="Preview" />

## 何时使用

- 命令很多的桌面应用：建模与流程图编辑器、文档与表格编辑器、CAD 和设计工具。
- 命令可以按几个任务领域划分（开始、插入、视图），每个领域一个选项卡，选项卡里是带名称的分组。
- 窗口尺寸变化较大：功能区通过缩小和折叠分组让所有命令都保持可用，而不是把它们截掉。

## 何时不用

- 只为一个视图提供少量操作：用一排 `Button` 或 `ButtonGroup`。
- 出现在选中内容旁边、针对选中内容的操作：用 `ContextMenu` 或 Selection toolbar 区块。
- 按名称查找并执行任意命令：用 Command palette 区块（功能区和命令面板可以搭配使用）。
- 在同一内容的不同视图之间切换：用 `Tabs`。

## 用法

添加包并在 `UseArc` 中注册：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcRibbon());
```

`Ribbon` 包含若干 `RibbonTab`，每个选项卡包含若干 `RibbonGroup`，每个分组包含命令。命令有 `Label`、`Icon`（24x24 的 Lucide 几何图形，例如 `{x:Static arc:ArcIcons.Copy}`，也可以是任意控件），并像普通按钮一样执行 `Command` 和 `Click`：

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

`ItemInvoked` 报告用户执行的每个命令，无论来自功能区、折叠分组还是快速访问工具栏；`Item` 是按钮，命令来自菜单时 `Entry` 是菜单行。`SelectionChanged` 报告选中的选项卡（`SelectedIndex`、`SelectedTab`）。`LeftContent` 和 `RightContent` 在选项卡行两端放置应用级的内容，例如“文件”菜单或搜索框。

### 尺寸

`Size` 决定命令的布局：`Large`（大图标在上、名称在下，占满分组高度；默认值）、`Medium`（小图标在名称左侧）或 `Small`（只有图标，名称移到工具提示里）。中、小命令每列最多叠放三个。`RibbonSeparator` 画一条分隔线并开始新的一列。其他控件（例如 Arc 的 `Select` 或 `NumberField`）按中号命令叠放。

<Demo name="Sizes" />

### 命令类型

`RibbonToggleButton` 保持开或关（`IsChecked`、`IsCheckedChanged`），打开时以强调色填充。`RibbonDropDownButton` 不执行命令，而是打开一个由 `MenuEntry` 行组成的菜单（支持分隔线和子菜单）；`ItemsSource` 和 `ItemTemplate` 可以根据数据生成菜单行。`RibbonSplitButton` 两者兼有：图标部分执行默认操作，箭头部分打开其他选项。带菜单的命令都有一个向下箭头。`Description` 和 `Shortcut` 在工具提示中名称的下方加上一句说明和快捷键。

<Demo name="CommandKinds" />

### 自适应分组

功能区比分组窄时，分组逐级缩小：大命令先变成中号，然后全部变成小号，最后整个分组折叠成一个按钮，点击后在下拉中显示完整的分组。每个命令最小只缩到它的 `MinSize`，不会让分组变窄的那一级会被跳过。`ReductionPriority` 较高的分组先缩到底；优先级相同的分组从右往左轮流缩小。`CanCollapse="False"` 让分组不进入最后一级。布局只取决于宽度，所以把窗口拉宽后会恢复成完全相同的布局。`IsCollapsed` 和 `IsDropDownOpen` 说明分组当前的显示状态。

<Demo name="Adaptive" />

`IsLauncherVisible` 在分组标题末尾加一个小按钮，用来打开该分组的完整对话框；处理 `LauncherClick` 或设置 `LauncherCommand`。

### 快速访问工具栏

`QuickAccessToolBar` 让几个命令始终可见，不论选中哪个选项卡。把它放在应用需要的位置（功能区上方或下方，或通过 `TitleBar.LeftContent` 放进 `ArcWindow` 的标题栏），并用 `Ribbon` 关联功能区。它的 `Items` 是工具栏自己的命令，以小号显示。设置了 `QuickAccessKey` 的功能区命令可以固定到工具栏：右键单击该命令，或使用工具栏末尾的向下箭头。`Keys` 按顺序列出已固定命令的键；保存它即可记住用户的选择，启动时再填回去。固定的命令显示为一个小号副本，它执行功能区中的原命令（打开其菜单、切换其开关），并跟随原命令的图标和启用状态。

<Demo name="QuickAccess" />

```csharp
// Restore the user's choice, and save it when it changes.
quickAccess.Keys.AddRange(settings.QuickAccessKeys);
quickAccess.Keys.CollectionChanged += (_, _) => settings.QuickAccessKeys = quickAccess.Keys.ToArray();
```

### 收起的功能区

`IsMinimized` 把功能区收起为只有选项卡行。此时单击选项卡会在内容上方打开它的分组；执行命令、在外部单击或按 Escape 会关闭它们。用户可以用选项卡行末尾的按钮、Ctrl+F1 或双击选项卡来收起或固定功能区。

<Demo name="Minimized" />

### 上下文选项卡

`IsContextual` 把选项卡标记为属于当前选中内容的工具选项卡（表格设计、节点工具），它的标题以强调色显示。用 `IsVisible` 显示或隐藏它：选中的选项卡被隐藏时，功能区会选中第一个可见的选项卡。

<Demo name="Contextual" />

## API 参考

<!-- api: Ribbon -->

<!-- api: RibbonTab -->

<!-- api: RibbonGroup -->

<!-- api: RibbonButton -->

<!-- api: RibbonToggleButton -->

<!-- api: RibbonDropDownButton -->

<!-- api: RibbonSplitButton -->

<!-- api: RibbonSeparator -->

<!-- api: QuickAccessToolBar -->

## 无障碍

- 选项卡行只占一个 Tab 停靠点：选中的选项卡可以用 Tab 键聚焦，其他选项卡用方向键到达。读屏软件把每个标题读作选项卡。
- 每个命令的 `Label` 就是它的无障碍名称，只显示图标时也一样。开关按钮会报告开或关的状态；分组的启动器按钮名为“<标题>选项”。
- 折叠分组的按钮带有分组名称，并在下拉中打开该分组；Escape 关闭下拉并把焦点还给按钮。
- 工具提示重复名称、快捷键和说明，所以小号命令对视力正常的用户也不只是一个图标。

### 键盘

| 按键 | 位置 | 操作 |
|---|---|---|
| ArrowLeft / ArrowRight | 选项卡行 | 选中上一个或下一个选项卡（跳过隐藏和禁用的选项卡）。 |
| Home / End | 选项卡行 | 选中第一个或最后一个选项卡。 |
| ArrowDown | 选项卡行 | 进入选中选项卡的命令（功能区收起时先把它们打开）。 |
| Enter / Space | 选项卡行 | 功能区收起时，打开选中选项卡的命令。 |
| ArrowLeft / ArrowRight | 命令 | 移到上一个或下一个命令。 |
| Escape | 命令 | 回到选中的选项卡；在下拉中则关闭下拉。 |
| Enter / Space | 命令 | 执行命令；在下拉按钮上打开菜单并高亮第一行。 |
| ArrowDown / F4 | 下拉、拆分或折叠分组按钮 | 打开菜单或下拉。 |
| Ctrl+F1 | 功能区内任意位置 | 把功能区收起为选项卡行，或重新固定展开。 |
| Shift+F10 / Menu | 设置了 `QuickAccessKey` 的命令 | 提供添加到快速访问工具栏或从中删除的选项。 |

## 动效

- 强调色下划线以弹簧从旧选项卡滑到新选项卡。
- 新选项卡的分组从移动方向一侧滑入 8px 并淡入（160 ms）。减少动效时只淡入；关闭动效时直接出现。
- 命令的填充色和文字颜色一步完成变化；菜单、下拉和收起功能区的弹层使用 Arc 的弹层动效打开和关闭。

## 响应式行为

- 分组逐级缩小以适应宽度（见“自适应分组”）；所有命令都保持可用，必要时通过折叠分组的下拉访问。
- 选项卡行放不下的选项卡会在行尾被截掉；请保持选项卡名称简短，或在小窗口中用 `IsMinimized` 收起功能区。
- 功能区面向桌面上的鼠标和键盘。在手机上请改用底部工具栏或菜单。

## 相关

- `Tabs`：在同一内容的不同视图之间切换。
- `ButtonGroup` 和 `SplitButton`：不需要功能区时的几个相关操作。
- `DropdownMenu` 和 `ContextMenu`：功能区按钮打开的菜单。
- `DockLayout`（`TKCode.Arc.Dock`）：IDE 式应用中功能区下方的面板和文档。
