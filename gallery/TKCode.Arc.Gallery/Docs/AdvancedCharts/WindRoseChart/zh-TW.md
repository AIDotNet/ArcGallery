---
title: 風玫瑰圖
summary: 呈現風從各個方向吹來的頻率與強度：花瓣依風速分層堆疊，可選百分比或計數刻度，中心為無風占比。
source-hash: 450bb00caf1e
---

<Demo name="Meteorological" />

## 何時使用

`WindRoseChart` 呈現方向上的分布，並在其上堆疊一個量值。每個方向扇區是一片花瓣，花瓣長度表示風從該方向吹來的頻率，花瓣各層則依風速把這段時間切分開。無風向的無風時段位於中心。

- **氣候與選址研究**：氣象站一年、一季或一個月的風況，用於機場跑道、風電場、港口或建築通風。
- **擴散與安全**：煙、噪音或洩漏物大多朝哪個方向飄散，以及飄散得有多快。
- **其他依方向與強度劃分的資料**：浪向與浪高、交通的行駛方向與車速、聲源方位。

圖表在 8 或 16 個扇區、四到七個風速區間時效果最好。

## 何時不用

- **每個方向只有一個數值，沒有可拆分的量值**：請使用 `RadarChart`。
- **隨時間變化的風況**：請使用橫軸為時間的 `LineChart` 或 `RealtimeStream`。
- **精確比較兩個方向**：不同角度上的花瓣難以精確比較。請使用 `BarChart`。
- **沒有環狀順序的類別**：花瓣圍成一圈，因此只有末尾類別與首個類別相鄰的資料才適合放進玫瑰圖，例如羅盤方向或一天中的小時。

## 用法

提供列（每個方向一列，附帶每個風速區間的數量）、記錄（每筆記錄包含一個方向、一個區間和一個數量）或你自己的物件，並可選擇提供無風數量。若不設定 `Bins`，圖表使用六個風速區間，單位為 m/s：0.5–2、2–4、4–6、6–8、8–10 以及 10 以上。

```xml
<arc:WindRoseChart Label="Harbour station, March" SectorCount="8" Calm="12">
    <arc:WindRoseRow Direction="N" Values="10 6 2 0 0 0" />
    <arc:WindRoseRow Direction="NE" Values="14 9 4 1 0 0" />
    <arc:WindRoseRow Direction="E" Values="5 3 1 0 0 0" />
    <arc:WindRoseRow Direction="SE" Values="4 2 0 0 0 0" />
    <arc:WindRoseRow Direction="S" Values="8 7 3 1 0 0" />
    <arc:WindRoseRow Direction="SW" Values="12 18 15 8 3 1" />
    <arc:WindRoseRow Direction="W" Values="9 12 8 4 1 0" />
    <arc:WindRoseRow Direction="NW" Values="6 5 2 1 0 0" />
</arc:WindRoseChart>
```

### 結構

1. **花瓣**：每個方向扇區一片，以其方向為中心。扇區 0 位於頂端的北方，扇區依順時針排列，與羅盤一致。
2. **風速層**：每片花瓣由風速區間從中心向外堆疊而成，層與層之間留有一小段間隔。
3. **環**：徑向刻度，數值沿前兩個扇區之間的邊界標註。
4. **無風圓**：中心處無風讀數的占比（或計數）。
5. **方向標籤**：外環之外的羅盤方位，使用目前的語言。
6. **提示**：方向、其總量以及每個風速層的數量。
7. **圖例**：玫瑰圖下方，顯示每個風速區間的顏色與範圍。

### 資料：列、記錄或你自己的物件

`ItemsSource` 接受任何 `WindRoseRow` 和 `WindRoseRecord` 清單，兩者可以混用；不設定時，圖表讀取內嵌的列（`Items`）。同一扇區、同一區間的項目會累加，可觀察集合的變化也會被追蹤。

| 型別 | 成員 | 說明 |
|---|---|---|
| `WindRoseRow` | `Direction` | 從北開始順時針計算的度數（數字），或方向名稱，如 `NNE`、`NbE`，或 `DirectionLabels` 中的某個標籤 |
| `WindRoseRow` | `Values` | 每個風速區間一個數量，由內向外排列；在 XAML 中寫作 `Values="1.2 3.4 0.8"` |
| `WindRoseRecord` | `Direction` | 同 `WindRoseRow` |
| `WindRoseRecord` | `Bin` | 風速區間的索引 |
| `WindRoseRecord` | `Value` | 數量 |
| `WindRoseBin` | `Minimum` | 區間的最低風速；下一區間的最小值即本區間的上限，最後一個區間沒有上限 |
| `WindRoseBin` | `Label` | 圖例和提示中的名稱；預設為區間範圍加上 `Unit`，如 "2–4 m/s" |
| `WindRoseBin` | `Color` | 固定顏色；預設取圖表漸層色中對應該區間的那一階 |

方向歸屬於包含它的扇區：16 個扇區時，扇區 1（NNE）涵蓋 11.25° 至 33.75°（不含 33.75°），360° 即北方。缺失、為負或不是數字的數量以零計算；超出 `Bins` 範圍的區間索引會被忽略。

自定義物件無需包裝。把成員繫結指向它們即可，可以使用長格式（每個物件一個數量），也可以使用寬格式（每個物件一個方向，並附帶一組數量）：

```xml
<arc:WindRoseChart ItemsSource="{Binding Readings}"
                   DirectionMemberBinding="{Binding Heading}"
                   BinMemberBinding="{Binding SpeedClass}"
                   ValueMemberBinding="{Binding Hours}" />
```

### 原始觀測資料的分箱

`WindRoseBinning.Bin` 將原始讀數（以度表示的風向與風速）分入扇區與風速區間。風速低於無風門檻（預設 0.5）的讀數，無論風向為何都計為無風；風向或風速不是數字、或風速為負的讀數會被略過，並計入 `Skipped`。

```csharp
var readings = station.Hours.Select(h => new WindObservation(h.DirectionDegrees, h.SpeedMetresPerSecond));
var wind = WindRoseBinning.Bin(readings, sectorCount: 16);
chart.ItemsSource = wind.Rows;
chart.Calm = wind.Calm;
```

傳入 `binMinimums` 可使用其他區間（並讓圖表使用相同的 `Bins`），傳入 `calmThreshold` 可改變無風的判定標準。泛型多載透過兩個選取器讀取任意項目：`WindRoseBinning.Bin(hours, h => h.Heading, h => h.Knots, 16)`。

### 扇區與方向

`SectorCount` 設定方向的數量。8、16 和 32 個扇區使用羅盤標籤（N、NE……；N、NNE……；N、NbE……）；其他從 4 到 72 的數量則以度數標註，例如 36 個 10° 的扇區。`StartAngle` 旋轉玫瑰圖，使扇區 0 位於螢幕上的另一個角度；`SweepDirection="CounterClockwise"` 讓扇區逆時針排列。兩者都不改變資料的歸屬：方向始終是羅盤度數。

<Demo name="SectorCounts" />

`DirectionLabels` 取代扇區 0 起的標籤；空白或缺失的項目保留該扇區的預設標籤，資料也可以用自定義標籤指定扇區。

```csharp
chart.SectorCount = 4;
chart.DirectionLabels = ["Bow", "Starboard", "Stern", "Port"];
```

### 風速區間、顏色與單位

預設情況下，各區間取自主題的第一個圖表顏色的同一色相：淺色表面上由淺到深，深色表面上由深到淺，因此風速越大看起來越強，並且圖表會跟隨強調色與外觀模式。為某個區間設定 `Color` 即可固定其顏色，設定 `Unit` 則決定預設標籤的單位。固定顏色在兩種外觀下保持不變，因此應選擇在淺色和深色表面上都清楚可辨的顏色。

<Demo name="CustomBins" />

### 百分比或計數刻度

使用 `RadialAxis="Percent"`（預設）時，花瓣的長度是其占全部讀數的比例（包含無風），因此花瓣與無風圓相加恰好為 100%。`RadialAxis="Count"` 則依原值繪製數量。`RadialMaximum` 固定外環處的數值（超出的花瓣延伸至邊緣為止，提示仍顯示真實數值），`RadialStep` 設定環與環之間的距離；保留 NaN 時，圖表會選擇一個整齊的最大值，環數不超過五個。

<Demo name="CountAxis" />

```xml
<arc:WindRoseChart RadialMaximum="20" RadialStep="5" ItemsSource="{Binding Months[0].Rows}" />
```

`ValueFormatter` 格式化所有顯示的數值（環刻度、提示、中心及讀數）。它接收的是百分比（0 到 100）或數量本身。

### 無風

`Calm` 是無風的數量，單位與數值相同：數值為計數時它也是計數，資料已是百分比時它就是百分比（此時百分比刻度會原樣保留）。中心以占比或計數顯示它；若無風圓太小、放不下文字，則只有摘要和讀數會顯示它。

### 即時資料

取代 `ItemsSource`、修改集合，或修改 `Calm`、`Bins` 或 `SectorCount`，每個色帶都會從螢幕上的目前位置彈至新的長度，同時環隨新的最大值移動。同一排程週期內對集合的修改只會讀取一次，因此清空後再重新填入，玫瑰圖會一次移動到位。

<Demo name="LiveUpdate" />

### 讀取與選取方向

懸停在花瓣（或其標籤）上，或使用方向鍵移動，會設定 `ActiveIndex` 並顯示提示；其他花瓣變淡，指標所在的層會顯示外框。懸停或聚焦圖例項目時，該風速區間會在所有花瓣中反白。

點選、輕觸、Enter 或 Space 會引發 `ItemInvoked`，攜帶扇區以及指標下方的層（透過鍵盤觸發時 `Bin` 為 -1）。設定 `IsSelectionEnabled` 後，該扇區還會成為 `SelectedIndex`，並以外框突顯；`SelectionChanged` 回報 `OldItem` 與 `NewItem`。無論變化來自手勢、程式碼還是繫結，只要 `SelectedIndex` 改變就會引發它。`Sectors` 列出每個扇區及其標籤、範圍、數量、總計和占比。

<Demo name="Selection" />

### 繫結到檢視模型

```xml
<arc:WindRoseChart Label="{Binding StationName}"
                   ItemsSource="{Binding Wind.Rows}"
                   Calm="{Binding Wind.Calm}"
                   SectorCount="{Binding Sectors}"
                   SelectedIndex="{Binding SelectedDirection, Mode=TwoWay}"
                   IsSelectionEnabled="True" />
```

- `ItemsSource` 可以是陣列、清單或可觀察集合。就地修改某一列的 `Values` 不會被察覺：請取代該列、清單或集合中的項目。
- `SelectedIndex` 與 `ActiveIndex` 支援雙向繫結；-1 表示無。

### 方向名稱與語言

預設標籤取自目前的 Arc 語言：英文為 N、NNE、NE……，簡體中文為 北、北東北、東北……，繁體中文為 北、北北東、東北……，語言切換時標籤隨之變化。"Calm" 也跟隨語言顯示為「無風」。資料可以使用英文縮寫（`NNE`、`NbE`）或目前語言的名稱來指定方向。

### 內容指南

- **Label**：寫明地點與時段，例如「港口站，2025 年每小時風況」。
- **Bins**：四到七個，依升序排列。區間邊界取整，並透過 `Unit` 統一標註一次單位。
- **自定義標籤**：保持簡短；字級較小時，標籤會每兩個、每四個方向顯示一個。

### 最佳實務

**建議**

- 說明花瓣顯示的是占全部讀數的比例還是計數，以及是否包含無風。
- 比較多個玫瑰圖時，保持相同的區間，並固定 `RadialMaximum`。
- 用與圖表顯示相同的扇區數對原始讀數分箱。

**避免**

- 比較花瓣面積：請讀取長度與提示。
- 把為 16 個扇區準備的資料重新歸入 8 個扇區：NNE 這類半途方向會落入順時針方向的下一個扇區。請重新對原始讀數分箱。

### 常見問題

**為什麼我的花瓣看起來很短？** 在百分比模式下，無風讀數計入 100%。可使用 `RadialMaximum` 放大，或改用 `RadialAxis="Count"`。

**可以顯示 36 個扇區嗎？** 可以：`SectorCount="36"`；標籤顯示為度數，並會稀疏以適應空間。

## API 參考

<!-- api: WindRoseChart -->

<!-- api: WindRoseRow -->

<!-- api: WindRoseRecord -->

<!-- api: WindRoseBin -->

<!-- api: WindRoseSector -->

<!-- api: WindRoseBinning -->

## 無障礙

- 玫瑰圖是可聚焦的群組。它的自動化名稱是圖表的 `Label` 加上按鍵說明；讀取某個方向時，會播報該方向、其總計及每個風速層，例如 "SW: 14.1%. 0.5–2 m/s 1.0%, 2–4 m/s 3.0%, …"，被選取的方向則追加 ", selected"。
- 沒有讀數時，說明文字會概括無風占比以及每個方向的總計。
- 風速區間在圖例、提示和讀數中都有名稱，並且預設顏色依明度排序，因此顏色不是唯一的區分依據。每個圖例項目都是一個 Tab 停駐點，以其區間及占讀數的比例命名。

### 鍵盤

| 按鍵 | 動作 |
|---|---|
| `Tab` | 聚焦玫瑰圖並讀取選取的方向（若無則讀取第一個），然後依序經過圖例項目。取得焦點的圖例項目會反白其風速區間 |
| `Right` / `Down` | 讀取螢幕上順時針方向的下一個方向 |
| `Left` / `Up` | 讀取螢幕上逆時針方向的下一個方向 |
| `Home` / `End` | 讀取第一個或最後一個方向 |
| `Enter` / `Space` | 選取正在讀取的方向（觸發 `ItemInvoked`；啟用 `IsSelectionEnabled` 時同時選取） |
| `Escape` | 清除讀數；若沒有讀數，則清除選取 |

## 動態效果

- 首次顯示時，各色帶從中心向外生長，並在輕微回彈的彈簧上定位。
- 新資料、新的刻度模式或新的扇區數會使每個色帶彈至新長度；環隨新的最大值移動。扇區數變化時，每片新花瓣從覆蓋其方向的舊花瓣處生長出來。
- 讀取某個方向時，其他花瓣變淡；提示在各方向之間滑動。
- 減少動態效果時，變化立即呈現；首次顯示為短暫淡入。

| 變化 | 完整 | 減少 | 關閉 |
|---|---|---|---|
| 首次顯示 | 色帶從中心生長 | 短暫淡入 | 立即 |
| 資料或刻度變化 | 色帶與環彈動 | 立即 | 立即 |
| 讀數移動 | 花瓣變淡，提示滑動 | 立即 | 立即 |

## 響應式

- 玫瑰圖會縮小以符合取得的寬度與高度，且不會超過 `Size`。
- 寬度低於 360 時，方向標籤環收緊。在任何尺寸下，若方向標籤會互相碰撞，它們會稀疏為每兩個、每四個或每八個方向顯示一個；四個基本方位始終保留，正在讀取的方向也始終顯示。
- 環標籤在環靠得太近時會稀疏；無風圓太小、放不下時，中心文字會隱藏。
- 圖例會換到更多列；提示始終留在圖表之內，有空間時位於花瓣旁邊。
- 觸控螢幕按下時讀取手指所在的方向，拖曳時掃過其他方向，輕觸則選取它。

## 相關元件

| 元件 | 適用情境 |
|---|---|
| `RadarChart` | 每個方向或度量只有一個數值 |
| `BarChart` | 方向之間需要精確比較，或沒有環狀順序 |
| `Sunburst` | 占比是階層巢狀的，而不是分布在一圈上 |
| `LineChart` | 風速或風向隨時間變化 |
| `ActivityHeatmap` | 兩個循環維度，例如一天中的小時與星期 |
