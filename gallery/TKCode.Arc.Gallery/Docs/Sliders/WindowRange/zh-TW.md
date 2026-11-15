---
title: 時間視窗
summary: 疊在即時面積圖上的可移動視窗，拖曳邊緣調整大小，甩動即可平移，日期隨之滾動。
source-hash: e05b7451ea04
---

<Demo name="Report" />

## 何時使用

`WindowRange` 用來選取序列中的一段，例如報表的日期範圍，同時讓資料的形狀始終可見。視窗內的部分以全彩繪製，其餘部分淡出，使用者放開之前就能看到自己選中了什麼。

- **分析報表的日期範圍**：每日工作階段、註冊或營收，視窗移動時上方的合計隨之更新。
- **比較時段**：讓固定長度的視窗在用量或延遲上滑動，找出繁忙的幾週。
- **概覽條**：放在只顯示所選區段的詳細圖表或表格上方。

## 何時不用

- **從日曆中選取確切日期**，或範圍跨越多年：使用 `DateRangePicker`。
- **用視窗縮放一張大型詳細圖表**並在圖上標出事件：使用 `BrushChart`。
- **背後沒有序列的數值範圍**：使用帶 `IsRange` 的 `Slider`；數值分布值得展示時使用 `RangeHistogramSlider`。

## 用法

在 `Data` 中傳入序列，每個步長一個數字，並用 `ValueFormatter` 說明如何書寫一個值。值採用定義域單位：預設 `Minimum` 為 0，`Maximum` 為最後一個點的索引，因此一組每日計數的定義域就是「第 0 天到第 N 天」。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:WindowRange Label="Report range"
                     Data="{Binding DailySessions}"
                     Start="{Binding From}"
                     End="{Binding To}"
                     MinSpan="6"
                     IsLive="True" />
</UserControl>
```

```csharp
var first = new DateTime(2026, 3, 2);
range.ValueFormatter = index => first.AddDays(index).ToString("MMM d", CultureInfo.InvariantCulture);
range.SpanFormatter = span => $"{span + 1:N0} days";
range.Ticks = [new WindowRangeTick(30, "Apr"), new WindowRangeTick(60, "May")];
```

`Start` 和 `End` 是雙向繫結的。保持為 null 時，視窗涵蓋定義域的最後四分之一。`WindowStart` 和 `WindowEnd` 始終是實際繪製的視窗，已套用步長和跨度限制。`ValueChanged` 在每次變化時觸發，拖曳或慣性滑行進行中也會觸發，因此適合計算即時合計。`ValueCommitted` 在拖曳、甩動或按鍵之後視窗靜止時觸發一次：在這裡取得報表。

`SpanFormatter` 收到的是終點減起點；要得到包含首尾的天數需要加一。在狹窄的欄中設定 `ShowSpan="False"` 可以去掉中間的讀數。

### 快捷範圍

設定範圍的按鈕呼叫 `SetRange(start, end)`：兩端同時改變，`ValueChanged` 只觸發一次，框架滑行過去，讀數隨之滾動。上面的範例只有在視窗恰好等於某個快捷範圍時才把它標為選取。

### 限制與步長

兩端都吸附到 `Step`。`MinSpan` 和 `MaxSpan` 限制視窗的長度（預設分別為一個步長和整個定義域）。方向鍵按 `Step` 移動；Page Up、Page Down 以及 Shift 加方向鍵使用 `LargeStep`（未設定時為定義域的十分之一）。

### 即時資料

序列變化時給 `Data` 指派一個新清單：點數相同時，折線和面積平滑變形，否則直接替換圖表。`IsLive` 會在最新的點上顯示一個脈動的圓點；只在該點仍在變化時開啟它。

## API 參考

<!-- api: WindowRange -->

## 無障礙

- 控制項是一個以 `Label` 命名的群組。起點邊緣、視窗和終點邊緣是三個滑桿，分別命名為「Report range, start」「Report range, window」和「Report range, end」。
- 邊緣回報自己能到達的範圍，該範圍遵守 `MinSpan` 和 `MaxSpan`，並透過 `ValueFormatter` 朗讀自己的值。視窗回報其起點，並朗讀兩端，例如「Jun 2 to Jul 1」。
- 圖表、框架和讀數對輔助技術而言是裝飾；所有值都由滑桿承載。
- 按下會把焦點交給命中的部分，但不顯示焦點環。鍵盤焦點會顯示焦點環：邊緣的焦點環圍繞邊緣，視窗的焦點環圍繞整個框架。

### 鍵盤

| 按鍵 | 操作 |
|---|---|
| `Tab` | 依序經過起點邊緣、視窗和終點邊緣 |
| `Right` / `Up` | 把取得焦點的邊緣或整個視窗移動一個 `Step` |
| `Left` / `Down` | 往回移動一個 `Step` |
| `Shift` + 方向鍵、`PageUp` / `PageDown` | 移動一個 `LargeStep` |
| `Home` / `End` | 視窗：移到定義域的起點或終點。邊緣：在跨度限制允許的範圍內移到最遠 |
| 在限制處按鍵 | 值不變；框架朝按鍵方向撞一下再彈回 |

## 動效

- 拖曳邊緣或視窗時一比一跟隨指標。視窗越過定義域末端時最多讓出 18 px，邊緣越過跨度限制時最多讓出 10 px，然後彈回。
- 放開平移時保留速度：視窗落在甩動約 0.2 s 能到達的位置並吸附到步長，以一個從該速度出發的彈簧減速停下。滑行時抓住視窗會讓它停在原處。
- 在視窗外按下會讓視窗滑行到以指標為中心的位置，指標移動時視窗繼續跟隨。
- 按鍵、快捷範圍和新值使用與慣性滑行相同的彈簧移動框架。讀數按變化方向逐位滾動，月份名稱作為一個整詞滾動；拖曳或滑行時讀數原地更新。
- 按住視窗時，視窗內出現淡淡的著色，兩個握把都會拉長；按住邊緣時，它自己的握把拉得更長。
- 新資料以柔和的彈簧改變折線和面積的形狀，即時圓點每 2.4 s 發出一圈光環。
- 減少動效時，框架和讀數直接跳到每個值，沒有滑行、拉伸、滾動、變形或脈動。關閉動效時一切都是即時的。

## 響應式

繪圖區和框架填滿給定的寬度，讀數共用一列，長度位於中間。控制項在約 320 px 以上可用；更窄的空間裡用 `ShowSpan` 隱藏長度，標籤會互相碰到時少傳一些刻度。邊緣在把手外 14 px 內都能被按到，因此用觸控也容易抓住，控制項不依賴懸停。

## 相關元件

| 元件 | 適用情境 |
|---|---|
| `DateRangePicker` | 從日曆中選取確切日期 |
| `BrushChart` | 用視窗縮放詳細圖表 |
| `RangeHistogramSlider` | 在分布之上選取數值範圍 |
| `Slider` | 沒有資料可展示的範圍 |
