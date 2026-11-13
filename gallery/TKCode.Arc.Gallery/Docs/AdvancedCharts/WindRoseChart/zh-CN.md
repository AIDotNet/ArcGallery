---
title: 风玫瑰图
summary: 展示风从各个方向吹来的频率与强度：花瓣按风速分层堆叠，可选百分比或计数刻度，中心为静风占比。
source-hash: 450bb00caf1e
---

<Demo name="Meteorological" />

## 何时使用

`WindRoseChart` 展示方向上的分布，并在其上堆叠一个量值。每个方向扇区是一片花瓣，花瓣长度表示风从该方向吹来的频率，花瓣各层则按风速把这段时间切分开。无风向的静风时段位于中心。

- **气候与选址研究**：气象站一年、一季或一个月的风况，用于机场跑道、风电场、港口或建筑通风。
- **扩散与安全**：烟、噪声或泄漏物大多朝哪个方向飘散，以及飘散得有多快。
- **其他按方向与强度划分的数据**：浪向与浪高、交通的行驶方向与车速、声源方位。

图表在 8 或 16 个扇区、四到七个风速区间时效果最好。

## 何时不用

- **每个方向只有一个数值，没有可拆分的量值**：请使用 `RadarChart`。
- **随时间变化的风况**：请使用横轴为时间的 `LineChart` 或 `RealtimeStream`。
- **精确比较两个方向**：不同角度上的花瓣难以精确比较。请使用 `BarChart`。
- **没有环形顺序的类别**：花瓣围成一圈，因此只有末尾类别与首个类别相邻的数据才适合放进玫瑰图，例如罗盘方向或一天中的小时。

## 用法

提供行（每个方向一行，附带每个风速区间的数量）、记录（每条记录包含一个方向、一个区间和一个数量）或你自己的对象，并可选择提供静风数量。若不设置 `Bins`，图表使用六个风速区间，单位为 m/s：0.5–2、2–4、4–6、6–8、8–10 以及 10 及以上。

```xml
<arc:WindRoseChart Label="Harbour station, March" SectorCount="8" Calm="12">
    <arc:WindRoseRow Direction="N" Values="10 6 2 0 0 0" />
    <arc:WindRoseRow Direction="NE" Values="14 9 4 1 0 0" />
    <arc:WindRoseRow Direction="E" Values="5 3 1 0 0 0" />
    <arc:WindRoseRow Direction="SE" Values="4 2 0 0 0 0" />
    <arc:WindRoseRow Direction="S" Values="8 7 3 1 0 0" />
    <arc:WindRoseRow Direction="SW" Values="12 18 15 8 3 1" />
    <arc:WindRoseRow Direction="W" Values="9 12 8 4 1 0" />
    <arc:WindRoseRow Direction="NW" Values="6 5 2 1 0 0" />
</arc:WindRoseChart>
```

### 结构

1. **花瓣**：每个方向扇区一片，以其方向为中心。扇区 0 位于顶部的北方，扇区按顺时针排列，与罗盘一致。
2. **风速层**：每片花瓣由风速区间从中心向外堆叠而成，层与层之间留有一小段间隔。
3. **环**：径向刻度，数值沿前两个扇区之间的边界标注。
4. **静风圆**：中心处静风读数的占比（或计数）。
5. **方向标签**：外环之外的罗盘方位，使用当前语言。
6. **提示**：方向、其总量以及每个风速层的数量。
7. **图例**：玫瑰图下方，显示每个风速区间的颜色与范围。

### 数据：行、记录或你自己的对象

`ItemsSource` 接受任何 `WindRoseRow` 和 `WindRoseRecord` 列表，两者可以混用；不设置时，图表读取内联的行（`Items`）。同一扇区、同一区间的条目会累加，可观察集合的变化也会被跟踪。

| 类型 | 成员 | 说明 |
|---|---|---|
| `WindRoseRow` | `Direction` | 从北开始顺时针计算的度数（数字），或方向名称，如 `NNE`、`NbE`，或 `DirectionLabels` 中的某个标签 |
| `WindRoseRow` | `Values` | 每个风速区间一个数量，由内向外排列；在 XAML 中写作 `Values="1.2 3.4 0.8"` |
| `WindRoseRecord` | `Direction` | 同 `WindRoseRow` |
| `WindRoseRecord` | `Bin` | 风速区间的索引 |
| `WindRoseRecord` | `Value` | 数量 |
| `WindRoseBin` | `Minimum` | 区间的最低风速；下一区间的最小值即本区间的上限，最后一个区间没有上限 |
| `WindRoseBin` | `Label` | 图例和提示中的名称；默认为区间范围加上 `Unit`，如 "2–4 m/s" |
| `WindRoseBin` | `Color` | 固定颜色；默认取图表渐变色中对应该区间的那一档 |

方向归属于包含它的扇区：16 个扇区时，扇区 1（NNE）覆盖 11.25° 至 33.75°（不含 33.75°），360° 即北方。缺失、为负或不是数字的数量按零计算；超出 `Bins` 范围的区间索引会被忽略。

自定义对象无需包装。把成员绑定指向它们即可，可以使用长格式（每个对象一个数量），也可以使用宽格式（每个对象一个方向，并附带一组数量）：

```xml
<arc:WindRoseChart ItemsSource="{Binding Readings}"
                   DirectionMemberBinding="{Binding Heading}"
                   BinMemberBinding="{Binding SpeedClass}"
                   ValueMemberBinding="{Binding Hours}" />
```

### 原始观测数据的分箱

`WindRoseBinning.Bin` 将原始读数（以度表示的风向与风速）分入扇区与风速区间。风速低于静风阈值（默认 0.5）的读数，无论风向如何都计为静风；风向或风速不是数字、或风速为负的读数会被跳过，并计入 `Skipped`。

```csharp
var readings = station.Hours.Select(h => new WindObservation(h.DirectionDegrees, h.SpeedMetresPerSecond));
var wind = WindRoseBinning.Bin(readings, sectorCount: 16);
chart.ItemsSource = wind.Rows;
chart.Calm = wind.Calm;
```

传入 `binMinimums` 可使用其他区间（并让图表使用相同的 `Bins`），传入 `calmThreshold` 可改变静风的判定标准。泛型重载通过两个选择器读取任意项：`WindRoseBinning.Bin(hours, h => h.Heading, h => h.Knots, 16)`。

### 扇区与方向

`SectorCount` 设置方向的数量。8、16 和 32 个扇区使用罗盘标签（N、NE……；N、NNE……；N、NbE……）；其他从 4 到 72 的数量则以度数标注，例如 36 个 10° 的扇区。`StartAngle` 旋转玫瑰图，使扇区 0 位于屏幕上的另一个角度；`SweepDirection="CounterClockwise"` 让扇区逆时针排列。两者都不改变数据的归属：方向始终是罗盘度数。

<Demo name="SectorCounts" />

`DirectionLabels` 替换扇区 0 起的标签；空白或缺失的条目保留该扇区的默认标签，数据也可以用自定义标签指定扇区。

```csharp
chart.SectorCount = 4;
chart.DirectionLabels = ["Bow", "Starboard", "Stern", "Port"];
```

### 风速区间、颜色与单位

默认情况下，各区间取自主题的第一个图表颜色的同一色相：浅色表面上由浅到深，深色表面上由深到浅，因此风速越大看起来越强，并且图表会跟随强调色和外观模式。为某个区间设置 `Color` 即可固定其颜色，设置 `Unit` 则决定默认标签的单位。固定颜色在两种外观下保持不变，因此应选择在浅色和深色表面上都清晰可辨的颜色。

<Demo name="CustomBins" />

### 百分比或计数刻度

使用 `RadialAxis="Percent"`（默认）时，花瓣的长度是其占全部读数的比例（包含静风），因此花瓣与静风圆相加恰好为 100%。`RadialAxis="Count"` 则按原值绘制数量。`RadialMaximum` 固定外环处的数值（超出的花瓣延伸至边缘为止，提示仍显示真实数值），`RadialStep` 设置环与环之间的距离；保留 NaN 时，图表会选择一个整齐的最大值，环数不超过五个。

<Demo name="CountAxis" />

```xml
<arc:WindRoseChart RadialMaximum="20" RadialStep="5" ItemsSource="{Binding Months[0].Rows}" />
```

`ValueFormatter` 格式化所有显示的数值（环刻度、提示、中心及读数）。它接收的是百分比（0 到 100）或数量本身。

### 静风

`Calm` 是静风的数量，单位与数值相同：数值为计数时它也是计数，数据已是百分比时它就是百分比（此时百分比刻度会原样保留）。中心以占比或计数显示它；若静风圆太小、放不下文字，则只有摘要和读数会显示它。

### 实时数据

替换 `ItemsSource`、修改集合，或修改 `Calm`、`Bins` 或 `SectorCount`，每个色带都会从屏幕上的当前位置弹至新的长度，同时环随新的最大值移动。同一调度周期内对集合的修改只会读取一次，因此清空后再重新填充，玫瑰图会一次性移动到位。

<Demo name="LiveUpdate" />

### 读取与选择方向

悬停在花瓣（或其标签）上，或使用方向键移动，会设置 `ActiveIndex` 并显示提示；其他花瓣变淡，指针所在的层会显示轮廓。悬停或聚焦图例项时，该风速区间会在所有花瓣中高亮。

点击、轻触、Enter 或 Space 会引发 `ItemInvoked`，携带扇区以及指针下方的层（通过键盘触发时 `Bin` 为 -1）。设置 `IsSelectionEnabled` 后，该扇区还会成为 `SelectedIndex`，并以轮廓突出显示；`SelectionChanged` 报告 `OldItem` 与 `NewItem`。无论变化来自手势、代码还是绑定，只要 `SelectedIndex` 改变就会引发它。`Sectors` 列出每个扇区及其标签、范围、数量、总计和占比。

<Demo name="Selection" />

### 绑定到视图模型

```xml
<arc:WindRoseChart Label="{Binding StationName}"
                   ItemsSource="{Binding Wind.Rows}"
                   Calm="{Binding Wind.Calm}"
                   SectorCount="{Binding Sectors}"
                   SelectedIndex="{Binding SelectedDirection, Mode=TwoWay}"
                   IsSelectionEnabled="True" />
```

- `ItemsSource` 可以是数组、列表或可观察集合。就地修改某一行的 `Values` 不会被察觉：请替换该行、列表或集合中的项。
- `SelectedIndex` 与 `ActiveIndex` 支持双向绑定；-1 表示无。

### 方向名称与语言

默认标签取自当前的 Arc 语言：英文为 N、NNE、NE……，简体中文为 北、北东北、东北……，繁体中文为 北、北北東、東北……，语言切换时标签随之变化。"Calm" 也跟随语言显示为"静风"。数据可以使用英文缩写（`NNE`、`NbE`）或当前语言的名称来指定方向。

### 内容指南

- **Label**：写明地点与时段，例如"港口站，2025 年每小时风况"。
- **Bins**：四到七个，按升序排列。区间边界取整，并通过 `Unit` 统一标注一次单位。
- **自定义标签**：保持简短；字号较小时，标签会每两个、每四个方向显示一个。

### 最佳实践

**推荐**

- 说明花瓣显示的是占全部读数的比例还是计数，以及是否包含静风。
- 比较多个玫瑰图时，保持相同的区间，并固定 `RadialMaximum`。
- 用与图表显示相同的扇区数对原始读数分箱。

**避免**

- 比较花瓣面积：请读取长度与提示。
- 把为 16 个扇区准备的数据重新归入 8 个扇区：NNE 这类半途方向会落入顺时针方向的下一个扇区。请重新对原始读数分箱。

### 常见问题

**为什么我的花瓣看起来很短？** 在百分比模式下，静风读数计入 100%。可使用 `RadialMaximum` 放大，或改用 `RadialAxis="Count"`。

**可以显示 36 个扇区吗？** 可以：`SectorCount="36"`；标签显示为度数，并会稀疏以适应空间。

## API 参考

<!-- api: WindRoseChart -->

<!-- api: WindRoseRow -->

<!-- api: WindRoseRecord -->

<!-- api: WindRoseBin -->

<!-- api: WindRoseSector -->

<!-- api: WindRoseBinning -->

## 无障碍

- 玫瑰图是可聚焦的组。它的自动化名称是图表的 `Label` 加上按键说明；读取某个方向时，会播报该方向、其总计及每个风速层，例如 "SW: 14.1%. 0.5–2 m/s 1.0%, 2–4 m/s 3.0%, …"，被选中的方向则追加 ", selected"。
- 没有读数时，帮助文本概括静风占比以及每个方向的总计。
- 风速区间在图例、提示和读数中都有名称，并且默认颜色按明度排序，因此颜色不是唯一的区分依据。每个图例项都是一个 Tab 停靠点，以其区间及占读数的比例命名。

### 键盘

| 按键 | 操作 |
|---|---|
| `Tab` | 聚焦玫瑰图并读取选中的方向（若无则读取第一个），然后依次经过图例项。获得焦点的图例项会高亮其风速区间 |
| `Right` / `Down` | 读取屏幕上顺时针方向的下一个方向 |
| `Left` / `Up` | 读取屏幕上逆时针方向的下一个方向 |
| `Home` / `End` | 读取第一个或最后一个方向 |
| `Enter` / `Space` | 选择正在读取的方向（触发 `ItemInvoked`；启用 `IsSelectionEnabled` 时同时选中） |
| `Escape` | 清除读数；若没有读数，则清除选择 |

## 动效

- 首次显示时，各色带从中心向外生长，并在轻微回弹的弹簧上定位。
- 新数据、新的刻度模式或新的扇区数会使每个色带弹至新长度；环随新的最大值移动。扇区数变化时，每片新花瓣从覆盖其方向的旧花瓣处生长出来。
- 读取某个方向时，其他花瓣变淡；提示在各方向之间滑动。
- 减弱动效时，变化立即呈现；首次显示为短暂淡入。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 首次显示 | 色带从中心生长 | 短暂淡入 | 立即 |
| 数据或刻度变化 | 色带与环弹动 | 立即 | 立即 |
| 读数移动 | 花瓣变淡，提示滑动 | 立即 | 立即 |

## 响应式

- 玫瑰图会缩小以适应获得的宽度和高度，且不会超过 `Size`。
- 宽度低于 360 时，方向标签环收紧。在任何尺寸下，若方向标签会相互碰撞，它们会稀疏为每两个、每四个或每八个方向显示一个；四个基本方位始终保留，正在读取的方向也始终显示。
- 环标签在环靠得太近时会稀疏；静风圆太小、放不下时，中心文字会隐藏。
- 图例会换到更多行；提示始终留在图表之内，有空间时位于花瓣旁边。
- 触屏按下时读取手指所在的方向，拖动时扫过其他方向，轻触则选中它。

## 相关组件

| 组件 | 适用情况 |
|---|---|
| `RadarChart` | 每个方向或度量只有一个数值 |
| `BarChart` | 方向之间需要精确比较，或没有环形顺序 |
| `Sunburst` | 占比是层级嵌套的，而不是分布在一圈上 |
| `LineChart` | 风速或风向随时间变化 |
| `ActivityHeatmap` | 两个循环维度，例如一天中的小时与星期 |
