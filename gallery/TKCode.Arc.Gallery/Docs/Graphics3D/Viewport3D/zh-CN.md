---
title: 3D 视口
summary: "在窗口里绘制你自己的 3D 场景：网格、基于物理的材质、灯光和阴影，只在有变化时才绘制，其他控件可以叠在上面。"
source-hash: 17465bfc1214
---

<Demo name="Preview" />

## 何时使用

- 你自己的场景：多个物体、自己的灯光和相机、用节点图写的材质、由你的代码驱动的动画。
- 放在普通布局里的 3D 图表、数字孪生、配置器和生成模型的预览，Arc 控件可以叠在画面上面或放在旁边。
- 大部分时间静止的画面。场景没有变化时不绘制任何帧。

## 何时不用

- 只需要从各个角度查看一个模型：用 `ModelViewer`，它已经配好灯光、地面阴影、相机控制和工具栏。
- 浏览器应用里每一帧都在变化的场景：在那里每一帧都要从显卡读回像素，开销比桌面上大（见“平台与预览状态”）。
- 固定的、装饰性的立体画面：营收地球和爆炸视图区块用 2D 绘制，不需要图形上下文。
- 需要稳定 API 的代码：两个 3D 包都是预览版。

## 用法

3D 支持由两个包组成。`TKCode.Arc.Graphics3D` 是引擎（场景图、几何体、材质、灯光、动画、glTF 加载器和渲染器），不依赖任何界面库，也不需要注册。`TKCode.Arc.Graphics3D.Controls` 包含 `Viewport3D` 和 `ModelViewer`，并带上引擎。在 `UseArc(...)` 里注册这两个控件：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

视口没有自己的尺寸，也没有外观：给它一个高度，或者放进 `Grid` 的单元格。

```xml
<arc:Viewport3D x:Name="Viewport" Height="360" />
```

场景在代码里用 `TKCode.Arc.Graphics3D` 命名空间的类型搭建。three.js 里的数值可以直接搬过来：单位相同（米），灯光强度相同，相机约定也相同（右手坐标系、Y 轴向上、视野用角度）。

```csharp
using System.Numerics;
using TKCode.Arc.Graphics3D;

var scene = new Scene();
var box = new Mesh(new BoxGeometry(1f, 1f, 1f), new StandardMaterial { Color = ColorF.FromHex(0xe5484d), Roughness = 0.4f })
{
    Position = new Vector3(0f, 0.5f, 0f),
    CastShadow = true,
};
var floor = new Mesh(new PlaneGeometry(12f, 12f), new StandardMaterial { Color = ColorF.FromHex(0x8a8f98) }) { ReceiveShadow = true };
floor.RotateX(-MathF.PI / 2f);
scene.Add(box, floor, new DirectionalLight(3f) { Position = new Vector3(3f, 5f, 2f), CastShadow = true }, new AmbientLight(0.4f));

var camera = new PerspectiveCamera(45f, 1f, 0.1f, 100f) { Position = new Vector3(3f, 2.5f, 4f) };
camera.LookAt(new Vector3(0f, 0.4f, 0f));

Viewport.Scene = scene;
Viewport.Camera = camera;
```

`AutoAspect`（默认开启）让透视相机的宽高比始终等于视口的宽高比，所以传给相机的那个值不重要。几何体、材质和纹理会一直占用显存，直到你调用它们的 `Dispose()`，或者视口离开窗口。

### 平台与预览状态

视口在 Avalonia 的合成树内部用 OpenGL 渲染。其他控件可以叠在它上面，它也和其他控件一样可以被裁剪、淡入淡出和变换。在 macOS 上，它在 Avalonia 的默认渲染器下就能工作，不需要改应用的设置。

- 两个包都是预览版，有自己的版本线（0.1.0-preview.1）。API 可能在两个预览版之间变化。
- 显卡驱动需要提供 OpenGL 3.3 或 OpenGL ES 3.0（在 Windows 上 Avalonia 自带的 ANGLE 满足这一点）；浏览器需要提供 WebGL 2。
- 目前在 macOS 上验证过，在 Windows 上验证过（一台 NVIDIA 显卡的机器：Avalonia 的默认渲染器、它的软件模式，以及显卡驱动自己的 OpenGL），在 Linux 的 X11 下用 Mesa 的软件 OpenGL 验证过，也在浏览器里用无头 Chromium 和软件 WebGL 验证过。带真实 GPU 驱动的 Linux、Wayland、其他显卡上的 Windows，以及真实显卡上的浏览器（包括 Firefox 和 Safari）尚未测试。
- 在 Linux 上，除非应用清空 `X11PlatformOptions.GlxRendererBlacklist`，Avalonia 不会用软件 OpenGL 驱动（llvmpipe）渲染，而是改用软件合成；这时视口走下一条所说的位图退路。
- Avalonia 用软件渲染时，视口在自己的 OpenGL 上下文里绘制，再把结果作为位图显示。每绘制一帧多一次像素读回，这时 `Presentation` 的值是 `Bitmap`。
- 在浏览器（WebAssembly）里，视口在页面的一个 WebGL 2 上下文里绘制，并以同样的方式把结果作为位图显示：Avalonia 的浏览器渲染器既不共享它的 WebGL 上下文，也不共享它的纹理。应用的浏览器项目要引用 `TKCode.Arc.Graphics3D.Browser` 包并启用它（见下文）。读回像素让每一帧比桌面上开销更大，所以尽量按需绘制。
- 完全没有 OpenGL 的机器、没有 WebGL 2 的浏览器，以及没有启用浏览器包的浏览器应用里没有画面。这时 `IsRendererReady` 保持为 false，`RendererError` 给出原因；请显示你自己的提示或静态图片。

```xml
<Grid Height="360">
    <arc:Viewport3D x:Name="Viewport" />
    <TextBlock Text="{Binding #Viewport.RendererError}"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
</Grid>
```

在浏览器应用里，让浏览器项目引用 `TKCode.Arc.Graphics3D.Browser`，并把它的入口加在其他入口旁边：

```csharp
// 浏览器项目的 Program.cs
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D().UseArcGraphics3DBrowser());
```

项目文件不需要再加别的东西：这个包用的是 Avalonia.Browser 已经链接进应用的 OpenGL ES 层。包的脚本加载完成之前就已经在屏幕上的视口，会在脚本到达后开始绘制。

### 按需绘制

`RenderMode` 默认是 `OnDemand`。视口在下列情况绘制一帧：

- 场景图发生变化：添加、移除、移动、显示或隐藏物体；
- 相机控制在移动；
- 视口改变尺寸，或它的某个属性改变；
- 你调用 `RequestRender()`。它的开销很小，下一帧之前无论调用多少次都只绘制一帧。

材质、纹理和灯光颜色的变化不会被观察到：改完之后调用 `RequestRender()`。

```csharp
material.Color = ColorF.FromHex(0x30a46c);
Viewport.RequestRender();
```

在 `FrameRendering` 里做动画，它在每一帧绘制之前在 UI 线程上触发；动画进行期间把 `RequestNextFrame` 设为 true。事件参数（`Viewport3DFrameEventArgs`）带有 `Elapsed`（距上一帧的时间，暂停后的第一帧为零）、`RequestNextFrame` 和 `Renderer`（绘制这一帧的渲染器）。本页顶部的预览就是这样旋转方块的。

```csharp
Viewport.FrameRendering += (_, e) =>
{
    if (spinning)
    {
        box.RotateY((float)e.Elapsed.TotalSeconds * 1.2f);
        e.RequestNextFrame = true;
    }
};
```

`RenderMode="Continuous"` 在视口可见时每个显示帧都绘制；只用于从不静止的场景。`Statistics` 报告上一帧的开销（物体数、绘制调用、三角形、CPU 时间），`Capture` 把一帧的像素交给回调：`Viewport.Capture(image => File.WriteAllBytes("scene.png", Png.Encode(image)))`。

### 相机控制

把 `CameraControls` 设为一个 `OrbitControls`，就可以让用户移动相机：拖动环绕，滚轮缩放，按住右键拖动平移。在触摸屏上，单指环绕，双指缩放和平移。方向键平移，按住 Shift 时方向键环绕。控制器还在滑行减速时，视口会继续绘制。没有 `CameraControls` 时，视口不响应输入。

```csharp
Viewport.CameraControls = new OrbitControls(camera)
{
    Target = new Vector3(0f, 0.4f, 0f),
    EnableDamping = true,
    MinDistance = 2f,
    MaxDistance = 12f,
    MaxPolarAngle = MathF.PI / 2f,
};
```

### 节点材质

材质只描述表面；光照、阴影和色调映射由引擎加上。每种内置材质都可以用节点代替它的数值（`ColorNode`、`RoughnessNode`、`OpacityNode`、`PositionNode` 等），`NodeMaterial` 则完全由节点定义。节点用 `TKCode.Arc.Graphics3D.Shading` 里 `Sl` 的函数构造，不写着色器文本。`Sl.Uniform` 给出一个之后可以修改而不必重建着色器的值，`Sl.Time` 是渲染器的时钟：只有在绘制帧的时候它才前进。

<Demo name="NodeMaterials" />

### 渲染线程

`UseRenderThread="True"` 把图形工作移到视口自己的线程上：UI 线程准备每一帧（动画、剔除、排序），渲染线程和显卡驱动打交道。在有上千次独立绘制的场景里收益明显，因为驱动占了一帧的大头；只显示一个模型时差别不大。它在视口自己拥有 OpenGL 上下文时生效（macOS，以及软件渲染下）；上下文属于 Avalonia 渲染器时（使用 GPU 渲染的 Windows 和 Linux），以及在只有一个线程的浏览器页面里，设置无效。请在视口加入窗口之前设置；`IsRenderThreadActive` 说明它是否在用。

### 换一个图形设备

`DeviceFactory` 用你创建的设备取代 OpenGL，例如 `TKCode.Arc.Graphics3D.WebGpu` 的 WebGPU 后端（它需要 wgpu-native 库，由你的应用自带）。设备在离屏表面上绘制，视口把画面作为位图显示，所以每一帧多一次读回；`Presentation` 的值是 `Bitmap`。请在视口加入窗口之前设置。设备由视口负责释放。

```csharp
Viewport.DeviceFactory = () => WgpuDevice.Create(pathOfWgpuNative);
```

### 后处理

`PostProcessing` 保存作用在成品画面上的效果：`AmbientOcclusion`（缝隙和接触处变暗）、`DepthOfField`（离开对焦距离后变模糊）、`Outline`（给选定物体描边）、`Bloom`（明亮部分周围的泛光）、`Vignette`（四角变暗）、`Saturation`、`Contrast` 和 `Fxaa`（对最终画面做抗锯齿）。它们默认全部关闭，全部关闭时没有任何开销。变化不会被观察到：改完设置之后调用 `RequestRender()`。场景的背景色在后处理下保持原来的颜色。

`PostProcessing.Passes` 接受你自己的全屏通道。`PostPass` 像节点材质一样由着色节点构成：`PostPass.Input(uv)` 是到目前为止的画面，`PostPass.Uv` 是屏幕上的位置。通道在景深之后、泛光之前执行，处理的是线性值。

```csharp
// 把红、蓝通道向两侧错开。
var amount = Sl.Uniform(0.004f);
Viewport.PostProcessing.Passes.Add(new PostPass(Sl.Vec3(
    PostPass.Input(PostPass.Uv + Sl.Vec2(amount, 0f)).X,
    PostPass.Input().Y,
    PostPass.Input(PostPass.Uv - Sl.Vec2(amount, 0f)).Z)));
```

泛光让比白色更亮的表面发光，例如 `EmissiveIntensity` 大于 1 的自发光材质：

```csharp
var lamp = new Mesh(new SphereGeometry(0.3f, 32, 16), new StandardMaterial
{
    Color = ColorF.Black,
    Emissive = ColorF.FromHex(0xffb454),
    EmissiveIntensity = 4f,
});
scene.Add(lamp);

Viewport.PostProcessing.Bloom.Enabled = true;
Viewport.PostProcessing.Bloom.Strength = 0.8f;
Viewport.PostProcessing.Vignette.Enabled = true;
Viewport.RequestRender();
```

| 设置 | 默认值 | 作用 |
|---|---|---|
| `Bloom.Enabled` | `false` | 开启泛光。 |
| `Bloom.Strength` | `0.6` | 加入多少泛光。 |
| `Bloom.Radius` | `0.7` | 泛光扩散多远，0 到 1。 |
| `Bloom.Threshold` | `1` | 场景亮度超过这个值的像素才发光。 |
| `Bloom.Knee` | `0.5` | 阈值的柔和程度。 |
| `Bloom.Levels` | `6` | 模糊的层数，3 到 8。 |
| `Vignette.Enabled` | `false` | 开启四角变暗。 |
| `Vignette.Intensity` | `0.35` | 四角有多暗，0 到 1。 |
| `Vignette.Smoothness` | `0.5` | 变暗的过渡有多平缓，0 到 1。 |
| `AmbientOcclusion.Enabled` | `false` | 开启缝隙变暗。 |
| `AmbientOcclusion.Radius` | `0.5` | 表面之间相互遮蔽的距离，世界单位。 |
| `AmbientOcclusion.Intensity` | `1` | 被遮蔽的区域有多暗。 |
| `AmbientOcclusion.Samples` | `16` | 每个像素的采样数，8 到 32。 |
| `DepthOfField.Enabled` | `false` | 开启对焦模糊。 |
| `DepthOfField.FocusDistance` | `5` | 离相机多远处是清晰的，世界单位。 |
| `DepthOfField.FocusRange` | `1` | 这个距离前后清晰区域的深度。 |
| `DepthOfField.Falloff` | `4` | 模糊从零增加到最大所经过的距离。 |
| `DepthOfField.MaxBlur` | `8` | 最大模糊半径，像素。 |
| `Outline.Objects` | 空 | 要描边的物体，每个都包含它下面的全部子物体。列表里有内容时显示描边。 |
| `Outline.Color` | `#3b82f6` | 描边的颜色。 |
| `Outline.Thickness` | `2` | 线宽，像素。 |
| `Fxaa` | `false` | 快速近似抗锯齿。 |
| `Saturation` | `1` | 色彩饱和度；0 为灰度。 |
| `Contrast` | `1` | 以中灰为中心的对比度。 |

### 舞台与画面

视口默认是透明的，所以它后面的表面会透出来。`StageBrush` 接受一个纯色画刷作为不透明的背景；`{arc:Viewport3DTokenResource StageColor}` 是主题的舞台色。场景的 `Background` 优先于这两者。`ToneMapping`（默认 `Neutral`，保持基础色不失真）和 `Exposure` 决定明亮的数值如何显示到屏幕上，`Samples` 设置多重采样抗锯齿（0 使用主题的值，1 关闭），`Shadows="False"` 关闭所有阴影。

```xml
<arc:Viewport3D Height="360"
                StageBrush="{arc:Viewport3DTokenResource StageColor}"
                ToneMapping="AcesFilmic"
                Exposure="1.2" />
```

## API 参考

<!-- api: Viewport3D -->

<!-- api: Viewport3DFrameEventArgs -->

## 无障碍

- 3D 画面里没有屏幕阅读器能读的文字。用 `AutomationProperties.Name` 为视口命名，并把它展示的信息（数值、状态、选中项）同时用普通控件给出。
- 视口可以获得键盘焦点。设置了 `CameraControls` 时，方向键平移，Shift 加方向键环绕；缩放没有按键，缩放很重要时请提供你自己的按钮。
- 不要只靠颜色或光照来表示场景里的状态。
- 持续超过五秒的动画需要有停止它的方式（WCAG 2.2.2），预览里的开关就是这样做的。

## 动效

- 视口本身不添加任何动效：它只绘制你的代码要求的帧。
- 装饰性的动画只在完整动效下开始（`ArcMotion.IsFull`），节点材质示例就是这样做的；减弱动效时绘制静止的画面。
- `OrbitControls.EnableDamping` 让相机在拖动结束后滑行一段再停下。想让相机立刻停住就不要开启它。

## 响应式

- 视口采用布局给它的尺寸，没有首选尺寸。给它一个高度，或者放进 `Grid` 的单元格。
- 画面按显示器的像素密度绘制，`AutoAspect` 让透视相机跟随视口的形状。
- 在触摸屏上，单指环绕，双指缩放和平移。设置了 `CameraControls` 且允许缩放时，滚轮缩放场景而不是滚动页面。

## 相关组件

- `ModelViewer`：单个模型，灯光、地面阴影、相机控制和工具栏都已配好。
- `VideoSurface`：另一种绘制在合成树内部的画面，用于视频。
