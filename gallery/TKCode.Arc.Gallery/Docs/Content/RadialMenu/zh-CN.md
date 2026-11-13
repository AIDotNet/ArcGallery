---
title: 径向菜单
summary: "围绕指针的一圈命令：右键单击或按住即可打开，然后单击某一项，或按住、朝它轻甩再松开。"
source-hash: e2fab1aea5d9
---

<Demo name="Preview" />

## 何时使用

径向菜单，也叫饼图菜单，会把若干命令排成一圈，围绕它打开的位置。每一项到指针的距离都相同，且各占一个方向，因此经常使用此菜单的人往往不再去读它：他们按住按钮，朝想要的命令轻甩，然后松开。这使它很适合人们整天来回切换、并且就在工作处使用的工具。

- **绘图与设计工具**：在画布上切换选择、钢笔、文本、形状和图像，无需移动到工具栏。圆环在指针处打开，手可以留在工作的地方。
- **图表与流程编辑器**：对指针下的项执行添加节点、连接、评论或删除，最常用的命令位于最容易轻甩到的方向。
- **CAD、三维与关卡编辑器**：捕捉模式、视图和变换工具，专业用户靠轻甩即可调用，每分钟常常要用几十次。
- **媒体与游戏工具**：对时间线片段或场景对象执行快捷操作，或在预览游戏的工具中使用武器与道具转盘。
- **紧凑的"添加"按钮**：由四到五个选项组成的扇形（照片、相机、文件、链接、笔记），在屏幕底部的浮动按钮上方展开。

## 何时不用

- **超过约八个命令，或命令需要分组**：使用 `ContextMenu` 或 `DropdownMenu`。项目太多时，每个扇区会过窄而难以瞄准，而且圆环上没有空间放置分隔线、分组标题或快捷键提示。
- **必须让人发现的操作**：在右键按下或按住时才打开的菜单，在有人尝试之前是看不见的。重要命令也应保留在可见的工具栏中，或保留带有明确标签触发器的 `DropdownMenu`。
- **会提交到表单的选择**：使用 `Select`、`RadioGroup` 或 `SegmentedControl`。它们会显示当前值，并参与验证；径向菜单只负责执行命令。
- **标签过长或彼此相似**：一个扇区只容纳一两个词。如果命令需要整句话才能区分，请改用列表。
- **只有一两个命令**：把它们放在内容旁边的按钮中。

## 用法

`RadialMenu` 派生自 Avalonia 的 `ItemsControl`。把菜单项声明为 `RadialMenuItem` 子元素，每一项都设置 `Header`、`Icon`，通常还设置 `Command`，然后用 `arc:RadialMenu.Menu` 把菜单附加到它所属的控件上。默认情况下，在该控件上按下右键或按住，都会在指针处打开菜单。

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

附加的菜单以该控件作为逻辑父级，因此即使菜单尚未打开，其绑定也能看到控件的 `DataContext`。每次只把一个菜单附加到一个控件上。

### 组成部分

1. **底盘**：圆环下方凸起的表面，使用凸起表面颜色，带有边框和浮动阴影。完整圆形时它是一个圆盘；扇形时它是一条圆角带，加上中心处的一个小圆盘。
2. **扇区**：每个可见项对应一个，按顺时针排列，由 `SectorGap`（4 px，各处宽度相同）分隔。静止的扇区有淡淡的填充；高亮的扇区采用柔和的强调色填充，并略微向外凸出。
3. **图标**（`Icon`）：24 乘 24 的 Lucide 几何图形，以 20 px 描边的字形绘制，也可以是任意控件。
4. **标签**（`Header`）：图标下方一行 12 px 文本，扇区过窄时以省略号截断。对于图标圆环，可用 `IsItemLabelVisible="False"` 关闭标签。
5. **中心**（`InnerRadius`，44 px）：显示高亮项的文本；未高亮任何项时显示 `CenterContent`。它同时也是手势的死区。

圆环的尺寸由 `InnerRadius` 和 `OuterRadius`（124 px）决定：默认圆环连同底盘的直径为 260 px。标签较长或项目较多时请加大，只有四个图标时可以缩小。

### 选择一项

有三种方式可以选择，并且可以自由组合：

- **单击**：打开菜单，然后单击一个扇区。指针经过圆环时会高亮指针下的扇区；按下时显示为按下状态，在同一扇区上松开即选中它。
- **标记手势**：保持按住打开菜单的那个按钮，朝一个扇区移动后松开。方向决定选择结果，即使远远超出圆环也有效。移回中心后松开则取消。完全没有移动就松开是普通单击，菜单保持打开，等待对某一项的单击。
- **键盘**：方向键或 Tab 在圆环上移动，字母键跳到下一个以该字母开头的项，Enter 或 Space 选中高亮的项。

选中一项会引发 `ItemInvoked`，以 `CommandParameter` 执行该项的 `Command`，以该项为参数执行菜单的 `ItemInvokedCommand`，然后关闭菜单。按下 Escape、在圆环外按下，或在中心按下，都会在不选择的情况下关闭菜单。

```csharp
tools.ItemInvoked += (_, e) => status.Text = $"Tool: {e.Container.Header}";
```

`SelectedItem` 是高亮项，且为双向绑定：它会随指针或键盘的移动而变化，并引发 `SelectionChanged`；选择之后，它保存被选中的项。打开前设置它，可以让某一项以高亮状态开始，例如当前工具。

### 圆形与扇形布局

`SweepAngle` 是各项共享的弧度：默认值 360 表示完整圆形，更小的值表示扇形。`StartAngle` 指定各项的起始位置，以从 12 点钟方向顺时针计算的度数表示。完整圆形中，第一项居中于该方向，因此四个项分别位于北、东、南、西；扇形中，第一项的前缘位于该方向。`StartAngle="-90"` 且 `SweepAngle="180"` 的扇形是圆环的上半部分，适合放在屏幕底部的按钮上。

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

各项总是平分扇形的弧度。`IsVisible="False"` 的项不占用扇区，其余项会收拢；`IsEnabled="False"` 的项保留其扇区，但不能被高亮或选中。

### 标记菜单

这种手势是径向菜单快速的原因所在。在画布上，按住右键（或按住任意按钮或手指），朝某个命令轻甩后松开：菜单在按下处打开，松开时完成选择。用了几天之后，人们就会记住向上是复制、向左是撤销，并且在看圆环之前就已完成轻甩。每次都保持项目顺序相同，方向才能保持不变。

<Demo name="MarkingMenu" />

`Trigger` 指定哪些手势会打开附加的菜单。它是一组标志：

| Trigger | 打开时机 |
|---|---|
| `RightPress` | 按下鼠标右键时，在指针处 |
| `Hold` | 在鼠标、触摸或触控笔上按住 400 ms、且位移不超过 8 px 之后，在按下点 |
| `Click` | 在控件上主按键单击或点按时（包括按钮的 `Click`，也包括键盘触发），位置按 `Placement` 的设置 |

默认值为 `RightPress, Hold`。焦点控件上的 ContextMenu 键和 Shift+F10 始终会打开菜单，位于控件中心，并高亮第一项。

### 命令与 MVVM

把 `ItemsSource` 绑定到视图模型的项集合。菜单会为每一项生成一个 `RadialMenuItem`，并以该项作为其 `DataContext`，因此样式可以把项的成员映射到 `Header`、`Icon`、`Command` 和 `IsEnabled`。为样式添加 `x:DataType`，以便编译其绑定。下面的示例会在运行时添加和移除工具；画布锁定时，删除工具的命令无法执行，因此它的扇区会被禁用。

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

`ItemInvokedCommand` 会在每次选择后运行，并以该项作为参数：对于 `ItemsSource`，参数是数据项；对于声明的项，参数是 `RadialMenuItem`。当一个命令处理所有项时使用它；当每一项各自执行不同的操作时，则使用项自身的 `Command`。设置了 `TextSelector` 且没有样式时，生成的项会以该文本作为标签；它是一个委托，因此无需反射，也适用于裁剪。

```csharp
menu.TextSelector = item => ((Tool)item!).Name;
```

### 自定义项内容

`ItemTemplate` 会替换生成的扇区所显示的内容，例如颜色色块、缩略图或徽章。即使设置了 `IsItemLabelVisible="False"`，使用模板的扇区仍会显示其模板，而 `TextSelector` 仍会为每一项提供中心标签和屏幕阅读器所需的文本。

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

声明的 `RadialMenuItem` 也可以把任意控件作为其 `Icon`，而 `HeaderTemplate` 用于设置其 `Header` 的格式。

### 从代码打开与定位

`OpenAt` 以控件坐标系中的某一点为中心打开菜单，例如工具希望提供下一步操作的位置。`Open(control)` 和 `Open()` 会在某个控件上打开菜单，或在 `PlacementTarget`（默认为附加的控件）上打开，位置由 `Placement` 决定：默认的 `Center` 表示控件中心，`Pointer` 表示指针最后所在的位置。`IsOpen` 为双向绑定，因此视图模型可以打开和关闭菜单。`Close` 会关闭菜单，并把焦点交还给原来的位置。

```csharp
menu.OpenAt(canvas, e.GetPosition(canvas)); // centred on a point
menu.Open(toolButton); // centred on a control
menu.Close();
```

菜单浮动在窗口的覆盖层上，或最近的 `ArcOverlayHost` 上。靠近该区域的边缘时，它会移动，使整个圆环保持在区域之内，并与边缘保持 8 px 的距离。扇形只需要自身范围位于区域之内，因此向上打开的半圆环可以紧贴在底边上方。直接放在布局中的 `RadialMenu` 不会浮动：它会就地显示圆环，作为静态的表盘。

### 内容指南

- **标签**：一到两个词，使用动词或工具名称，句首大写，例如："钢笔"、"复制"、"添加链接"。中心会重复高亮项的标签，因此在扇区中被截断的标签，在中心仍能完整读到。
- **图标**：为每一项都提供图标；在圆环中，人眼找图标比找文字更快。纯图标的圆环需要为每一项设置 `Header` 或 `AutomationProperties.Name`。
- **顺序**：把最常用的命令放在顶部，把与其相反的命令放在底部（复制与粘贴、撤销与重做），并在各版本之间保持顺序稳定，使人们已经学会的方向继续有效。
- **数量**：四到八项。四项和八项对应罗盘方向，最容易轻甩。

| 避免使用 | 改为 |
|---|---|
| 项目 1 | 选择 |
| 在此处创建新评论 | 评论 |
| 删除所选对象 | 删除 |
| 其他 | 为命令命名 |

### 最佳实践

**建议**

- 让相同的项始终位于相同的位置，以便人们学会各个方向。
- 为每一项都提供图标和简短标签；标签被隐藏时，还要提供无障碍名称。
- 在工具栏或菜单等可见位置，同时提供相同的命令。
- 把扇形用于靠近屏幕边缘的按钮，使圆环朝远离边缘的方向打开。

**避免**

- 超过八项，或项目随选择而变化。会变形的圆环无法被学会。
- 顺序取决于数据的项，例如最近文件。
- 把破坏性命令放在最容易轻甩的方向（正上方）；应放在轻甩不会意外碰到的位置。
- 在项模板中使用交互式控件；菜单会接管指针以进行选择。

### 常见问题

**标记手势如何区分单击与轻甩？** 如果在释放按钮之前，指针离开了中心（`InnerRadius`），释放时便会选中该方向的项；如果指针已回到中心，则取消。如果指针从未离开中心，释放即为单击，菜单保持打开。

**为什么靠近边缘时圆环不会恰好在指针下方打开？** 为了保持在窗口之内，它会移动位置。手势仍然从按下的位置开始测量方向，因此轻甩的效果不变。

**径向菜单可以有子菜单吗？** 本版本不支持。圆环只保留一级命令；需要时，可从某个命令再打开第二个菜单。

**能否在画布上用左键单击打开它？** 设置 `Trigger="Click"` 和 `Placement="Pointer"`：单击或点按会在发生处打开菜单。

## API 参考

<!-- api: RadialMenu -->

<!-- api: RadialMenuItem -->

## 无障碍

- 菜单会被播报为一个菜单，名称由 `Label` 指定（默认为 "径向菜单"）；每个扇区都是一个菜单项，其名称取自 `AutomationProperties.Name`、`TextSelector` 文本或其文本 `Header`，并且可以被辅助技术调用。禁用的项会报告其未启用。
- 键盘导航会把焦点移到高亮项上，因此屏幕阅读器会朗读它；指针的高亮不会移动焦点。高亮代表键盘位置，因此扇区不绘制焦点框。
- 菜单关闭时，焦点会回到打开之前所在的位置；通过键盘选择之后，获得焦点的控件会显示其焦点框。
- 图标不会被朗读。每一项都需要文本，即使是纯图标的圆环也一样。
- 标记手势只是一种快捷方式，而不是唯一的方式：每一项也都可以通过单击或键盘触达。

### 键盘

| 按键 | 操作 |
|---|---|
| `ContextMenu` 键 / `Shift+F10` | 在焦点控件上打开附加的菜单，并高亮第一项 |
| `Right` / `Down` / `Tab` | 顺时针移到下一项（跳过禁用和隐藏的项，并循环） |
| `Left` / `Up` / `Shift+Tab` | 逆时针移到上一项 |
| `Home` / `End` | 第一项 / 最后一项 |
| 字母键 | 下一个文本以该字母开头的项 |
| `Enter` / `Space` | 选中高亮的项 |
| `Escape` | 不选择即关闭；焦点回到原处 |

## 动效

- 圆环在淡入时以灵敏弹簧的节奏从中心放大，淡出时略微缩小。退出动画结束之后才会引发 `Closed`。
- 高亮的扇区以灵敏弹簧的节奏从中心向外凸出，其填充与颜色交叉淡化。
- 中心标签以平缓的淡入淡出变化。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 打开 | 以灵敏弹簧从 0.86 缩放到 1，160 ms 淡入 | 120 ms 淡入 | 立即 |
| 关闭 | 缩放到 0.94，并以 120 ms 淡出 | 120 ms 淡出 | 立即 |
| 高亮与按下颜色 | 160 ms 淡变 | 120 ms 淡变 | 立即 |
| 高亮扇区凸出 | 灵敏弹簧 | 无 | 无 |
| 中心标签 | 标签淡变 | 120 ms 交叉淡化 | 立即 |

## 响应式

- 圆环保持其尺寸，默认直径为 260 px，并会移动以保持在窗口之内，因此只要窗口宽于约 280 px 即可容纳。在手机尺寸的视图上，优先使用六项或更少，或使用朝远离最近边缘方向打开的扇形。
- 触摸通过按住来打开菜单；随后手指可以轻甩并抬起以选择，或抬起后点按某一项。触摸没有悬停，因此在手指移动或点按之前，不会高亮任何项。
- 无法放入扇区的标签以省略号结尾，中心会显示高亮项的完整标签。

## 相关组件

- `ContextMenu`：指针处的操作列表，支持分组、勾选和快捷键提示。
- `DropdownMenu`：位于可见且带标签触发器之后的操作。
- `SegmentedControl`：当选择的是一个始终显示在屏幕上的值时使用。

| 组件 | 选择时机 |
|---|---|
| `ContextMenu` | 超过八个操作、需要分组或快捷键提示 |
| `DropdownMenu` | 操作需要可见的触发器 |
| `SegmentedControl` | 选择的是一个始终可见的值 |
| `CommandPalette` | 人们通过名称搜索命令 |
