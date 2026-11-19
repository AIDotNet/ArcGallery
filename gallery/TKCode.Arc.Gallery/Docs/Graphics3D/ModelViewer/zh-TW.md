---
title: 模型檢視器
summary: "在帶有地面陰影的打光舞台上展示 glTF 模型或以程式碼建立的模型：可以環繞、縮放和平移，讓它緩慢轉動，並重設檢視。"
source-hash: bf441c3ba1c2
---

<Demo name="Preview" />

預覽從應用程式內建的 GLB 載入 Dafeiyu 模型，紋理也包含在檔案中。拖曳角色可旋轉，捲動可縮放，也可用工具列自動旋轉或重設視角。

## 何時使用

- 產品頁、素材瀏覽器，以及生成或上傳模型的預覽：讓使用者轉動並檢視的單一物件。
- 一個 glTF 2.0 檔案（`.gltf` 或 `.glb`），不想自己設定燈光、攝影機和地面就能有正確的效果。
- 用引擎在程式碼裡建立、需要同樣舞台的物件。

## 何時不用

- 有多個物件、自己的燈光或攝影機，或者由你的程式碼驅動動畫的場景：用 `Viewport3D`。
- 帶有 WebP 紋理的檔案：目前還不支援。Draco、meshopt 網格壓縮和 KTX2（Basis）紋理已支援。
- 一張產品圖片就夠了：用 `Image`，負擔小得多。

## 用法

`ModelViewer` 在 `TKCode.Arc.Graphics3D.Controls` 套件裡，這個套件會一併帶入引擎 `TKCode.Arc.Graphics3D`。在 `UseArc(...)` 裡註冊它：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcGraphics3D());
```

兩個套件都是預覽版，有自己的版本線（0.1.0-preview.1），API 可能在兩個預覽版之間變動。檢視器透過一個 `Viewport3D` 繪製，所以平台方面的說明相同：在 Avalonia 的合成樹內部以 OpenGL 算繪，目前在 macOS（使用 Avalonia 的預設算繪器）、Windows（NVIDIA 顯示卡）和 Linux（Mesa 的軟體 OpenGL）上驗證過，軟體算繪下改走較慢的點陣圖退路。在瀏覽器裡，瀏覽器專案以 `UseArcGraphics3DBrowser()` 啟用 `TKCode.Arc.Graphics3D.Browser` 之後，檢視器在頁面的一個 WebGL 2 內容裡繪製（在無頭 Chromium 和軟體 WebGL 下驗證過）；沒有啟用時檢視器只顯示舞台，沒有模型。

檢視器會自行為拿到的內容取景：把地面放到模型下方，對準燈光，並把攝影機放在能看到整個模型的位置。模型的每個部分都會投射並接收陰影。

### 來自檔案

`Source` 接受 glTF 2.0 檔案（`.gltf` 或 `.glb`）的路徑、應用程式的 `avares://` 資源，或 `http://`、`https://` 位址；`.gltf` 旁邊的緩衝區和圖片會從同一個位置取得。模型在 UI 執行緒之外讀取；讀取期間 `IsLoading` 為 true，並顯示載入指示器。來自網路的模型在每次設定 `Source` 時都會重新下載：檢視器不做快取。`LoadAsync` 則從資料流讀取，例如使用者選取的檔案。從資料流載入的 `.gltf` 找不到它的外部檔案：請用 `.glb` 或 data URI。

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

模型顯示到螢幕上時引發 `ModelLoaded`。載入失敗時引發 `LoadFailed`，檢視器會在模型的位置顯示 `ErrorMessage`。帶有 WebP 紋理的檔案就會這樣失敗：目前還不支援。以 Draco 或 meshopt 壓縮網格、使用 KTX2（Basis）紋理的檔案可以載入。

檔案裡的動畫列在 `Animations` 裡。`PlayAnimation(index)` 循環播放其中一個，`StopAnimation()` 讓模型回到靜止姿勢。

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

### 來自程式碼

`Model` 顯示任何用引擎建立的物件來取代檔案：單一網格或一整個群組。檢視器不會釋放交給它的物件。

```csharp
using TKCode.Arc.Graphics3D;

Viewer.Model = new Mesh(
    new TorusKnotGeometry(0.6f, 0.2f, 160, 24),
    new PhysicalMaterial { Color = ColorF.FromHex(0xc2410c), Roughness = 0.35f, Clearcoat = 1f });
```

three.js 裡的數值可以直接沿用：單位相同，燈光強度相同，攝影機慣例也相同（右手座標系、Y 軸朝上、視野以角度表示）。

<Demo name="FromCode" />

### 燈光、旋轉與陰影

`Environment` 選擇照亮模型並出現在它反射裡的環境：`Studio`（預設，灰色房間裡的柔光箱；顏色不失真）、`Outdoor`（晴朗的天空和暖色的太陽）或 `None`（只有檢視器內建的兩個光源）。這些環境是計算出來的，所以應用程式不需要附帶任何圖片檔。`EnvironmentIntensity` 是環境光的倍數，`Exposure` 和 `ToneMapping` 決定明亮的數值如何顯示到螢幕上，`ShadowIntensity` 設定地面陰影的深淺，從 0（沒有）到 1。

`AutoRotate` 在沒有人拖曳時讓模型繞垂直軸緩慢轉動，速度是每秒 `AutoRotateSpeed` 度（預設 12：30 秒轉一圈）。它只在完整動態效果下轉動。

<Demo name="Lighting" />

### 攝影機與工具列

使用者拖曳來環繞，用滾輪縮放，用右鍵平移；在觸控螢幕上，單指環繞，雙指縮放和平移。角落裡的兩個按鈕分別切換自動旋轉，以及把攝影機放回能看到整個模型的位置；`ResetView()` 在程式碼裡做同樣的事。`CameraControlsEnabled="False"` 固定攝影機，`ShowToolbar="False"` 隱藏按鈕。按鈕的名稱跟隨應用程式的語言（內建 en-US、zh-CN 和 zh-TW）；想用自己的措辭時設定 `ResetViewLabel` 和 `AutoRotateLabel`。

```xml
<arc:ModelViewer Height="320"
                 CameraControlsEnabled="False"
                 ShowToolbar="False"
                 Environment="Outdoor"
                 ShadowIntensity="0.5" />
```

`CameraControls`、`Camera` 和 `Scene` 提供檢視器自己的環繞控制器、攝影機和場景，用來限制環繞範圍，或者在模型旁邊加入燈光和物件。`PostProcessing` 提供內部視埠的效果（環境光遮蔽、景深、描邊、泛光、暗角、飽和度、對比、FXAA；見 `Viewport3D`）；在檢視器的範本套用之前它是 null。

```csharp
Viewer.CameraControls.MinPolarAngle = MathF.PI / 6f;
Viewer.CameraControls.MaxPolarAngle = MathF.PI / 2f;
Viewer.Scene.Add(new PointLight(ColorF.FromHex(0xffd9a0), 8f) { Position = new Vector3(1f, 2f, 1f) });
```

## API 參考

<!-- api: ModelViewer -->

<!-- api: ModelLoadFailedEventArgs -->

## 無障礙

- 模型裡沒有螢幕閱讀器能讀的文字。用 `AutomationProperties.Name` 為檢視器命名，並在旁邊用文字描述產品。
- 兩個按鈕的無障礙名稱和工具提示使用應用程式的語言；`ResetViewLabel` 和 `AutoRotateLabel` 可以取代它們。
- 畫面可以取得鍵盤焦點：方向鍵平移，Shift 加方向鍵環繞。縮放沒有按鍵。
- 轉動的模型屬於會動的內容：自動旋轉按鈕讓使用者可以停止它（WCAG 2.2.2），所以開啟 `AutoRotate` 時請保留工具列。

## 動效

- 自動旋轉是裝飾。它只在完整動態效果下執行；減少動態效果時模型靜止不動，使用者仍然可以手動轉動它。
- 拖曳結束後攝影機會滑行一段再停下。想讓攝影機立刻停住，把 `CameraControls.EnableDamping` 設為 false。
- 檢視器依需求繪製：靜止的模型不繪製任何影格。攝影機移動時、自動旋轉執行時和動畫播放時，它才持續繪製。

## 響應式

- 檢視器會填滿給它的寬度，最小高度為 160。請給它一個高度，或者你自己的長寬比。
- 檢視器改變形狀時模型維持在取景內：攝影機的長寬比跟隨檢視器。
- 在觸控螢幕上，單指環繞，雙指縮放和平移。指標在模型上方時，滾輪會縮放模型而不是捲動頁面；放在長頁面裡、不應佔用滾輪的檢視器請設定 `CameraControlsEnabled="False"`。

## 相關元件

- `Viewport3D`：只有視埠，用於你自己的場景、燈光和攝影機。
- `Image`：產品的靜態圖片。
