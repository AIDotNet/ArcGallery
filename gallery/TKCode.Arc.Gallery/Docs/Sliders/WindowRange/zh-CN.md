---
title: 时间窗口
summary: 叠在实时面积图上的可移动窗口，拖动边缘调整大小，甩动即可平移，日期随之滚动。
source-hash: e05b7451ea04
---

<Demo name="Report" />

## 何时使用

`WindowRange` 用来选取序列中的一段，例如报表的日期范围，同时让数据的形状始终可见。窗口内的部分以全色绘制，其余部分淡出，使用者松手之前就能看到自己选中了什么。

- **分析报表的日期范围**：每日会话、注册或收入，窗口移动时上方的合计随之更新。
- **比较时段**：让固定长度的窗口在用量或延迟上滑动，找出繁忙的几周。
- **概览条**：放在只显示所选区段的详细图表或表格上方。

## 何时不用

- **从日历中选取确切日期**，或范围跨越多年：使用 `DateRangePicker`。
- **用窗口缩放一张大型详细图表**并在图上标出事件：使用 `BrushChart`。
- **背后没有序列的数值范围**：使用带 `IsRange` 的 `Slider`；数值分布值得展示时使用 `RangeHistogramSlider`。

## 用法

在 `Data` 中传入序列，每个步长一个数字，并用 `ValueFormatter` 说明如何书写一个值。值采用定义域单位：默认 `Minimum` 为 0，`Maximum` 为最后一个点的索引，因此一组每日计数的定义域就是"第 0 天到第 N 天"。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:WindowRange Label="Report range"
                     Data="{Binding DailySessions}"
                     Start="{Binding From}"
                     End="{Binding To}"
                     MinSpan="6"
                     IsLive="True" />
</UserControl>
```

```csharp
var first = new DateTime(2026, 3, 2);
range.ValueFormatter = index => first.AddDays(index).ToString("MMM d", CultureInfo.InvariantCulture);
range.SpanFormatter = span => $"{span + 1:N0} days";
range.Ticks = [new WindowRangeTick(30, "Apr"), new WindowRangeTick(60, "May")];
```

`Start` 和 `End` 是双向绑定的。保持为 null 时，窗口覆盖定义域的最后四分之一。`WindowStart` 和 `WindowEnd` 始终是实际绘制的窗口，已应用步长和跨度限制。`ValueChanged` 在每次变化时触发，拖动或惯性滑行进行中也会触发，因此适合计算实时合计。`ValueCommitted` 在拖动、甩动或按键之后窗口静止时触发一次：在这里获取报表。

`SpanFormatter` 收到的是终点减起点；要得到包含首尾的天数需要加一。在狭窄的列中设置 `ShowSpan="False"` 可以去掉中间的读数。

### 快捷范围

设置范围的按钮调用 `SetRange(start, end)`：两端同时改变，`ValueChanged` 只触发一次，框架滑行过去，读数随之滚动。上面的示例只有在窗口恰好等于某个快捷范围时才把它标为选中。

### 限制与步长

两端都吸附到 `Step`。`MinSpan` 和 `MaxSpan` 限制窗口的长度（默认分别为一个步长和整个定义域）。方向键按 `Step` 移动；Page Up、Page Down 以及 Shift 加方向键使用 `LargeStep`（未设置时为定义域的十分之一）。

### 实时数据

序列变化时给 `Data` 赋一个新列表：点数相同时，折线和面积平滑变形，否则直接替换图表。`IsLive` 会在最新的点上显示一个脉动的圆点；只在该点仍在变化时打开它。

## API 参考

<!-- api: WindowRange -->

## 无障碍

- 控件是一个以 `Label` 命名的组。起点边缘、窗口和终点边缘是三个滑块，分别命名为"Report range, start""Report range, window"和"Report range, end"。
- 边缘报告自己能到达的范围，该范围遵守 `MinSpan` 和 `MaxSpan`，并通过 `ValueFormatter` 朗读自己的值。窗口报告其起点，并朗读两端，例如"Jun 2 to Jul 1"。
- 图表、框架和读数对辅助技术而言是装饰；所有值都由滑块承载。
- 按下会把焦点交给命中的部分，但不显示焦点环。键盘焦点会显示焦点环：边缘的焦点环围绕边缘，窗口的焦点环围绕整个框架。

### 键盘

| 按键 | 操作 |
|---|---|
| `Tab` | 依次经过起点边缘、窗口和终点边缘 |
| `Right` / `Up` | 把获得焦点的边缘或整个窗口移动一个 `Step` |
| `Left` / `Down` | 往回移动一个 `Step` |
| `Shift` + 方向键、`PageUp` / `PageDown` | 移动一个 `LargeStep` |
| `Home` / `End` | 窗口：移到定义域的起点或终点。边缘：在跨度限制允许的范围内移到最远 |
| 在限制处按键 | 值不变；框架朝按键方向撞一下再弹回 |

## 动效

- 拖动边缘或窗口时一比一跟随指针。窗口越过定义域末端时最多让出 18 px，边缘越过跨度限制时最多让出 10 px，然后弹回。
- 松开平移时保留速度：窗口落在甩动约 0.2 s 能到达的位置并吸附到步长，以一个从该速度出发的弹簧减速停下。滑行时抓住窗口会让它停在原处。
- 在窗口外按下会让窗口滑行到以指针为中心的位置，指针移动时窗口继续跟随。
- 按键、快捷范围和新值使用与惯性滑行相同的弹簧移动框架。读数按变化方向逐位滚动，月份名称作为一个整词滚动；拖动或滑行时读数原地更新。
- 按住窗口时，窗口内出现淡淡的着色，两个握把都会拉长；按住边缘时，它自己的握把拉得更长。
- 新数据以柔和的弹簧改变折线和面积的形状，实时圆点每 2.4 s 发出一圈光环。
- 减少动效时，框架和读数直接跳到每个值，没有滑行、拉伸、滚动、变形或脉动。关闭动效时一切都是即时的。

## 响应式

绘图区和框架填满给定的宽度，读数共用一行，长度位于中间。控件在约 320 px 以上可用；更窄的空间里用 `ShowSpan` 隐藏长度，标签会相互碰到时少传一些刻度。边缘在把手外 14 px 内都能被按到，因此用触摸也容易抓住，控件不依赖悬停。

## 相关组件

| 组件 | 适用场景 |
|---|---|
| `DateRangePicker` | 从日历中选取确切日期 |
| `BrushChart` | 用窗口缩放详细图表 |
| `RangeHistogramSlider` | 在分布之上选取数值范围 |
| `Slider` | 没有数据可展示的范围 |
