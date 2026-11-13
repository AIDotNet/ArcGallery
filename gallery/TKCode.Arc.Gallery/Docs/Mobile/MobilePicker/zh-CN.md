---
title: 移动端选择器
summary: 行尾显示当前值的列表行，点按后打开一个操作面板，列出可选项并勾选当前项。
source-hash: 4d94d890328e
---

<Demo name="Basic" />

## 何时使用

`Picker` 是设置页里从短列表中选择一个值的方式：行上显示当前选中的内容，点按后从底部边缘弹出可选项。

- **有几个命名值的设置项**：语言、文字大小、默认日历。
- **选项能放进面板的表单字段**：短列表中的国家、货币、尺寸。
- **与其他行并列的一行**：它位于 `ListGroup` 中，与 `ListItem` 和开关并排，外观一致。

实时示例在手机外框中运行，那是桌面上的模拟；真机上的触摸反馈和面板的拖动这里尚未验证。

## 何时不用

- **两三个都应始终可见的选项**：使用 `MobileSegmented` 或 `MobileRadio`。
- **更适合用图片表达的选择**：使用 `MobileRadioCards`。
- **选项很多，或需要搜索的选项**：改为推入一个包含 `ListGroup` 和 `Searchbar` 的页面；操作面板超过大约八行就会滚动。
- **操作而非取值**：直接使用 `MobileActionSheet`。

## 用法

把选择器放进 `ListGroup`。它的内容是行的标题；选项用内联的 `Items` 写出，或者用 `ItemsSource`。`SelectedIndex` 和 `SelectedItem`（都是双向绑定）相互跟随。点按后会打开一个操作面板，列出各个选项并勾选当前项；选择一行会选中它并关闭面板，点取消则关闭面板且不做更改。

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

选择器会在它所在的 `Page` 上方打开面板，效果与 `Page.Overlay` 中的面板覆盖页面相同，因此无需在页面中添加任何东西；面板会自行为页面底部留出安全区。不在页面中时，则在窗口上方打开。未选中任何项时，行上显示 `PlaceholderText`（"Not set"），颜色较淡。面板的标题是 `SheetTitle`，若未设置则取行的标题（当它是文本时）；`SheetMessage` 会在标题下方增加一行说明，`CancelText` 设置取消行的文字（设为 null 则隐藏该行）。`IsOpen` 可在代码中打开和关闭面板。禁用的选择器，或没有选项的选择器，不会打开面板。

选择器是一个 `ListItem`，因此 `Icon`、`Leading`、`Subtitle` 和 `ShowChevron`（默认开启）与任何行一样可用；`Trailing` 不显示，因为行尾已经用来显示值。

### 文本与模板

选项显示其 `ToString()` 的结果。`DisplayMemberBinding` 从选项的某个成员读取文本；`ItemTemplate` 同时构建行尾的值和面板中的每一行，例如一个符号加一个名称。在以 NativeAOT 编译的应用中，请为模板设置 `x:DataType`，使其绑定被编译：

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

### 事件

只要选中项发生变化，就会触发 `SelectionChanged`：在面板中选择了一行、通过代码或绑定设置了 `SelectedItem` 或 `SelectedIndex`，或者清空了选择。它的参数包含 `OldItem`、`NewItem`、`OldIndex` 和 `NewIndex`。当用户选择一行时会触发 `ItemInvoked`，即使选中的是已经选中的那一行，因此它适合用于“选择后关闭此页面”这类场景。

```csharp
picker.SelectionChanged += (_, e) => Save((Currency?)e.NewItem);
```

在选项到达之前设置的索引会等待选项就绪。选项变化时，选中项保持原位，索引随之跟随；不再位于选项之中的项仍会显示，其索引为 -1。

### Token

`PickerToken` 保存 `ValueMaxWidth`（160），即值在以省略号结尾之前能占用的最大宽度，以便为标题留出空间。行的其余部分是一个 `ListItem`（`ListItemToken`），面板则是一个 `ActionSheet`（`ActionSheetToken`、`ActionSheetItemToken`）。颜色来自 ArcDesign token：值使用次要文本颜色，占位文字使用弱化文本颜色。

## API 参考

<!-- api: MobilePicker -->

## 无障碍

行是一个按钮，其名称即标题；它的帮助文本为当前值（或占位文字），因此屏幕阅读器会同时读出两者。Tab 可以移到该行，按 Enter 或空格即可打开面板；此时已勾选的行获得键盘焦点，Tab 在各行之间移动，Esc 关闭面板。面板关闭后，焦点返回选择器。使用键盘聚焦的选择器会像按下的行一样着色。每个选项都是 56 px 高的行，当前项以对勾标记，而不只靠颜色区分。

## 动效

面板是一个 `ActionSheet`，依其物理特性运动：它以平滑的弹簧升起，拖动时跟随手指，松手时把手指的速度交给弹簧；减弱动效时则直接跳到位置。行在按下时以短暂的淡入改变底色，与 `ListItem` 一样。对勾随选中项出现，本身没有动效。

## 响应式

- `Picker` 没有断点。它是一个宽度占满、高度至少 48 px 的行，其面板在任何宽度下都横跨页面，两侧各留 8 px。
- 行尾的值最宽为 `ValueMaxWidth`（160 px），超出部分以省略号结尾，因此过长的值不会挤掉标题；标题占用行中的其余部分。
- 面板随行数增高，最高为页面的 90%，之后纵向滚动，从不横向滚动。它会为页面底部留出安全区，因此取消行不会被 Home 指示条遮住。
- 没有任何功能依赖悬停：手指按下时行会变深，点按即打开面板。

## 相关组件

- `MobileActionSheet`：选择器所打开的面板，及其 `IsChecked` 行。
- `MobileListGroup`：选择器所在的分组。
- `MobileRadioCards`：适合更宜用图片表达的选择。
- `MobileSegmented`：适合两到四个需要始终可见的选项。
