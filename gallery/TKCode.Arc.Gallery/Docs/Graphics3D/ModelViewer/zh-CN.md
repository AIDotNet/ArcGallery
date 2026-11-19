---
title: 模型查看器
summary: "在带地面阴影的布光舞台上展示 glTF 模型或用代码搭建的模型：可以环绕、缩放和平移，让它缓慢转动，并重置视图。"
source-hash: bf441c3ba1c2
---

<Demo name="Preview" />

预览从应用内置的 GLB 加载大肥鱼，纹理也包含在文件中。拖动角色可旋转，滚动可缩放，也可用工具栏自动旋转或重置视角。

## 何时使用

- 产品页、素材浏览器，以及生成或上传的模型的预览：让用户转动并查看的单个物体。
- 一个 glTF 2.0 文件（`.gltf` 或 `.glb`），不想自己配置灯光、相机和地面就能有正确的效果。
- 用引擎在代码里搭建、需要同样舞台的物体。

## 何时不用

- 有多个物体、自己的灯光或相机，或者由你的代码驱动动画的场景：用 `Viewport3D`。
- 带 WebP 纹理的文件：目前还不支持。Draco、meshopt 网格压缩和 KTX2（Basis）纹理已支持。
- 一张产品图片就够了：用 `Image`，开销小得多。

## 用法

`ModelViewer` 在 `TKCode.Arc.Graphics3D.Controls` 包里，这个包会带上引擎 `TKCode.Arc.Graphics3D`。在 `UseArc(...)` 里注册它：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

两个包都是预览版，有自己的版本线（0.1.0-preview.1），API 可能在两个预览版之间变化。查看器通过一个 `Viewport3D` 绘制，所以平台方面的说明相同：在 Avalonia 的合成树内部用 OpenGL 渲染，目前在 macOS（使用 Avalonia 的默认渲染器）、Windows（NVIDIA 显卡）和 Linux（Mesa 的软件 OpenGL）上验证过，软件渲染下走较慢的位图退路。在浏览器里，浏览器项目用 `UseArcGraphics3DBrowser()` 启用 `TKCode.Arc.Graphics3D.Browser` 之后，查看器在页面的一个 WebGL 2 上下文里绘制（在无头 Chromium 和软件 WebGL 下验证过）；没有启用时查看器只显示舞台，没有模型。

查看器会自己为拿到的内容取景：把地面放到模型下方，对准灯光，并把相机放在能看到整个模型的位置。模型的每个部分都会投射并接收阴影。

### 来自文件

`Source` 接受 glTF 2.0 文件（`.gltf` 或 `.glb`）的路径、应用的 `avares://` 资源，或 `http://`、`https://` 地址；`.gltf` 旁边的缓冲区和图片从同一个位置获取。模型在 UI 线程之外读取；读取期间 `IsLoading` 为 true，并显示加载指示器。来自网络的模型在每次设置 `Source` 时都会重新下载：查看器不做缓存。`LoadAsync` 则从流读取，例如用户选择的文件。从流加载的 `.gltf` 找不到它的外部文件：请用 `.glb` 或 data URI。

```xml
<arc:ModelViewer x:Name="Viewer" Height="360"
                 Source="avares://MyApp/Assets/Dafeiyu.glb"
                 ToneMapping="None" />
```

```csharp
Viewer.Source = Path.Combine(AppContext.BaseDirectory, "Assets", "chair.glb");

// Or from a stream:
await using var stream = AssetLoader.Open(new Uri("avares://MyApp/Assets/chair.glb"));
await Viewer.LoadAsync(stream);
```

模型显示到屏幕上时触发 `ModelLoaded`。加载失败时触发 `LoadFailed`，查看器在模型的位置显示 `ErrorMessage`。带 WebP 纹理的文件就会这样失败：目前还不支持。用 Draco 或 meshopt 压缩网格、用 KTX2（Basis）纹理的文件可以加载。

文件里的动画列在 `Animations` 里。`PlayAnimation(index)` 循环播放其中一个，`StopAnimation()` 让模型回到静止姿态。

```csharp
Viewer.ModelLoaded += (_, _) =>
{
    if (Viewer.Animations.Count > 0)
    {
        Viewer.PlayAnimation(0);
    }
};
Viewer.LoadFailed += (_, e) => Log(e.Exception.Message);
```

### 来自代码

`Model` 显示任何用引擎搭建的物体来代替文件：单个网格或一整个组。查看器不会释放交给它的物体。

```csharp
using TKCode.Arc.Graphics3D;

Viewer.Model = new Mesh(
    new TorusKnotGeometry(0.6f, 0.2f, 160, 24),
    new PhysicalMaterial { Color = ColorF.FromHex(0xc2410c), Roughness = 0.35f, Clearcoat = 1f });
```

three.js 里的数值可以直接搬过来：单位相同，灯光强度相同，相机约定也相同（右手坐标系、Y 轴向上、视野用角度）。

<Demo name="FromCode" />

### 灯光、旋转与阴影

`Environment` 选择照亮模型并出现在它反射里的环境：`Studio`（默认，灰色房间里的柔光箱；颜色不失真）、`Outdoor`（晴朗的天空和暖色的太阳）或 `None`（只有查看器自带的两盏灯）。这些环境是计算出来的，所以应用不需要附带任何图片文件。`EnvironmentIntensity` 是环境光的倍数，`Exposure` 和 `ToneMapping` 决定明亮的数值如何显示到屏幕上，`ShadowIntensity` 设置地面阴影的深浅，从 0（没有）到 1。

`AutoRotate` 在没有人拖动时让模型绕竖直轴缓慢转动，速度是每秒 `AutoRotateSpeed` 度（默认 12：30 秒转一圈）。它只在完整动效下转动。

<Demo name="Lighting" />

### 相机与工具栏

用户拖动来环绕，用滚轮缩放，用右键平移；在触摸屏上，单指环绕，双指缩放和平移。角落里的两个按钮分别切换自动旋转，以及把相机放回能看到整个模型的位置；`ResetView()` 在代码里做同样的事。`CameraControlsEnabled="False"` 固定相机，`ShowToolbar="False"` 隐藏按钮。按钮的名称跟随应用的语言（内置 en-US、zh-CN 和 zh-TW）；想用自己的措辞时设置 `ResetViewLabel` 和 `AutoRotateLabel`。

```xml
<arc:ModelViewer Height="320"
                 CameraControlsEnabled="False"
                 ShowToolbar="False"
                 Environment="Outdoor"
                 ShadowIntensity="0.5" />
```

`CameraControls`、`Camera` 和 `Scene` 给出查看器自己的环绕控制器、相机和场景，用来限制环绕范围，或者在模型旁边添加灯光和物体。`PostProcessing` 给出内部视口的效果（环境光遮蔽、景深、描边、泛光、暗角、饱和度、对比度、FXAA；见 `Viewport3D`）；在查看器的模板应用之前它是 null。

```csharp
Viewer.CameraControls.MinPolarAngle = MathF.PI / 6f;
Viewer.CameraControls.MaxPolarAngle = MathF.PI / 2f;
Viewer.Scene.Add(new PointLight(ColorF.FromHex(0xffd9a0), 8f) { Position = new Vector3(1f, 2f, 1f) });
```

## API 参考

<!-- api: ModelViewer -->

<!-- api: ModelLoadFailedEventArgs -->

## 无障碍

- 模型里没有屏幕阅读器能读的文字。用 `AutomationProperties.Name` 为查看器命名，并在旁边用文字描述产品。
- 两个按钮的无障碍名称和工具提示使用应用的语言；`ResetViewLabel` 和 `AutoRotateLabel` 可以替换它们。
- 画面可以获得键盘焦点：方向键平移，Shift 加方向键环绕。缩放没有按键。
- 转动的模型属于运动的内容：自动旋转按钮让用户可以停止它（WCAG 2.2.2），所以开启 `AutoRotate` 时请保留工具栏。

## 动效

- 自动旋转是装饰。它只在完整动效下运行；减弱动效时模型静止不动，用户仍然可以手动转动它。
- 拖动结束后相机会滑行一段再停下。想让相机立刻停住，把 `CameraControls.EnableDamping` 设为 false。
- 查看器按需绘制：静止的模型不绘制任何帧。相机移动时、自动旋转运行时和动画播放时，它才持续绘制。

## 响应式

- 查看器填满给它的宽度，最小高度为 160。请给它一个高度，或者你自己的宽高比。
- 查看器改变形状时模型保持在取景内：相机的宽高比跟随查看器。
- 在触摸屏上，单指环绕，双指缩放和平移。指针在模型上方时，滚轮缩放模型而不是滚动页面；放在长页面里、不应占用滚轮的查看器请设置 `CameraControlsEnabled="False"`。

## 相关组件

- `Viewport3D`：只有视口，用于你自己的场景、灯光和相机。
- `Image`：产品的静态图片。
