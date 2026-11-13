---
title: 移動端單選卡片
summary: 以圖片或簡短描述呈現的可選卡片網格，適合一次只能選一項的選擇；有按下狀態，沒有懸停狀態。
source-hash: bbf600b5d55c
---

<Demo name="Basic" />

## 何時使用

`RadioCards` 把一項選擇排成卡片網格，因此每個選項都可以展示圖示、色塊或一行描述。

- **方案、配送速度、版面配置**：每個選項都需要一句話才能區分的情境。
- **形狀、顏色、樣式**：比起名稱，更容易靠看來辨認的選項。
- **設定頁中的一項選擇**：把網格放進 `ListGroup` 的 `ListCell` 裡。

即時範例在手機外框裡執行，那是桌面上的模擬；真機上的觸控回饋這裡沒有驗證。

## 何時不用

- **只有簡短標籤的選項**：用 `MobileSegmented`（兩到四項）或 `MobileRadio`（清單）。
- **值顯示在一列裡的設定**：用 `MobilePicker`。
- **任意數量的選項同時選擇**：單選卡片只保存一項選擇；用 `MobileCheckbox`。

## 用法

在群組中寫入 `RadioCardOption`，或者把 `ItemsSource` 繫結到一個選項清單。每個選項都有 `Value`、`Label`、可選的 `Description` 和可選的 `Icon`。`SelectedValue`（雙向）是選取選項的 `Value`；`SelectedIndex` 和 `SelectedItem` 的用法與任何 Avalonia 選擇控制項相同，`SelectionChanged` 會回報每一次選取的變化。

```xml
<arcm:RadioCards xmlns="https://github.com/avaloniaui"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                 xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                 SelectedValue="{Binding Plan}">
    <arcm:RadioCardOption Value="free" Label="Free" Description="3 projects and 1 GB of storage." Icon="{x:Static arc:ArcIcons.Zap}" />
    <arcm:RadioCardOption Value="pro" Label="Pro" Description="Unlimited projects and priority support." Icon="{x:Static arc:ArcIcons.Star}" />
    <arcm:RadioCardOption Value="enterprise" Label="Enterprise" Description="Talk to sales first." IsEnabled="False" />
</arcm:RadioCards>
```

卡片在 `Columns` 指定的欄數之間平分寬度（預設 2 欄），同一列的卡片高度與其中最高的一張相同。`Icon` 如果是幾何圖形（例如 `ArcIcons` 中的 Lucide 圖示），會以 22 px 的描邊圖示繪製；其他內容（`Ellipse` 色塊、形狀）則原樣顯示。手指按在卡片上會使其變暗，啟用完整動效時還會輕微下壓；在卡片上抬起手指即選取它，而從卡片上開始的捲動不會選取。

`IsEnabled="False"` 會讓選項變暗且無法選取；`IsVisible="False"` 會隱藏它，並且不佔空間。卡片會跟隨選項之後的變化。不是選項的資料項目可以用 `ItemTemplate` 顯示；此時卡片的標籤就是該範本。

### 緊湊選擇

對於色塊和形狀，用 `ShowIndicator="False"` 關閉單選指示器，並使用三欄：滑動到選取卡片的圓環本身就標示了選擇。

<Demo name="Swatches" />

```xml
<arcm:RadioCards Columns="3" ShowIndicator="False" SelectedValue="{Binding Color}">
    <arcm:RadioCardOption Value="violet" Label="Violet">
        <arcm:RadioCardOption.Icon>
            <Ellipse Width="28" Height="28" Fill="#6E56CF" />
        </arcm:RadioCardOption.Icon>
    </arcm:RadioCardOption>
</arcm:RadioCards>
```

### Token

`RadioCardsToken`（識別 `MobileRadioCards`）保存 `ColumnGap` 和 `RowGap`（10）以及 `RingThickness`（2）。`RadioCardToken`（識別 `MobileRadioCard`）保存卡片的 `CardPadding`（12）、`IconExtent`（22）、`LabelMargin`、`DescriptionMargin`、`IndicatorSize`（22）、`IndicatorBorderWidth`（1.5）、`DotSize`（10）、`IndicatorMargin`、`DisabledOpacity`（0.5），以及焦點環的 `FocusRingMargin` 和 `FocusRingRadius`。識別帶有 `Mobile` 前綴，是因為桌面套件有自己的單選卡片。顏色、圓角和字型來自 ArcDesign token：圓環和圓點使用 `ControlOn`，與移動端單選一致。

## API 參考

<!-- api: MobileRadioCards -->

<!-- api: MobileRadioCard -->

<!-- api: MobileRadioCardOption -->

## 無障礙

整個群組是一個 Tab 停駐點：Tab 會落在選取的卡片上（若沒有選取項，則落在第一張可選的卡片上）。方向鍵在網格中移動選擇，左右鍵依閱讀順序移動（從右到左的版面中會鏡像，並且循環），上下鍵依列移動；停用和隱藏的卡片會被略過。Home 和 End 跳到第一張和最後一張卡片，Enter 或 Space 選取目前有焦點的卡片。鍵盤焦點會在卡片周圍顯示 2 px 的焦點環。卡片以其標籤命名，其描述即為說明文字。選取狀態由圓環和實心的單選圓點標示，而不只靠顏色；當 `ShowIndicator="False"` 時，較粗的圓環仍然保留。

## 動效

選擇變化時，一個圓環以彈簧效果從一張卡片滑到另一張；圓環在渲染階段繪製，因此版面不會移動。圓環第一次出現時會原地淡入。單選圓點以短暫的淡入彈入。按下的卡片以快速曲線變暗，僅在啟用完整動效時，還會以較靈敏的彈簧縮放到 0.97。減少動效時，圓環直接跳到目標位置，不做縮放，圓點和顏色立即變化。

## 響應式

- `RadioCards` 沒有斷點：`Columns`（預設 2）指定的等寬欄平分群組得到的寬度，同一列的卡片高度與其中最高的一張相同。
- 在手機上，帶指示器和文字的卡片保持兩欄；三到四欄適合帶簡短標籤的圖示和色塊，並使用 `ShowIndicator="False"`。標籤和描述會換行，因此卡片會變高而不是變寬。
- 隱藏的選項不佔空間，其後的卡片會向上補位進入網格。
- 沒有任何功能依賴懸停：手指按住會使卡片變暗，抬起即選取該卡片。

## 相關元件

- `MobileRadio`：以清單列呈現的一項選擇。
- `MobileSegmented`：一列中的兩到四個簡短選項。
- `MobilePicker`：顯示值的一列，點擊後在底部面板中開啟選項。
- `MobileListGroup`：把網格放進 `ListCell` 中，以便與其他列一起排列。
