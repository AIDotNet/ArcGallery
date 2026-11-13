---
title: 移动端单选卡片
summary: 以图片或简短描述呈现的可选卡片网格，适合一次只能选一项的选择；有按下状态，没有悬停状态。
source-hash: bbf600b5d55c
---

<Demo name="Basic" />

## 何时使用

`RadioCards` 把一项选择排成卡片网格，因此每个选项都可以展示图标、色块或一行描述。

- **套餐、配送速度、布局**：每个选项都需要一句话才能区分的场景。
- **形状、颜色、样式**：比起名称，更容易靠看来辨认的选项。
- **设置页中的一项选择**：把网格放进 `ListGroup` 的 `ListCell` 里。

实时示例在手机外框里运行，那是桌面上的模拟；真机上的触摸反馈这里没有验证。

## 何时不用

- **只有简短标签的选项**：用 `MobileSegmented`（两到四项）或 `MobileRadio`（列表）。
- **值显示在一行里的设置**：用 `MobilePicker`。
- **任意数量的选项同时选择**：单选卡片只保存一项选择；用 `MobileCheckbox`。

## 用法

在分组中写入 `RadioCardOption`，或者把 `ItemsSource` 绑定到一个选项列表。每个选项都有 `Value`、`Label`、可选的 `Description` 和可选的 `Icon`。`SelectedValue`（双向）是选中选项的 `Value`；`SelectedIndex` 和 `SelectedItem` 的用法与任何 Avalonia 选择控件相同，`SelectionChanged` 会报告每一次选择的变化。

```xml
<arcm:RadioCards xmlns="https://github.com/avaloniaui"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                 xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                 SelectedValue="{Binding Plan}">
    <arcm:RadioCardOption Value="free" Label="Free" Description="3 projects and 1 GB of storage." Icon="{x:Static arc:ArcIcons.Zap}" />
    <arcm:RadioCardOption Value="pro" Label="Pro" Description="Unlimited projects and priority support." Icon="{x:Static arc:ArcIcons.Star}" />
    <arcm:RadioCardOption Value="enterprise" Label="Enterprise" Description="Talk to sales first." IsEnabled="False" />
</arcm:RadioCards>
```

卡片在 `Columns` 指定的列数之间平分宽度（默认 2 列），同一行的卡片高度与其中最高的一张相同。`Icon` 如果是几何图形（例如 `ArcIcons` 中的 Lucide 图标），会以 22 px 的描边图标绘制；其他内容（`Ellipse` 色块、形状）则原样显示。手指按在卡片上会使其变暗，启用完整动效时还会轻微下压；在卡片上抬起手指即选中它，而从卡片上开始的滚动不会选中。

`IsEnabled="False"` 会让选项变暗且无法选中；`IsVisible="False"` 会隐藏它，并且不占空间。卡片会跟随选项之后的变化。不是选项的数据项可以用 `ItemTemplate` 显示；此时卡片的标签就是该模板。

### 紧凑选择

对于色块和形状，用 `ShowIndicator="False"` 关闭单选指示器，并使用三列：滑动到选中卡片的圆环本身就标示了选择。

<Demo name="Swatches" />

```xml
<arcm:RadioCards Columns="3" ShowIndicator="False" SelectedValue="{Binding Color}">
    <arcm:RadioCardOption Value="violet" Label="Violet">
        <arcm:RadioCardOption.Icon>
            <Ellipse Width="28" Height="28" Fill="#6E56CF" />
        </arcm:RadioCardOption.Icon>
    </arcm:RadioCardOption>
</arcm:RadioCards>
```

### Token

`RadioCardsToken`（标识 `MobileRadioCards`）保存 `ColumnGap` 和 `RowGap`（10）以及 `RingThickness`（2）。`RadioCardToken`（标识 `MobileRadioCard`）保存卡片的 `CardPadding`（12）、`IconExtent`（22）、`LabelMargin`、`DescriptionMargin`、`IndicatorSize`（22）、`IndicatorBorderWidth`（1.5）、`DotSize`（10）、`IndicatorMargin`、`DisabledOpacity`（0.5），以及焦点环的 `FocusRingMargin` 和 `FocusRingRadius`。标识带有 `Mobile` 前缀，是因为桌面包有自己的单选卡片。颜色、圆角和字体来自 ArcDesign token：圆环和圆点使用 `ControlOn`，与移动端单选一致。

## API 参考

<!-- api: MobileRadioCards -->

<!-- api: MobileRadioCard -->

<!-- api: MobileRadioCardOption -->

## 无障碍

整个分组是一个 Tab 停靠点：Tab 会落在选中的卡片上（若没有选中项，则落在第一张可选的卡片上）。方向键在网格中移动选择，左右键按阅读顺序移动（从右到左的布局中会镜像，并且循环），上下键按行移动；禁用和隐藏的卡片会被跳过。Home 和 End 跳到第一张和最后一张卡片，Enter 或 Space 选中当前有焦点的卡片。键盘焦点会在卡片周围显示 2 px 的焦点环。卡片以其标签命名，其描述即为帮助文本。选中状态由圆环和实心的单选圆点标示，而不只靠颜色；当 `ShowIndicator="False"` 时，较粗的圆环仍然保留。

## 动效

选择变化时，一个圆环以弹簧效果从一张卡片滑到另一张；圆环在渲染阶段绘制，因此布局不会移动。圆环第一次出现时会原地淡入。单选圆点以短暂的淡入弹入。按下的卡片以快速曲线变暗，仅在启用完整动效时，还会以较灵敏的弹簧缩放到 0.97。减少动效时，圆环直接跳到目标位置，不做缩放，圆点和颜色立即变化。

## 响应式

- `RadioCards` 没有断点：`Columns`（默认 2）指定的等宽列平分分组得到的宽度，同一行的卡片高度与其中最高的一张相同。
- 在手机上，带指示器和文字的卡片保持两列；三到四列适合带简短标签的图标和色块，并使用 `ShowIndicator="False"`。标签和描述会换行，因此卡片会变高而不是变宽。
- 隐藏的选项不占空间，其后的卡片会向上补位进入网格。
- 没有任何功能依赖悬停：手指按住会使卡片变暗，抬起即选中该卡片。

## 相关组件

- `MobileRadio`：以列表行呈现的一项选择。
- `MobileSegmented`：一行中的两到四个简短选项。
- `MobilePicker`：显示值的一行，点击后在底部面板中打开选项。
- `MobileListGroup`：把网格放进 `ListCell` 中，以便与其他行一起排列。
