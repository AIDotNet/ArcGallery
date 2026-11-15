---
title: A2UI 介面
summary: "用 Arc 控制項呈現 A2UI 1.0（代理到使用者介面）訊息：代理用 JSON 描述一個介面，介面檢視把它畫出來，保存使用者輸入的資料，並把使用者的操作傳回代理。"
source-hash: d4162f1ba2cb
---

<Demo name="Preview" />

## 何時使用

- AI 代理需要用介面而不是文字回答：要填寫的表單、要做的選擇、要比較的清單或圖表。
- 介面在執行時由模型或伺服器產生，又必須留在你的設計系統裡：代理只能使用你登記的元件，不能寫程式碼或樣式。
- 多個前端共用一個協定：A2UI 在網頁、行動裝置和這裡都一樣。

## 何時不用

- 自己用 XAML 或 C# 建立的介面：直接用控制項。
- 豐富文件與長回答：用 `Markdown`。

## 用法

加入套件並在 `UseArc` 中註冊：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcA2UI());
```

建立 `A2UIRenderer`，把代理的訊息（每行一個 JSON 物件）交給它，再用 `A2UISurfaceView` 顯示一個介面。檢視會等到介面被建立，隨元件到達逐步填入，介面被刪除時清空。

```csharp
var renderer = new A2UIRenderer();
renderer.Outbound += (_, e) => SendToAgent(e.ToJson());   // 動作、函式呼叫、錯誤
var view = new A2UISurfaceView { Renderer = renderer, SurfaceId = "booking_1" };

await renderer.ProcessAsync(responseStream);                // JSON Lines，在 UI 執行緒上套用
```

### 雙向資料

輸入元件（`TextField`、`CheckBox`、`ChoicePicker`、`Slider`、`DateTimeInput`）會把使用者輸入寫進介面資料模型中 `value` 繫結的路徑，繫結同一路徑的其他元件立即更新。在動作發生之前不會傳送任何內容給代理；設定了 `sendDataModel` 時，資料會隨動作一起送出（`GetRendererDataModel`）。

<Demo name="Form" />

### 驗證與動作

`@checks` 規則用目錄裡的函式（`required`、`email`、`regex`、`length`、`numeric`、`and`、`or`、`not`）驗證輸入和按鈕；驗證不通過的按鈕會停用。按鈕的 `action.event` 會變成一則解析好 `context` 的 `action` 訊息；`action.functionCall` 執行本機函式，例如 `openUrl`，它會先觸發 `UrlOpening`。

### 目錄

呈現器預設支援兩個目錄：

- A2UI 基本目錄（`A2UICatalogs.Basic`，18 個元件、14 個函式）：文字、圖片、圖示、列與欄、清單、卡片、分頁、對話框、按鈕和各種表單輸入；
- Arc 目錄（`A2UICatalogs.Arc`，18 個以 Arc 控制項繪製的元件）：`BarChart`、`LineChart`、`DonutChart`、`Sparkline`、`Metric`、`Table`、`Timeline`、`Stepper`、`Progress`、`Alert`、`Badge`、`Avatar`、`Markdown`、`CodeBlock`、`Accordion`、`Switch`、`SegmentedControl` 和 `Rating`。元件寫上 `"@catalogId": "https://github.com/AIDotNet/TKCode.Arc/a2ui/catalogs/arc/v1"` 即可使用；其中的輸入元件和基本目錄一樣雙向繫結，Markdown 裡的連結會先經過 `UrlOpening`。

可以用 JSON Schema 文件加入自己的目錄，並為每個元件登記一個檢視：

```csharp
var charts = A2UICatalog.FromJson(chartsCatalogJson);
var options = new A2UIRendererOptions { Catalogs = [A2UICatalogs.Basic, A2UICatalogs.Arc, charts] };
var registry = A2UIComponentRegistry.CreateDefault()
    .Register(charts.Id, "BarChart", () => new MyBarChartView());
```

元件用 `@catalogId` 選擇目錄；由代理實作的函式會透過 `callAgentFunction` 傳給代理。

### 驗證模式

預設（`A2UIValidationMode.Lenient`）每個元件都依目錄 Schema 驗證，尚未到達的子元件顯示為佔位，適合模型串流輸出的回答。`Strict` 還會拒絕不完整的批次（缺少 root、懸空參照、循環）。被拒絕的訊息透過 `ErrorReported` 回報，同時作為 `error` 訊息傳給代理，模型可以據此自行修正。

## API 參考

<!-- api: A2UISurfaceView -->

## 無障礙

`@accessibility` 對應到平台：`label` 對應自動化名稱，`description` 對應說明文字，`live` 對應即時播報設定，`hidden` 對應原始自動化檢視。Arc 控制項保留各自的鍵盤與螢幕閱讀器行為。

## 動效

元件沿用繪製它們的 Arc 控制項的動效。回答串流到達時，新元件佔據自己的位置，不推動相鄰元件；檢視本身沒有額外動畫。

## 響應式

列、欄和清單使用給定的寬度；橫向清單可以捲動。代理可能送出很長的介面時，把檢視放進可捲動的容器。

## 相關元件

- `Markdown`：同一代理的豐富文字回答。
- `ChatThread`：訊息裡可以放介面的對話。
- `Form` 與 `FormItem`：用 XAML 撰寫的表單。
