---
title: 环形进度
summary: 同时支持确定圆形进度与不确定旋转加载状态的环形指示器。
source-hash: 47e69f10a656
---

<Demo name="Overview" />

## 何时使用

`ProgressRing` 在紧凑的圆形空间中传达正在进行的操作或可度量的完成进度。当水平进度条过宽，或任务总量未知、需要明确的加载指示时使用。

- **不确定加载状态**：将 `IsIndeterminate="True"` 用于数据加载、视图刷新或卡片、对话框与工具栏中的后台等待。
- **紧凑的确定进度**：适用于上传、下载、导出或同步任务，将环形进度放在条目旁或空状态居中位置。
- **行内状态指示**：使用 `Size="Small"` 配合状态文案或放在表格单元格中表示正在处理。

## 何时不用

- **表单或列表中的整行进度任务**：有充足水平空间且需要左侧标题行时，使用 `Progress`。
- **页面或卡片内容占位**：加载内容前希望保留版面结构时，使用 `Skeleton`。
- **带阈值分档的仪表板指标**：需要 270° 刻度与警告/危险区间时，使用 `Gauge`。

## 用法

`ProgressRing` 默认为确定模式（`IsIndeterminate="False"`），通过 `Value`、`Minimum`（默认 `0`）和 `Maximum`（默认 `100`）设置进度；设置 `IsIndeterminate="True"` 可切换为持续旋转的加载环。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <StackPanel Orientation="Horizontal" Spacing="24">
        <arc:ProgressRing IsIndeterminate="True" Label="Loading" />
        <arc:ProgressRing Value="68" IsValueVisible="True" Label="Uploading" />
    </StackPanel>
</UserControl>
```

### 尺寸

支持 `ControlSize.Small`（20 px）、`ControlSize.Medium`（40 px，默认）和 `ControlSize.Large`（64 px），也可以显式设置 `Width`、`Height` 和 `Thickness`。

<Demo name="Sizes" />

### 确定进度

单一平滑弹簧同时驱动顺时针圆弧与中心百分比。当 `Value` 达到 `Maximum` 时，圆环切换为成功色；开启 `IsValueVisible` 时，中心百分比会切换为完成对勾。

<Demo name="Determinate" />

### 自定义中心内容

设置 `Content`（以及可选的 `ContentTemplate`）可在圆环中心放置自定义文字、计数或图标。设置自定义内容后会覆盖内置的百分比读数。

<Demo name="CustomContent" />

## API 参考

<!-- api: ProgressRing -->

## 无障碍

- `ProgressRing` 为只读指示器，不获取键盘焦点（`Focusable="False"`）。
- 自动化控件类型为 `ProgressBar`。
- `Label` 作为无障碍名称；未设置标签时，不确定模式默认为 `"Loading"`，确定模式默认为 `"Progress"`。
- `AutomationProperties.ItemStatus` 在不确定模式下报告 `"Loading"`，在确定模式下报告当前百分比（如 `"68%"`）。

## 动效

- **确定模式**：由 `ArcSpring.Smooth` 弹簧同步驱动圆弧与百分比；达到 100% 后延迟 200 ms 切换为 `Success` 颜色并弹出完成对勾。
- **不确定模式**：由 `LiveLoop` 驱动的弧段在可见且处于视口内时持续旋转，离开视口自动暂停。
- **减弱动效**：确定模式直接跳至目标值；不确定模式停止旋转并改为柔和的不透明度呼吸。
- **关闭动效**：所有变化立即到位，不确定模式显示静止圆弧。

## 响应式行为

`ProgressRing` 根据 `Size` 预设（`Small`、`Medium`、`Large`）确定尺寸，或在显式 `Width` 与 `Height` 内保持正圆并居中显示在可选 `Label` 上方。

## 相关

- `Progress`：带标题行的水平进度条。
- `Skeleton`：加载期间保留版面结构的骨架屏。
- `Gauge`：带阈值分档的仪表板指标盘。
