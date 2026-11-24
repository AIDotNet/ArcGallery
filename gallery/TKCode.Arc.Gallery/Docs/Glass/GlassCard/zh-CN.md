---
title: Glass card
summary: 一张磨砂玻璃卡片：模糊身后的内容，带柔和的边缘光，以及随指针或倾斜信号移动的光泽。
source-hash: 0e64697ebbe9
---

<Demo name="Preview" />

## 何时使用

`GlassCard` 是一张磨砂玻璃做成的内容卡片，适合背景本身就是设计一部分的地方：照片、插画、地图或色彩丰富的首屏。卡片显示身后内容经过模糊和着色的副本，背景仍能看出颜色和形状，卡片上的文字依然清晰。一条细细的边缘光沿卡片四周延伸，左上角最亮，到右下角淡成一道浅影，所以即使在复杂的图片上，卡片也像一块浮起的玻璃。一片柔和的光泽随指针在玻璃上移动，并照亮附近的边缘；设置 `MaxTilt` 后，卡片还会朝指针倾斜。同样的光也可以通过 `Tilt` 跟随其他信号，比如设备的方向。

- **首屏图片上的摘要**：海岸线上的今晚天气、城市地图上的行程。
- **插画页面上的浮动面板**：登录卡片、方案选择或一条简短通知。
- **展示区中的重点卡片**：一件产品、一张通行证或一项功能，让它看起来像一个真实的物件。

## 何时不用

- **纯色背景的普通页面**：没有可以模糊的内容。使用更轻、更清楚的 `Card`。
- **长文本或密集的表单**：透过玻璃的内容会降低对比度。保持文字简短、字号较大，或改用不透明的表面。
- **网格中的多张卡片**：每张卡片都要在自己下面重新绘制并模糊背景。同时只用一到两张。
- **带彩虹箔和翻面的收藏卡**：使用 `HoloCard`。

## 用法

在容器上把 `arc:GlassBackdrop.Source` 设为玻璃身后的那一层。容器内的每个玻璃表面都会显示它所覆盖的那一部分的模糊副本。来源必须是卡片身后的一层（同级元素或它的子元素），不能是卡片的上级元素。

```xml
<Panel arc:GlassBackdrop.Source="{Binding #Scene}">
    <Border Name="Scene" Background="#2B3A55" /><!-- your picture or page -->
    <arc:GlassCard Width="320" HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock Text="Evening swim" />
    </arc:GlassCard>
</Panel>
```

`GlassCard` 派生自 Avalonia 的 `ContentControl`，位于 Special 包中：在 `UseArc()` 旁调用 `UseArcSpecial()` 注册。

### 磨砂效果的做法

Avalonia 的效果（如 `BlurEffect`）只作用于元素自己的绘制内容，没有公开的滤镜可以处理元素身后的内容。因此玻璃在自己下面把背景再画一遍：用来源的 `VisualBrush`，放在来源相对卡片的位置上，只对这一层加 `BlurEffect`，并按卡片的圆角裁剪。着色、上方的光泽层、指针光泽和边缘光画在它上面，内容再画在最上面。全部使用公开的 Avalonia API。

这个副本是玻璃绘制时对来源拍下的一张图。卡片相对来源移动或改变大小、主题变化时，会重新拍一次。来源自己会变化时（视频、实时图表、动画插画），设置 `arc:GlassBackdrop.IsLive="True"`，卡片在屏幕上时每一帧都会重画副本。

没有来源时，卡片是一块不带模糊的半透明着色玻璃：身后内容清晰可见，所以着色更深。开启 ArcDesign 的 `ReduceTransparency` 令牌时，卡片是带普通边框的不透明浮起表面。

### 原生背景模糊

`arc:GlassBackdrop.Mode="Native"` 让容器内的玻璃改用 Avalonia 自带的背景效果（`Visual.BackdropEffect`，Avalonia 12.2）：
由合成器直接模糊玻璃身后已经画好的像素，因此不需要来源，也不会把任何内容画两遍，即使来源每帧都在变化。默认仍是 `Brush`，即上面
介绍的副本方式。

```xml
<Panel arc:GlassBackdrop.Mode="Native">
    <Border Background="#2B3A55" /><!-- your picture, video or page -->
    <arc:GlassCard Width="320" HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock Text="Evening swim" />
    </arc:GlassCard>
</Panel>
```

两种模式的着色、边缘光和光泽相同，模糊也像副本一样会读取卡片边缘之外的内容。取舍如下：

- 渲染器必须能读回已画的内容。页面的图片（`RenderTargetBitmap`、页面的 `VisualBrush`）和无头绘制只显示着色，没有模糊。
- 位于独立原生窗口中的弹出层身后什么都没有画，显示着色面板；位于覆盖层中的弹出层（Arc 默认）会模糊其下方的页面。
- 放在带 `Effect` 或 `OpacityMask` 的元素里时，玻璃没有模糊；普通的 `Opacity` 没有影响。
- `arc:GlassBackdrop.Cache` 决定背景的保存方式：`Retained`（合成器对模糊的默认选择）保留一张快照，只刷新变化的部分，指针光泽
  或滑动的选中块不会重新读取背景；`Volatile` 每次绘制都重新读取。未设置 `Cache` 时，`IsLive="True"` 会选择 `Volatile`。

### 倾斜与光

`MaxTilt`（默认 0，最大 20 度）让卡片以透视朝指针倾斜，指针下方的边缘向后退。`SheenIntensity` 在 0 到 1 之间调整光泽和边缘光的强度。在触摸屏上，手指按下时光会跟随手指。

指针不在卡片上时，`Tilt` 从其他来源获取光的方向：每个轴从 -1 到 1 的向量，(0, 0) 表示静止。可以接入设备方向、滚动位置或滑块。

<Demo name="TiltSignal" />

### 没有背景来源

没有设置 `arc:GlassBackdrop.Source` 的卡片依然是玻璃：一块半透明、着色更深的面板，带同样的边缘光和光泽，但没有模糊。

<Demo name="NoBackdrop" />

### 内容建议

- 文字保持简短并有分量：一个标题、一两行说明，再加一两个操作。
- 选择有大块柔和形状的背景。卡片后面的细节和小字会被模糊成杂色。
- 两种主题都要检查：浅色主题的玻璃像乳白色，深色主题的像烟色。

### 常见问题

**为什么来源什么都不显示？** 来源必须在同一个窗口中，并且不能包含卡片。卡片若在来源之内，绘制来源时就要绘制它自己，所以玻璃会退回为着色面板。

**在弹出层中能用吗？** 可以，前提是弹出层位于窗口的覆盖层中（Arc 弹出层默认如此）。在独立原生窗口中的弹出层看不到身后的窗口，会显示着色面板。

## API 参考

<!-- api: GlassCard -->

<!-- api: GlassBackdrop -->

## 无障碍

- 卡片是一个组；当它代表一件事时，给它设置 `AutomationProperties.Name`（例如“今晚的游泳预报”）。内容照常朗读；玻璃各层不在无障碍树中。
- 卡片本身不可聚焦；卡片里的控件可以。
- 玻璃会降低上面文字的对比度。卡片上的文字至少 14px，优先使用中等或半粗字重，并在实际背景上用两种主题测试。
- ArcDesign 的 `ReduceTransparency` 设置会把所有玻璃表面变成不透明表面。

## 动效

- 光泽以平滑弹簧跟随指针（约 0.35 秒，无回弹），指针进入时在 240ms 内淡入，离开时在 480ms 内淡出。
- 倾斜以约 0.45 秒的平滑弹簧跟随，无回弹。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 指针在卡片上 | 光泽和边缘光跟随，倾斜（设置 `MaxTilt` 时） | 无 | 无 |
| `Tilt` 变化 | 光泽和倾斜跟随 | 无 | 无 |

## 响应式

- 卡片没有断点：它占用给它的宽度；在宽布局中设置 `Width` 或 `MaxWidth`。
- 触摸时，手指按下期间光泽和倾斜跟随手指，抬起后恢复。
- 每次布局之后磨砂副本都会与来源重新对齐，所以调整窗口大小时副本保持在正确位置。

## 相关组件

- `Card`：普通表面上的内容卡片。
- `HoloCard`：带彩虹箔、闪粉和翻面的收藏卡。
- `GlassMenu`、`GlassSegmented` 和 `GlassTabBar`：使用同一种材质的其他玻璃表面。

| 组件 | 适用场景 |
|---|---|
| `GlassCard` | 卡片要浮在图片或彩色背景上 |
| `Card` | 普通页面上的内容卡片 |
| `HoloCard` | 一张卡片要像真实的收藏品 |
