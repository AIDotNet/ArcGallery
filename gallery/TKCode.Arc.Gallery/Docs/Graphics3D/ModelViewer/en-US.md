---
title: Model viewer
summary: "Show a glTF model, or one built in code, on a lit stage with a ground shadow: orbit, zoom and pan it, let it turn slowly, and reset the view."
---

<Demo name="Preview" />

The bundled GLB includes Dafeiyu's textures and three animations. The preview plays Idle by default. Choose Swim to see the tail, hair ornament and body move as she floats, Wave to see her raise a hand, or Still to inspect the rest pose. The animation stops when the preview scrolls out of view or motion is reduced. Drag to orbit, scroll to zoom, or use the toolbar to auto-rotate and reset the view.

## When to use

- Product pages, asset browsers and previews of generated or uploaded models: one object that people turn and inspect.
- A glTF 2.0 file (`.gltf` or `.glb`) that should look right without setting up lights, a camera or a floor.
- An object built in code with the engine that needs the same stage.

## When not to use

- A scene with several objects, your own lights or cameras, or animation driven by your code: use `Viewport3D`.
- Files with WebP textures: they are not supported yet. Draco and meshopt mesh compression and KTX2 (Basis) textures are.
- A picture of the product is enough: use `Image`, which costs far less.

## Usage

`ModelViewer` is in `TKCode.Arc.Graphics3D.Controls`, which brings the engine `TKCode.Arc.Graphics3D` with it. Register the package inside `UseArc(...)`:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

Both packages are previews with their own version line (0.1.0-preview.1), and the API may change from one preview to the next. The viewer draws through a `Viewport3D`, so the same platform notes apply: OpenGL inside the Avalonia composition tree, verified on macOS (under Avalonia's default renderer), on Windows with an NVIDIA card and on Linux with Mesa's software OpenGL, and a slower bitmap fallback under software rendering. In the browser the viewer draws in a WebGL 2 context of the page once the browser project has enabled `TKCode.Arc.Graphics3D.Browser` with `UseArcGraphics3DBrowser()` (verified in headless Chromium with software WebGL); without it the viewer shows its stage but no model.

The viewer frames whatever it is given: it puts the ground under the model, aims the light, and places the camera so that the whole model is in view. Every part of the model casts and receives shadows.

### From a file

`Source` takes the path of a glTF 2.0 file (`.gltf` or `.glb`), an `avares://` resource of the app, or an `http://` or `https://` address; buffers and images next to a `.gltf` are fetched from the same place. The model is read off the UI thread; `IsLoading` is true and a spinner shows meanwhile. A model from the web is downloaded each time `Source` is set: the viewer keeps no cache. `LoadAsync` reads a stream instead, for example a file the user picked. A `.gltf` loaded from a stream cannot reach its external files: use a `.glb` or data URIs.

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

`ModelLoaded` is raised when the model is on screen. When loading fails, `LoadFailed` is raised and the viewer shows `ErrorMessage` in place of the model. Files with WebP textures fail this way: they are not supported yet. Files with Draco or meshopt mesh compression and with KTX2 (Basis) textures load.

A file's animations are listed in `Animations`. `PlayAnimation(index)` plays one in a loop and `StopAnimation()` returns the model to its rest pose.

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

### From code

`Model` shows any object built with the engine in place of a file: a single mesh or a whole group. The viewer does not dispose an object it was given.

```csharp
using TKCode.Arc.Graphics3D;

Viewer.Model = new Mesh(
    new TorusKnotGeometry(0.6f, 0.2f, 160, 24),
    new PhysicalMaterial { Color = ColorF.FromHex(0xc2410c), Roughness = 0.35f, Clearcoat = 1f });
```

Numbers carry over from three.js: the same units, the same light intensities and the same camera conventions (right-handed, Y up, the field of view in degrees).

<Demo name="FromCode" />

### Lighting, rotation and shadow

`Environment` chooses the surroundings that light the model and show in its reflections: `Studio` (the default, soft boxes in a gray room; colors stay true), `Outdoor` (a clear sky with a warm sun) or `None` (only the viewer's two lights). The environments are computed, so no image files ship with your app. `EnvironmentIntensity` multiplies their light, `Exposure` and `ToneMapping` decide how bright values reach the screen, and `ShadowIntensity` sets the darkness of the shadow on the ground from 0 (none) to 1.

`AutoRotate` turns the model slowly about its vertical axis while nobody is dragging it, at `AutoRotateSpeed` degrees per second (12 by default: one turn in 30 seconds). It turns only at full motion.

<Demo name="Lighting" />

### Camera and toolbar

People orbit by dragging, zoom with the wheel and pan with the right button; on a touch screen one finger orbits and two fingers zoom and pan. The two buttons in the corner switch auto-rotation and put the camera back where it framed the whole model; `ResetView()` does the same from code. `CameraControlsEnabled="False"` fixes the camera and `ShowToolbar="False"` hides the buttons. The buttons' names follow the app's language (en-US, zh-CN and zh-TW are built in); set `ResetViewLabel` and `AutoRotateLabel` to use your own wording.

```xml
<arc:ModelViewer Height="320"
                 CameraControlsEnabled="False"
                 ShowToolbar="False"
                 Environment="Outdoor"
                 ShadowIntensity="0.5" />
```

`CameraControls`, `Camera` and `Scene` give the viewer's own orbit controls, camera and scene, to limit the orbit or to add lights and objects next to the model. `PostProcessing` gives the effects of the inner viewport (ambient occlusion, depth of field, outlines, bloom, vignette, saturation, contrast, FXAA; see `Viewport3D`); it is null until the viewer's template is applied.

```csharp
Viewer.CameraControls.MinPolarAngle = MathF.PI / 6f;
Viewer.CameraControls.MaxPolarAngle = MathF.PI / 2f;
Viewer.Scene.Add(new PointLight(ColorF.FromHex(0xffd9a0), 8f) { Position = new Vector3(1f, 2f, 1f) });
```

## API reference

<!-- api: ModelViewer -->

<!-- api: ModelLoadFailedEventArgs -->

## Accessibility

- The model has no text a screen reader can read. Name the viewer with `AutomationProperties.Name` and describe the product in text next to it.
- The two buttons have accessible names and tooltips in the app's language; `ResetViewLabel` and `AutoRotateLabel` replace them.
- The picture takes keyboard focus: the arrow keys pan and Shift with an arrow key orbits. Zoom has no key.
- A turning model is moving content: the auto-rotate button lets people stop it (WCAG 2.2.2), so keep the toolbar when `AutoRotate` is on.

## Motion

- Auto-rotation is decoration. It runs only at full motion; with reduced motion the model stands still, and people can still turn it by hand.
- The camera glides to a stop after a drag. Set `CameraControls.EnableDamping` to false for a camera that stops at once.
- The viewer draws on demand: a still model draws no frames. It keeps drawing while the camera moves, while auto-rotation runs and while an animation plays.

## Responsive behavior

- The viewer fills the width it is given and has a minimum height of 160. Give it a height or an aspect ratio of your own.
- The model stays framed when the viewer changes shape: the camera's aspect ratio follows the viewer.
- On touch screens, one finger orbits and two fingers zoom and pan. The wheel zooms the model instead of scrolling the page while the pointer is over it; set `CameraControlsEnabled="False"` for a viewer inside a long page that should not take the wheel.

## Related

- `Viewport3D`: the bare viewport for a scene, lights and cameras of your own.
- `Image`: a still picture of the product.
