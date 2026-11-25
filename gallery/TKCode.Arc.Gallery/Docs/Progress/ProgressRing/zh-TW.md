---
title: 環形進度
summary: 同時支援確定圓形進度與不確定旋轉載入狀態的環形指示器。
source-hash: 47e69f10a656
---

<Demo name="Overview" />

## 何時使用

`ProgressRing` 在精簡的圓形空間中傳達進行中的操作或可度量的完成進度。當水平進度條過寬，或任務總量未知、需要明確的載入指示時使用。

- **不確定載入狀態**：將 `IsIndeterminate="True"` 用於資料載入、檢視重新整理或卡片、對話方塊與工具列中的背景等待。
- **精簡的確定進度**：適用於上傳、下載、匯出或同步任務，將環形進度放在項目旁或空狀態置中位置。
- **行內狀態指示**：使用 `Size="Small"` 搭配狀態文案或放在表格儲存格中表示正在處理。

## 何時不用

- **表單或清單中的整列進度任務**：有充足水平空間且需要左側標題列時，使用 `Progress`。
- **頁面或卡片內容預留位置**：載入內容前希望保留版面結構時，使用 `Skeleton`。
- **帶閾值分級的儀表板指標**：需要 270° 刻度與警告/危險區間時，使用 `Gauge`。

## 用法

`ProgressRing` 預設為確定模式（`IsIndeterminate="False"`），透過 `Value`、`Minimum`（預設 `0`）與 `Maximum`（預設 `100`）設定進度；設定 `IsIndeterminate="True"` 可切換為持續旋轉的載入環。

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

支援 `ControlSize.Small`（20 px）、`ControlSize.Medium`（40 px，預設）與 `ControlSize.Large`（64 px），也可以明確設定 `Width`、`Height` 與 `Thickness`。

<Demo name="Sizes" />

### 確定進度

單一平滑彈簧同時驅動順時針圓弧與中心百分比。當 `Value` 達到 `Maximum` 時，圓環切換為成功色；開啟 `IsValueVisible` 時，中心百分比會切換為完成勾號。

<Demo name="Determinate" />

### 自訂中心內容

設定 `Content`（以及可選的 `ContentTemplate`）可在圓環中心放置自訂文字、計數或圖示。設定自訂內容後會取代內建的百分比讀數。

<Demo name="CustomContent" />

## API 參考

<!-- api: ProgressRing -->

## 無障礙

- `ProgressRing` 為唯讀指示器，不取得鍵盤焦點（`Focusable="False"`）。
- 自動化控制項類型為 `ProgressBar`。
- `Label` 作為無障礙名稱；未設定標籤時，不確定模式預設為 `"Loading"`，確定模式預設為 `"Progress"`。
- `AutomationProperties.ItemStatus` 在不確定模式下回報 `"Loading"`，在確定模式下回報目前百分比（例如 `"68%"`）。

## 動效

- **確定模式**：由 `ArcSpring.Smooth` 彈簧同步驅動圓弧與百分比；達到 100% 後延遲 200 ms 切換為 `Success` 顏色並顯示完成勾號。
- **不確定模式**：由 `LiveLoop` 驅動的弧段在可見且位於視口內時持續旋轉，離開視口自動暫停。
- **減弱動效**：確定模式直接跳至目標值；不確定模式停止旋轉並改為柔和的不透明度呼吸。
- **關閉動效**：所有變化立即到位，不確定模式顯示靜止圓弧。

## 響應式行為

`ProgressRing` 依據 `Size` 預設（`Small`、`Medium`、`Large`）決定尺寸，或在明確的 `Width` 與 `Height` 內保持正圓並置中顯示於可選 `Label` 上方。

## 相關

- `Progress`：帶標題列的水平進度條。
- `Skeleton`：載入期間保留版面結構的骨架屏。
- `Gauge`：帶閾值分級的儀表板指標盤。
