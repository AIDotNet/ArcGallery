---
title: 3D viewport
summary: "Draw a 3D scene of your own inside the window: meshes, physically based materials, lights and shadows, drawn only when something changes, with other controls on top."
---

<Demo name="Preview" />

## When to use

- A scene of your own: several objects, your own lights and cameras, materials written as node graphs, animation driven by your code.
- 3D charts, digital twins, configurators and previews of generated models that sit inside an ordinary layout, with Arc controls over or beside the picture.
- Pictures that mostly stand still. A scene that does not change draws no frames.

## When not to use

- One model to look at from all sides: use `ModelViewer`, which adds the lighting, the ground shadow, the camera controls and a toolbar.
- A browser app whose scene changes on every frame: there each frame is read back from the graphics card, which costs more than on the desktop (see Platforms and preview status).
- A fixed, decorative 3D-looking picture: the Revenue globe and Exploded view blocks draw theirs in 2D and need no graphics context.
- Code that needs a stable API: both 3D packages are previews.

## Usage

Two packages make up the 3D support. `TKCode.Arc.Graphics3D` is the engine (scene graph, geometries, materials, lights, animation, the glTF loader and the renderer); it has no UI dependency and needs no registration. `TKCode.Arc.Graphics3D.Controls` holds `Viewport3D` and `ModelViewer` and brings the engine with it. Register the controls inside `UseArc(...)`:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

The viewport has no size of its own and no chrome: give it a height or a cell of a `Grid`.

```xml
<arc:Viewport3D x:Name="Viewport" Height="360" />
```

A scene is built in code from the types of the `TKCode.Arc.Graphics3D` namespace. Numbers carry over from three.js: the same units (meters), the same light intensities and the same camera conventions (right-handed, Y up, the field of view in degrees).

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

`AutoAspect` (on by default) keeps a perspective camera's aspect ratio equal to the viewport's, so the value passed to the camera does not matter. Geometries, materials and textures hold graphics memory until you call their `Dispose()` or the viewport leaves the window.

### Platforms and preview status

The viewport renders with OpenGL inside the Avalonia composition tree. Other controls can sit on top of it, and it clips, fades and transforms like any control. On macOS it works under Avalonia's default renderer, with no change to the app's settings.

- Both packages are previews with their own version line (0.1.0-preview.1). The API may change from one preview to the next.
- The graphics driver must offer OpenGL 3.3 or OpenGL ES 3.0 (on Windows Avalonia brings ANGLE, which does); a browser must offer WebGL 2.
- They have been verified on macOS, on Windows (one machine with an NVIDIA card: Avalonia's default renderer, its software mode and the graphics driver's own OpenGL), on Linux under X11 with Mesa's software OpenGL, and in the browser in headless Chromium with software WebGL. Linux with real GPU drivers, Wayland, Windows on other graphics hardware, and browsers on real graphics hardware (Firefox and Safari among them) are untested.
- On Linux, Avalonia does not render with a software OpenGL driver (llvmpipe) unless the app clears `X11PlatformOptions.GlxRendererBlacklist`; it composites in software instead, and the viewport uses the bitmap fallback described next.
- When Avalonia renders in software, the viewport draws in an OpenGL context of its own and shows the result as a bitmap. That costs one read-back per drawn frame, and `Presentation` then reads `Bitmap`.
- In the browser (WebAssembly) the viewport draws in a WebGL 2 context of the page and shows the result the same way, as a bitmap: Avalonia's browser renderer shares neither its WebGL context nor its textures. The app's browser project adds the `TKCode.Arc.Graphics3D.Browser` package and enables it (below). The read-back makes a frame cost more than on the desktop, so draw on demand where you can.
- There is no picture on a machine with no OpenGL at all, in a browser without WebGL 2, and in a browser app that has not enabled the browser package. `IsRendererReady` stays false and `RendererError` says why; show a message or a still image of your own in that case.

```xml
<Grid Height="360">
    <arc:Viewport3D x:Name="Viewport" />
    <TextBlock Text="{Binding #Viewport.RendererError}"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
</Grid>
```

In a browser app, reference `TKCode.Arc.Graphics3D.Browser` from the browser project and add its entry next to the others:

```csharp
// Program.cs of the browser project
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D().UseArcGraphics3DBrowser());
```

The project file needs nothing more: the package uses the OpenGL ES layer that Avalonia.Browser already links into the app. A viewport that is on screen before the package's script has loaded starts drawing when it arrives.

### Drawing on demand

`RenderMode` is `OnDemand` by default. The viewport draws a frame when:

- the scene graph changes: objects are added, removed, moved, shown or hidden;
- the camera controls move;
- the viewport is resized or one of its properties changes;
- you call `RequestRender()`. It is cheap, and any number of calls before the next frame draw one frame.

Changes to materials, textures and light colors are not observed: call `RequestRender()` after them.

```csharp
material.Color = ColorF.FromHex(0x30a46c);
Viewport.RequestRender();
```

Animate in `FrameRendering`, which is raised on the UI thread before each frame, and set `RequestNextFrame` to true while the animation runs. Its arguments (`Viewport3DFrameEventArgs`) carry `Elapsed`, the time since the previous frame (zero for the first frame after a pause), `RequestNextFrame`, and `Renderer`, the renderer that draws the frame. The Preview at the top of this page spins its box this way.

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

`RenderMode="Continuous"` draws on every display frame while the viewport is visible; keep it for scenes that never rest. `Statistics` reports what the last frame cost (objects, draw calls, triangles, CPU time), and `Capture` hands a frame's pixels to a callback: `Viewport.Capture(image => File.WriteAllBytes("scene.png", Png.Encode(image)))`.

### Camera controls

Set `CameraControls` to an `OrbitControls` to let people move the camera: drag to orbit, scroll to zoom, and drag with the right button to pan. On a touch screen one finger orbits and two fingers zoom and pan. The arrow keys pan or, with Shift, orbit. The viewport keeps drawing while the controls are still gliding to a stop. Without `CameraControls` the viewport does not react to input.

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

### Node materials

A material describes a surface; the engine adds the lighting, shadows and tone mapping. Every built-in material takes nodes in place of its values (`ColorNode`, `RoughnessNode`, `OpacityNode`, `PositionNode` and others), and `NodeMaterial` is defined by nodes alone. Nodes are built with the functions of `Sl` in `TKCode.Arc.Graphics3D.Shading`, not written as shader text. `Sl.Uniform` gives a value you can change later without rebuilding the shader, and `Sl.Time` is the renderer's clock: it moves only while frames are drawn.

<Demo name="NodeMaterials" />

### Render thread

`UseRenderThread="True"` moves the graphics work to a thread of the viewport's own: the UI thread prepares each frame (animation, culling, sorting) and the render thread talks to the graphics driver. It pays off in scenes with thousands of separate draws, where the driver's share of a frame is large; for a single model it changes little. It works where the viewport owns its OpenGL context (on macOS, and under software rendering); where the context belongs to Avalonia's renderer (Windows and Linux with GPU rendering), and in the browser, where a page has one thread, the setting has no effect. Set it before the viewport joins a window; `IsRenderThreadActive` says whether it is in use.

### Another graphics device

`DeviceFactory` replaces OpenGL with a device you create, such as the WebGPU backend of `TKCode.Arc.Graphics3D.WebGpu` (it needs the wgpu-native library, which your app ships). The device draws off screen and the viewport shows the picture as a bitmap, so each frame costs a read-back; `Presentation` reads `Bitmap`. Set it before the viewport joins a window. The viewport disposes the device.

```csharp
Viewport.DeviceFactory = () => WgpuDevice.Create(pathOfWgpuNative);
```

### Post-processing

`PostProcessing` holds effects applied to the finished picture: `AmbientOcclusion` (darker creases and contact areas), `DepthOfField` (blur away from a focus distance), `Outline` (a line around chosen objects), `Bloom` (glow around bright parts), `Vignette` (darker corners), `Saturation`, `Contrast` and `Fxaa` (anti-aliasing of the final picture). All are off by default, and while all are off they cost nothing. Changes are not observed: call `RequestRender()` after changing a setting. A scene background color keeps its exact color under post-processing.

`PostProcessing.Passes` takes full-screen passes of your own. A `PostPass` is built from shader nodes like a node material: `PostPass.Input(uv)` is the picture so far and `PostPass.Uv` the position on screen. Passes run after depth of field and before bloom, on linear values.

```csharp
// Split the red and blue channels sideways.
var amount = Sl.Uniform(0.004f);
Viewport.PostProcessing.Passes.Add(new PostPass(Sl.Vec3(
    PostPass.Input(PostPass.Uv + Sl.Vec2(amount, 0f)).X,
    PostPass.Input().Y,
    PostPass.Input(PostPass.Uv - Sl.Vec2(amount, 0f)).Z)));
```

Bloom makes surfaces glow that are brighter than white, such as an emissive material with `EmissiveIntensity` above 1:

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

| Setting | Default | What it does |
|---|---|---|
| `Bloom.Enabled` | `false` | Turns the glow on. |
| `Bloom.Strength` | `0.6` | How much glow is added. |
| `Bloom.Radius` | `0.7` | How far the glow spreads, 0 to 1. |
| `Bloom.Threshold` | `1` | Scene brightness above which a pixel glows. |
| `Bloom.Knee` | `0.5` | Softness of the threshold. |
| `Bloom.Levels` | `6` | Number of blur levels, 3 to 8. |
| `Vignette.Enabled` | `false` | Turns the darker corners on. |
| `Vignette.Intensity` | `0.35` | How dark the corners get, 0 to 1. |
| `Vignette.Smoothness` | `0.5` | How gradually the darkening starts, 0 to 1. |
| `AmbientOcclusion.Enabled` | `false` | Turns the darker creases on. |
| `AmbientOcclusion.Radius` | `0.5` | How far surfaces shade each other, in world units. |
| `AmbientOcclusion.Intensity` | `1` | How dark the shaded areas get. |
| `AmbientOcclusion.Samples` | `16` | Samples per pixel, 8 to 32. |
| `DepthOfField.Enabled` | `false` | Turns the focus blur on. |
| `DepthOfField.FocusDistance` | `5` | Distance from the camera that is sharp, in world units. |
| `DepthOfField.FocusRange` | `1` | Depth of the sharp zone around that distance. |
| `DepthOfField.Falloff` | `4` | Distance over which the blur grows to its maximum. |
| `DepthOfField.MaxBlur` | `8` | Largest blur radius in pixels. |
| `Outline.Objects` | empty | The objects to outline, each with everything below it. The line shows while the list has entries. |
| `Outline.Color` | `#3b82f6` | The line color. |
| `Outline.Thickness` | `2` | Line width in pixels. |
| `Fxaa` | `false` | Fast approximate anti-aliasing. |
| `Saturation` | `1` | Color saturation; 0 is grayscale. |
| `Contrast` | `1` | Contrast about middle gray. |

### Stage and picture

The viewport is transparent by default, so the surface behind it shows through. `StageBrush` takes a solid color brush for an opaque backdrop; `{arc:Viewport3DTokenResource StageColor}` is the theme's stage color. A scene `Background` takes precedence over both. `ToneMapping` (`Neutral` by default, which keeps base colors true) and `Exposure` decide how bright values reach the screen, `Samples` sets multisample anti-aliasing (0 uses the theme's value, 1 turns it off), and `Shadows="False"` turns all shadows off.

```xml
<arc:Viewport3D Height="360"
                StageBrush="{arc:Viewport3DTokenResource StageColor}"
                ToneMapping="AcesFilmic"
                Exposure="1.2" />
```

## API reference

<!-- api: Viewport3D -->

<!-- api: Viewport3DFrameEventArgs -->

## Accessibility

- A 3D picture has no text a screen reader can read. Name the viewport with `AutomationProperties.Name`, and give the facts it shows (values, states, selections) in ordinary controls as well.
- The viewport takes keyboard focus. With `CameraControls`, the arrow keys pan and Shift with an arrow key orbits; zoom has no key, so offer buttons of your own when zooming matters.
- Do not rely on color or lighting alone to mark a state in the scene.
- Animation that runs for more than five seconds needs a way to stop it (WCAG 2.2.2), as the switch in the Preview does.

## Motion

- The viewport adds no motion of its own: it draws the frames your code asks for.
- Start decorative animation only at full motion (`ArcMotion.IsFull`), as the node-material demo does; at reduced motion draw a still picture.
- `OrbitControls.EnableDamping` lets the camera glide to a stop after a drag. Leave it off for a camera that stops at once.

## Responsive behavior

- The viewport takes the size its layout gives it and has no preferred size. Give it a height, or place it in a `Grid` cell.
- The picture is drawn at the display's pixel density, and `AutoAspect` keeps a perspective camera in step with the viewport's shape.
- On touch screens, one finger orbits and two fingers zoom and pan. While `CameraControls` is set with zoom enabled, the wheel zooms the scene instead of scrolling the page.

## Related

- `ModelViewer`: one model with lighting, a ground shadow, camera controls and a toolbar already in place.
- `VideoSurface`: another picture drawn inside the composition tree, for video.
