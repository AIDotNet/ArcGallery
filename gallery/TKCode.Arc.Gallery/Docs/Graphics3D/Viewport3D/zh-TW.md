---
title: 3D 視埠
summary: "在視窗裡繪製你自己的 3D 場景：網格、物理式材質、燈光和陰影，只在有變動時才繪製，其他控制項可以疊在上面。"
source-hash: 17465bfc1214
---

<Demo name="Preview" />

## 何時使用

- 你自己的場景：多個物件、自己的燈光和攝影機、用節點圖撰寫的材質、由你的程式碼驅動的動畫。
- 放在一般版面裡的 3D 圖表、數位分身、設定器和生成模型的預覽，Arc 控制項可以疊在畫面上方或放在旁邊。
- 大部分時間靜止的畫面。場景沒有變動時不會繪製任何影格。

## 何時不用

- 只需要從各個角度檢視一個模型：用 `ModelViewer`，它已經備妥燈光、地面陰影、攝影機控制和工具列。
- 瀏覽器應用程式裡每個影格都在變動的場景：在那裡每個影格都要從顯示卡讀回像素，負擔比桌面上大（見「平台與預覽狀態」）。
- 固定的、裝飾性的立體畫面：營收地球儀和分解檢視區塊以 2D 繪製，不需要圖形內容。
- 需要穩定 API 的程式碼：兩個 3D 套件都是預覽版。

## 用法

3D 支援由兩個套件組成。`TKCode.Arc.Graphics3D` 是引擎（場景圖、幾何體、材質、燈光、動畫、glTF 載入器和算繪器），不相依任何介面程式庫，也不需要註冊。`TKCode.Arc.Graphics3D.Controls` 包含 `Viewport3D` 和 `ModelViewer`，並會一併帶入引擎。在 `UseArc(...)` 裡註冊這兩個控制項：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

視埠沒有自己的尺寸，也沒有外框：給它一個高度，或者放進 `Grid` 的儲存格。

```xml
<arc:Viewport3D x:Name="Viewport" Height="360" />
```

場景在程式碼裡用 `TKCode.Arc.Graphics3D` 命名空間的型別建立。three.js 裡的數值可以直接沿用：單位相同（公尺），燈光強度相同，攝影機慣例也相同（右手座標系、Y 軸朝上、視野以角度表示）。

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

`AutoAspect`（預設開啟）讓透視攝影機的長寬比始終等於視埠的長寬比，所以傳給攝影機的那個值並不重要。幾何體、材質和紋理會一直佔用顯示記憶體，直到你呼叫它們的 `Dispose()`，或者視埠離開視窗。

### 平台與預覽狀態

視埠在 Avalonia 的合成樹內部以 OpenGL 算繪。其他控制項可以疊在它上面，它也和其他控制項一樣可以被裁剪、淡入淡出和變換。在 macOS 上，它在 Avalonia 的預設算繪器下就能運作，不需要更改應用程式的設定。

- 兩個套件都是預覽版，有自己的版本線（0.1.0-preview.1）。API 可能在兩個預覽版之間變動。
- 顯示卡驅動程式需要提供 OpenGL 3.3 或 OpenGL ES 3.0（在 Windows 上 Avalonia 內建的 ANGLE 符合這一點）；瀏覽器需要提供 WebGL 2。
- 目前在 macOS 上驗證過，在 Windows 上驗證過（一台 NVIDIA 顯示卡的機器：Avalonia 的預設算繪器、它的軟體模式，以及顯示卡驅動程式自己的 OpenGL），在 Linux 的 X11 下以 Mesa 的軟體 OpenGL 驗證過，也在瀏覽器裡以無頭 Chromium 和軟體 WebGL 驗證過。搭配真實 GPU 驅動程式的 Linux、Wayland、其他顯示卡上的 Windows，以及真實顯示卡上的瀏覽器（包括 Firefox 和 Safari）尚未測試。
- 在 Linux 上，除非應用程式清空 `X11PlatformOptions.GlxRendererBlacklist`，Avalonia 不會用軟體 OpenGL 驅動程式（llvmpipe）算繪，而是改用軟體合成；這時視埠改走下一條所說的點陣圖退路。
- Avalonia 以軟體算繪時，視埠在自己的 OpenGL 內容裡繪製，再把結果當成點陣圖顯示。每繪製一格多一次像素讀回，這時 `Presentation` 的值是 `Bitmap`。
- 在瀏覽器（WebAssembly）裡，視埠在頁面的一個 WebGL 2 內容裡繪製，並以同樣的方式把結果當成點陣圖顯示：Avalonia 的瀏覽器算繪器既不共用它的 WebGL 內容，也不共用它的紋理。應用程式的瀏覽器專案要參考 `TKCode.Arc.Graphics3D.Browser` 套件並啟用它（見下文）。讀回像素讓每個影格比桌面上負擔更大，所以盡量依需求繪製。
- 完全沒有 OpenGL 的機器、沒有 WebGL 2 的瀏覽器，以及沒有啟用瀏覽器套件的瀏覽器應用程式裡沒有畫面。這時 `IsRendererReady` 維持為 false，`RendererError` 會說明原因；請顯示你自己的提示或靜態圖片。

```xml
<Grid Height="360">
    <arc:Viewport3D x:Name="Viewport" />
    <TextBlock Text="{Binding #Viewport.RendererError}"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
</Grid>
```

在瀏覽器應用程式裡，讓瀏覽器專案參考 `TKCode.Arc.Graphics3D.Browser`，並把它的進入點加在其他進入點旁邊：

```csharp
// 瀏覽器專案的 Program.cs
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D().UseArcGraphics3DBrowser());
```

專案檔不需要再加別的東西：這個套件用的是 Avalonia.Browser 已經連結進應用程式的 OpenGL ES 層。套件的指令碼載入完成之前就已經在螢幕上的視埠，會在指令碼到達後開始繪製。

### 依需求繪製

`RenderMode` 預設是 `OnDemand`。視埠在下列情況繪製一個影格：

- 場景圖發生變動：新增、移除、移動、顯示或隱藏物件；
- 攝影機控制正在移動；
- 視埠改變尺寸，或它的某個屬性改變；
- 你呼叫 `RequestRender()`。它的負擔很小，下一個影格之前無論呼叫多少次都只繪製一個影格。

材質、紋理和燈光顏色的變動不會被觀察到：改完之後呼叫 `RequestRender()`。

```csharp
material.Color = ColorF.FromHex(0x30a46c);
Viewport.RequestRender();
```

在 `FrameRendering` 裡製作動畫，它會在每個影格繪製之前於 UI 執行緒上引發；動畫進行期間把 `RequestNextFrame` 設為 true。事件引數（`Viewport3DFrameEventArgs`）帶有 `Elapsed`（距離上一個影格的時間，暫停後的第一個影格為零）、`RequestNextFrame` 和 `Renderer`（繪製這個影格的算繪器）。本頁頂端的預覽就是這樣旋轉方塊的。

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

`RenderMode="Continuous"` 在視埠可見時每個顯示影格都繪製；只用於從不靜止的場景。`Statistics` 回報上一個影格的負擔（物件數、繪製呼叫、三角形、CPU 時間），`Capture` 把一個影格的像素交給回呼：`Viewport.Capture(image => File.WriteAllBytes("scene.png", Png.Encode(image)))`。

### 攝影機控制

把 `CameraControls` 設為一個 `OrbitControls`，就可以讓使用者移動攝影機：拖曳環繞，滾輪縮放，按住右鍵拖曳平移。在觸控螢幕上，單指環繞，雙指縮放和平移。方向鍵平移，按住 Shift 時方向鍵環繞。控制器還在滑行減速時，視埠會繼續繪製。沒有 `CameraControls` 時，視埠不回應輸入。

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

### 節點材質

材質只描述表面；光照、陰影和色調對應由引擎加上。每種內建材質都可以用節點取代它的數值（`ColorNode`、`RoughnessNode`、`OpacityNode`、`PositionNode` 等），`NodeMaterial` 則完全由節點定義。節點用 `TKCode.Arc.Graphics3D.Shading` 裡 `Sl` 的函式建構，不必撰寫著色器文字。`Sl.Uniform` 提供一個之後可以修改而不必重建著色器的值，`Sl.Time` 是算繪器的時鐘：只有在繪製影格的時候它才會前進。

<Demo name="NodeMaterials" />

### 算繪執行緒

`UseRenderThread="True"` 把圖形工作移到視埠自己的執行緒上：UI 執行緒準備每一格（動畫、剔除、排序），算繪執行緒和顯示卡驅動程式打交道。在有上千次獨立繪製的場景裡效益明顯，因為驅動程式佔了一格的大部分；只顯示一個模型時差別不大。它在視埠自己擁有 OpenGL 內容時生效（macOS，以及軟體算繪下）；內容屬於 Avalonia 算繪器時（使用 GPU 算繪的 Windows 和 Linux），以及在只有一個執行緒的瀏覽器頁面裡，設定無效。請在視埠加入視窗之前設定；`IsRenderThreadActive` 說明它是否在使用中。

### 換一個圖形裝置

`DeviceFactory` 以你建立的裝置取代 OpenGL，例如 `TKCode.Arc.Graphics3D.WebGpu` 的 WebGPU 後端（它需要 wgpu-native 程式庫，由你的應用程式自帶）。裝置在離屏表面上繪製，視埠把畫面當成點陣圖顯示，所以每一格多一次讀回；`Presentation` 的值是 `Bitmap`。請在視埠加入視窗之前設定。裝置由視埠負責釋放。

```csharp
Viewport.DeviceFactory = () => WgpuDevice.Create(pathOfWgpuNative);
```

### 後製處理

`PostProcessing` 保存套用在成品畫面上的效果：`AmbientOcclusion`（縫隙和接觸處變暗）、`DepthOfField`（離開對焦距離後變模糊）、`Outline`（替選定物件描邊）、`Bloom`（明亮部分周圍的泛光）、`Vignette`（四角變暗）、`Saturation`、`Contrast` 和 `Fxaa`（對最終畫面做反鋸齒）。它們預設全部關閉，全部關閉時沒有任何負擔。變動不會被觀察到：改完設定之後呼叫 `RequestRender()`。場景的背景色在後製處理下維持原本的顏色。

`PostProcessing.Passes` 接受你自己的全螢幕通道。`PostPass` 像節點材質一樣由著色節點構成：`PostPass.Input(uv)` 是到目前為止的畫面，`PostPass.Uv` 是螢幕上的位置。通道在景深之後、泛光之前執行，處理的是線性值。

```csharp
// 把紅、藍通道向兩側錯開。
var amount = Sl.Uniform(0.004f);
Viewport.PostProcessing.Passes.Add(new PostPass(Sl.Vec3(
    PostPass.Input(PostPass.Uv + Sl.Vec2(amount, 0f)).X,
    PostPass.Input().Y,
    PostPass.Input(PostPass.Uv - Sl.Vec2(amount, 0f)).Z)));
```

泛光讓比白色更亮的表面發光，例如 `EmissiveIntensity` 大於 1 的自發光材質：

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

| 設定 | 預設值 | 作用 |
|---|---|---|
| `Bloom.Enabled` | `false` | 開啟泛光。 |
| `Bloom.Strength` | `0.6` | 加入多少泛光。 |
| `Bloom.Radius` | `0.7` | 泛光擴散多遠，0 到 1。 |
| `Bloom.Threshold` | `1` | 場景亮度超過這個值的像素才會發光。 |
| `Bloom.Knee` | `0.5` | 臨界值的柔和程度。 |
| `Bloom.Levels` | `6` | 模糊的層數，3 到 8。 |
| `Vignette.Enabled` | `false` | 開啟四角變暗。 |
| `Vignette.Intensity` | `0.35` | 四角有多暗，0 到 1。 |
| `Vignette.Smoothness` | `0.5` | 變暗的過渡有多平緩，0 到 1。 |
| `AmbientOcclusion.Enabled` | `false` | 開啟縫隙變暗。 |
| `AmbientOcclusion.Radius` | `0.5` | 表面之間互相遮蔽的距離，世界單位。 |
| `AmbientOcclusion.Intensity` | `1` | 被遮蔽的區域有多暗。 |
| `AmbientOcclusion.Samples` | `16` | 每個像素的取樣數，8 到 32。 |
| `DepthOfField.Enabled` | `false` | 開啟對焦模糊。 |
| `DepthOfField.FocusDistance` | `5` | 離攝影機多遠處是清晰的，世界單位。 |
| `DepthOfField.FocusRange` | `1` | 這個距離前後清晰區域的深度。 |
| `DepthOfField.Falloff` | `4` | 模糊從零增加到最大所經過的距離。 |
| `DepthOfField.MaxBlur` | `8` | 最大模糊半徑，像素。 |
| `Outline.Objects` | 空 | 要描邊的物件，每個都包含它底下的全部子物件。清單裡有內容時顯示描邊。 |
| `Outline.Color` | `#3b82f6` | 描邊的顏色。 |
| `Outline.Thickness` | `2` | 線寬，像素。 |
| `Fxaa` | `false` | 快速近似反鋸齒。 |
| `Saturation` | `1` | 色彩飽和度；0 為灰階。 |
| `Contrast` | `1` | 以中間灰為中心的對比。 |

### 舞台與畫面

視埠預設是透明的，所以它後面的表面會透出來。`StageBrush` 接受一個純色筆刷作為不透明的背景；`{arc:Viewport3DTokenResource StageColor}` 是主題的舞台色。場景的 `Background` 優先於這兩者。`ToneMapping`（預設 `Neutral`，保持基礎色不失真）和 `Exposure` 決定明亮的數值如何顯示到螢幕上，`Samples` 設定多重取樣反鋸齒（0 使用主題的值，1 關閉），`Shadows="False"` 關閉所有陰影。

```xml
<arc:Viewport3D Height="360"
                StageBrush="{arc:Viewport3DTokenResource StageColor}"
                ToneMapping="AcesFilmic"
                Exposure="1.2" />
```

## API 參考

<!-- api: Viewport3D -->

<!-- api: Viewport3DFrameEventArgs -->

## 無障礙

- 3D 畫面裡沒有螢幕閱讀器能讀的文字。用 `AutomationProperties.Name` 為視埠命名，並把它呈現的資訊（數值、狀態、選取項目）同時用一般控制項提供。
- 視埠可以取得鍵盤焦點。設定了 `CameraControls` 時，方向鍵平移，Shift 加方向鍵環繞；縮放沒有按鍵，縮放很重要時請提供你自己的按鈕。
- 不要只靠顏色或光照來表示場景裡的狀態。
- 持續超過五秒的動畫需要有停止它的方式（WCAG 2.2.2），預覽裡的開關就是這樣做的。

## 動效

- 視埠本身不加入任何動效：它只繪製你的程式碼要求的影格。
- 裝飾性的動畫只在完整動態效果下開始（`ArcMotion.IsFull`），節點材質範例就是這樣做的；減少動態效果時繪製靜止的畫面。
- `OrbitControls.EnableDamping` 讓攝影機在拖曳結束後滑行一段再停下。想讓攝影機立刻停住就不要開啟它。

## 響應式

- 視埠採用版面給它的尺寸，沒有偏好尺寸。給它一個高度，或者放進 `Grid` 的儲存格。
- 畫面依顯示器的像素密度繪製，`AutoAspect` 讓透視攝影機跟隨視埠的形狀。
- 在觸控螢幕上，單指環繞，雙指縮放和平移。設定了 `CameraControls` 且允許縮放時，滾輪會縮放場景而不是捲動頁面。

## 相關元件

- `ModelViewer`：單一模型，燈光、地面陰影、攝影機控制和工具列都已備妥。
- `VideoSurface`：另一種繪製在合成樹內部的畫面，用於影片。
