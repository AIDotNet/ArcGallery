---
title: 徑向選單
summary: "圍繞指標的一圈命令：按右鍵或按住即可開啟，然後點選某一項，或按住、朝它輕甩再放開。"
source-hash: e2fab1aea5d9
---

<Demo name="Preview" />

## 何時使用

徑向選單，也叫圓餅選單，會把若干命令排成一圈，圍繞它開啟的位置。每一項到指標的距離都相同，且各佔一個方向，因此經常使用此選單的人往往不再去讀它：他們按住按鈕，朝想要的命令輕甩，然後放開。這使它很適合人們整天來回切換、並且就在工作處使用的工具。

- **繪圖與設計工具**：在畫布上切換選取、筆、文字、形狀和影像，無需移動到工具列。圓環在指標處開啟，手可以留在工作的地方。
- **圖表與流程編輯器**：對指標下的項目執行新增節點、連接、註解或刪除，最常用的命令位於最容易輕甩到的方向。
- **CAD、三維與關卡編輯器**：捕捉模式、檢視和變換工具，專業使用者靠輕甩即可叫出，每分鐘常常要用幾十次。
- **媒體與遊戲工具**：對時間軸片段或場景物件執行快捷操作，或在預覽遊戲的工具中使用武器與道具轉盤。
- **精簡的「新增」按鈕**：由四到五個選項組成的扇形（照片、相機、檔案、連結、筆記），在螢幕底部的浮動按鈕上方展開。

## 何時不用

- **超過約八個命令，或命令需要分組**：使用 `ContextMenu` 或 `DropdownMenu`。項目太多時，每個扇區會過窄而難以瞄準，而且圓環上沒有空間放置分隔線、分組標題或快捷鍵提示。
- **必須讓人發現的操作**：在右鍵按下或按住時才開啟的選單，在有人嘗試之前是看不見的。重要命令也應保留在可見的工具列中，或保留帶有明確標籤觸發器的 `DropdownMenu`。
- **會提交到表單的選擇**：使用 `Select`、`RadioGroup` 或 `SegmentedControl`。它們會顯示目前的值，並參與驗證；徑向選單只負責執行命令。
- **標籤過長或彼此相似**：一個扇區只容納一兩個詞。如果命令需要整句話才能區分，請改用清單。
- **只有一兩個命令**：把它們放在內容旁邊的按鈕中。

## 用法

`RadialMenu` 衍生自 Avalonia 的 `ItemsControl`。把選單項目宣告為 `RadialMenuItem` 子元素，每一項都設定 `Header`、`Icon`，通常還設定 `Command`，然後用 `arc:RadialMenu.Menu` 把選單附加到它所屬的控制項上。預設情況下，在該控制項上按下右鍵或按住，都會在指標處開啟選單。

```xml
<Border Height="300" CornerRadius="18" Background="{arc:ArcDesignTokenResource SurfaceMuted}">
    <arc:RadialMenu.Menu>
        <arc:RadialMenu Label="Canvas tools">
            <arc:RadialMenuItem Header="Select" Icon="{x:Static arc:ArcIcons.Move}" Command="{Binding SelectToolCommand}" />
            <arc:RadialMenuItem Header="Pen" Icon="{x:Static arc:ArcIcons.Pencil}" Command="{Binding PenToolCommand}" />
            <arc:RadialMenuItem Header="Text" Icon="{x:Static arc:ArcIcons.Type}" Command="{Binding TextToolCommand}" />
            <arc:RadialMenuItem Header="Delete" Icon="{x:Static arc:ArcIcons.Trash2}" Command="{Binding DeleteCommand}" />
        </arc:RadialMenu>
    </arc:RadialMenu.Menu>
</Border>
```

附加的選單以該控制項作為邏輯父層，因此即使選單尚未開啟，其繫結也能看到控制項的 `DataContext`。每次只把一個選單附加到一個控制項上。

### 組成部分

1. **底盤**：圓環下方凸起的表面，使用凸起表面顏色，帶有邊框和浮動陰影。完整圓形時它是一個圓盤；扇形時它是一條圓角帶，加上中心處的一個小圓盤。
2. **扇區**：每個可見項目對應一個，依順時針排列，由 `SectorGap`（4 px，各處寬度相同）分隔。靜止的扇區有淡淡的填滿；醒目提示的扇區採用柔和的強調色填滿，並略微向外凸出。
3. **圖示**（`Icon`）：24 乘 24 的 Lucide 幾何圖形，以 20 px 描邊的字形繪製，也可以是任意控制項。
4. **標籤**（`Header`）：圖示下方一行 12 px 文字，扇區過窄時以省略號截斷。對於圖示圓環，可用 `IsItemLabelVisible="False"` 關閉標籤。
5. **中心**（`InnerRadius`，44 px）：顯示醒目提示項目的文字；未醒目提示任何項目時顯示 `CenterContent`。它同時也是手勢的死區。

圓環的尺寸由 `InnerRadius` 和 `OuterRadius`（124 px）決定：預設圓環連同底盤的直徑為 260 px。標籤較長或項目較多時請加大，只有四個圖示時可以縮小。

### 選取一項

有三種方式可以選取，並且可以自由組合：

- **點選**：開啟選單，然後點選一個扇區。指標經過圓環時會醒目提示指標下的扇區；按下時顯示為按下狀態，在同一扇區上放開即選取它。
- **標記手勢**：保持按住開啟選單的那個按鈕，朝一個扇區移動後放開。方向決定選取結果，即使遠遠超出圓環也有效。移回中心後放開則取消。完全沒有移動就放開是一般點選，選單保持開啟，等待對某一項目的點選。
- **鍵盤**：方向鍵或 Tab 在圓環上移動，字母鍵跳到下一個以該字母開頭的項目，Enter 或 Space 選取醒目提示的項目。

選取一項會引發 `ItemInvoked`，以 `CommandParameter` 執行該項的 `Command`，以該項為參數執行選單的 `ItemInvokedCommand`，然後關閉選單。按下 Escape、在圓環外按下，或在中心按下，都會在不選取的情況下關閉選單。

```csharp
tools.ItemInvoked += (_, e) => status.Text = $"Tool: {e.Container.Header}";
```

`SelectedItem` 是醒目提示的項目，且為雙向繫結：它會隨指標或鍵盤的移動而變化，並引發 `SelectionChanged`；選取之後，它保存被選中的項目。開啟前設定它，可以讓某一項目以醒目提示狀態開始，例如目前的工具。

### 圓形與扇形配置

`SweepAngle` 是各項目共享的弧度：預設值 360 表示完整圓形，較小的值表示扇形。`StartAngle` 指定各項目的起始位置，以從 12 點鐘方向順時針計算的度數表示。完整圓形中，第一項目置中於該方向，因此四個項目分別位於北、東、南、西；扇形中，第一項目的前緣位於該方向。`StartAngle="-90"` 且 `SweepAngle="180"` 的扇形是圓環的上半部，適合放在螢幕底部的按鈕上。

<Demo name="Sector" />

```xml
<arc:Button Variant="Primary" AutomationProperties.Name="Add to note">
    <arc:ArcGlyph Data="{x:Static arc:ArcIcons.Plus}" Size="18" />
    <arc:RadialMenu.Menu>
        <arc:RadialMenu Label="Add to note" Trigger="Click" StartAngle="-90" SweepAngle="180">
            <arc:RadialMenuItem Header="Photo" Icon="{x:Static arc:ArcIcons.Image}" />
            <arc:RadialMenuItem Header="File" Icon="{x:Static arc:ArcIcons.File}" />
            <arc:RadialMenuItem Header="Link" Icon="{x:Static arc:ArcIcons.Link}" />
        </arc:RadialMenu>
    </arc:RadialMenu.Menu>
</arc:Button>
```

各項目總是平分扇形的弧度。`IsVisible="False"` 的項目不佔用扇區，其餘項目會收攏；`IsEnabled="False"` 的項目保留其扇區，但不能被醒目提示或選取。

### 標記選單

這種手勢是徑向選單快速的原因所在。在畫布上，按住右鍵（或按住任意按鈕或手指），朝某個命令輕甩後放開：選單在按下處開啟，放開時完成選取。用了幾天之後，人們就會記住向上是複製、向左是復原，並且在看圓環之前就已完成輕甩。每次都保持項目順序相同，方向才能保持不變。

<Demo name="MarkingMenu" />

`Trigger` 指定哪些手勢會開啟附加的選單。它是一組旗標：

| Trigger | 開啟時機 |
|---|---|
| `RightPress` | 按下滑鼠右鍵時，在指標處 |
| `Hold` | 在滑鼠、觸控或觸控筆上按住 400 ms、且位移不超過 8 px 之後，在按下點 |
| `Click` | 在控制項上主按鍵點選或輕觸時（包括按鈕的 `Click`，也包括鍵盤觸發），位置依 `Placement` 的設定 |

預設值為 `RightPress, Hold`。焦點控制項上的 ContextMenu 鍵和 Shift+F10 一律會開啟選單，位於控制項中心，並醒目提示第一項。

### 命令與 MVVM

把 `ItemsSource` 繫結到檢視模型的項目集合。選單會為每一項目產生一個 `RadialMenuItem`，並以該項目作為其 `DataContext`，因此樣式可以把項目的成員對應到 `Header`、`Icon`、`Command` 和 `IsEnabled`。為樣式加上 `x:DataType`，以便編譯其繫結。下面的範例會在執行階段新增和移除工具；畫布鎖定時，刪除工具的命令無法執行，因此它的扇區會被停用。

<Demo name="Mvvm" />

```xml
<arc:RadialMenu Label="Tools" ItemsSource="{Binding Tools}" ItemInvokedCommand="{Binding RememberCommand}">
    <arc:RadialMenu.Styles>
        <Style Selector="arc|RadialMenuItem">
            <Setter Property="Header" Value="{Binding Name}" />
            <Setter Property="Icon" Value="{Binding Icon}" />
            <Setter Property="Command" Value="{Binding Command}" />
        </Style>
    </arc:RadialMenu.Styles>
</arc:RadialMenu>
```

`ItemInvokedCommand` 會在每次選取後執行，並以該項目作為參數：對於 `ItemsSource`，參數是資料項目；對於宣告的項目，參數是 `RadialMenuItem`。當一個命令處理所有項目時使用它；當每一項目各自執行不同的操作時，則使用項目自身的 `Command`。設定了 `TextSelector` 且沒有樣式時，產生的項目會以該文字作為標籤；它是一個委派，因此無需反射，也適用於修剪。

```csharp
menu.TextSelector = item => ((Tool)item!).Name;
```

### 自訂項目內容

`ItemTemplate` 會取代產生的扇區所顯示的內容，例如顏色色塊、縮圖或徽章。即使設定了 `IsItemLabelVisible="False"`，使用範本的扇區仍會顯示其範本，而 `TextSelector` 仍會為每一項目提供中心標籤和螢幕閱讀器所需的文字。

<Demo name="CustomTemplate" />

```xml
<arc:RadialMenu Label="Highlight color" Trigger="Click" IsItemLabelVisible="False">
    <arc:RadialMenu.ItemTemplate>
        <DataTemplate>
            <Ellipse Width="26" Height="26" Fill="{Binding Brush}" />
        </DataTemplate>
    </arc:RadialMenu.ItemTemplate>
</arc:RadialMenu>
```

宣告的 `RadialMenuItem` 也可以把任意控制項作為其 `Icon`，而 `HeaderTemplate` 用於設定其 `Header` 的格式。

### 從程式碼開啟與定位

`OpenAt` 以控制項座標系中的某一點為中心開啟選單，例如工具希望提供下一步操作的位置。`Open(control)` 和 `Open()` 會在某個控制項上開啟選單，或在 `PlacementTarget`（預設為附加的控制項）上開啟，位置由 `Placement` 決定：預設的 `Center` 表示控制項中心，`Pointer` 表示指標最後所在的位置。`IsOpen` 為雙向繫結，因此檢視模型可以開啟和關閉選單。`Close` 會關閉選單，並把焦點交還給原來的位置。

```csharp
menu.OpenAt(canvas, e.GetPosition(canvas)); // centred on a point
menu.Open(toolButton); // centred on a control
menu.Close();
```

選單浮動在視窗的覆蓋層上，或最近的 `ArcOverlayHost` 上。靠近該區域的邊緣時，它會移動，使整個圓環保持在區域之內，並與邊緣保持 8 px 的距離。扇形只需要自身範圍位於區域之內，因此向上開啟的半圓環可以緊貼在底邊上方。直接放在版面配置中的 `RadialMenu` 不會浮動：它會就地顯示圓環，作為靜態的錶盤。

### 內容指南

- **標籤**：一到兩個詞，使用動詞或工具名稱，句首大寫，例如：「筆」、「複製」、「新增連結」。中心會重複醒目提示項目的標籤，因此在扇區中被截斷的標籤，在中心仍能完整讀到。
- **圖示**：為每一項目都提供圖示；在圓環中，人眼找圖示比找文字更快。純圖示的圓環需要為每一項目設定 `Header` 或 `AutomationProperties.Name`。
- **順序**：把最常用的命令放在頂部，把與其相反的命令放在底部（複製與貼上、復原與重做），並在各版本之間保持順序穩定，使人們已經學會的方向繼續有效。
- **數量**：四到八項。四項和八項對應羅盤方向，最容易輕甩。

| 避免使用 | 改為 |
|---|---|
| 項目 1 | 選取 |
| 在此處建立新註解 | 註解 |
| 刪除所選物件 | 刪除 |
| 其他 | 為命令命名 |

### 最佳實務

**建議**

- 讓相同的項目始終位於相同的位置，以便人們學會各個方向。
- 為每一項目都提供圖示和簡短標籤；標籤被隱藏時，還要提供無障礙名稱。
- 在工具列或選單等可見位置，同時提供相同的命令。
- 把扇形用於靠近螢幕邊緣的按鈕，使圓環朝遠離邊緣的方向開啟。

**避免**

- 超過八項，或項目隨選取而變化。會變形的圓環無法被學會。
- 順序取決於資料的項目，例如最近檔案。
- 把破壞性命令放在最容易輕甩的方向（正上方）；應放在輕甩不會意外碰到的位置。
- 在項目範本中使用互動式控制項；選單會接管指標以進行選取。

### 常見問題

**標記手勢如何區分點選與輕甩？** 如果在放開按鈕之前，指標離開了中心（`InnerRadius`），放開時便會選取該方向的項目；如果指標已回到中心，則取消。如果指標從未離開中心，放開即為點選，選單保持開啟。

**為什麼靠近邊緣時圓環不會恰好在指標下方開啟？** 為了保持在視窗之內，它會移動位置。手勢仍然從按下的位置開始測量方向，因此輕甩的效果不變。

**徑向選單可以有子選單嗎？** 本版本不支援。圓環只保留一級命令；需要時，可從某個命令再開啟第二個選單。

**能否在畫布上用左鍵點選開啟它？** 設定 `Trigger="Click"` 和 `Placement="Pointer"`：點選或輕觸會在發生處開啟選單。

## API 參考

<!-- api: RadialMenu -->

<!-- api: RadialMenuItem -->

## 協助工具

- 選單會被朗讀為一個選單，名稱由 `Label` 指定（預設為 "徑向選單"）；每個扇區都是一個選單項目，其名稱取自 `AutomationProperties.Name`、`TextSelector` 文字或其文字 `Header`，並且可以被輔助技術叫出。停用的項目會回報其未啟用。
- 鍵盤導覽會把焦點移到醒目提示的項目上，因此螢幕閱讀器會朗讀它；指標的醒目提示不會移動焦點。醒目提示代表鍵盤位置，因此扇區不繪製焦點框。
- 選單關閉時，焦點會回到開啟之前所在的位置；透過鍵盤選取之後，獲得焦點的控制項會顯示其焦點框。
- 圖示不會被朗讀。每一項目都需要文字，即使是純圖示的圓環也一樣。
- 標記手勢只是一種捷徑，而不是唯一的方式：每一項目也都可以透過點選或鍵盤觸達。

### 鍵盤

| 按鍵 | 操作 |
|---|---|
| `ContextMenu` 鍵 / `Shift+F10` | 在焦點控制項上開啟附加的選單，並醒目提示第一項 |
| `Right` / `Down` / `Tab` | 順時針移到下一項（跳過停用和隱藏的項目，並循環） |
| `Left` / `Up` / `Shift+Tab` | 逆時針移到上一項 |
| `Home` / `End` | 第一項 / 最後一項 |
| 字母鍵 | 下一個文字以該字母開頭的項目 |
| `Enter` / `Space` | 選取醒目提示的項目 |
| `Escape` | 不選取即關閉；焦點回到原處 |

## 動態效果

- 圓環在淡入時以靈敏彈簧的節奏從中心放大，淡出時略微縮小。結束動畫完成之後才會引發 `Closed`。
- 醒目提示的扇區以靈敏彈簧的節奏從中心向外凸出，其填滿與顏色交叉淡化。
- 中心標籤以平緩的淡入淡出變化。

| 變化 | 完整 | 減少 | 關閉 |
|---|---|---|---|
| 開啟 | 以靈敏彈簧從 0.86 縮放到 1，160 ms 淡入 | 120 ms 淡入 | 立即 |
| 關閉 | 縮放到 0.94，並以 120 ms 淡出 | 120 ms 淡出 | 立即 |
| 醒目提示與按下顏色 | 160 ms 淡變 | 120 ms 淡變 | 立即 |
| 醒目提示扇區凸出 | 靈敏彈簧 | 無 | 無 |
| 中心標籤 | 標籤淡變 | 120 ms 交叉淡化 | 立即 |

## 響應式

- 圓環保持其尺寸，預設直徑為 260 px，並會移動以保持在視窗之內，因此只要視窗寬於約 280 px 即可容納。在手機尺寸的檢視上，優先使用六項或更少，或使用朝遠離最近邊緣方向開啟的扇形。
- 觸控透過按住來開啟選單；隨後手指可以輕甩並抬起以選取，或抬起後輕觸某一項目。觸控沒有懸停，因此在手指移動或輕觸之前，不會醒目提示任何項目。
- 無法放入扇區的標籤以省略號結尾，中心會顯示醒目提示項目的完整標籤。

## 相關元件

- `ContextMenu`：指標處的操作清單，支援分組、勾選和快捷鍵提示。
- `DropdownMenu`：位於可見且帶標籤觸發器之後的操作。
- `SegmentedControl`：當選取的是一個始終顯示在螢幕上的值時使用。

| 元件 | 選擇時機 |
|---|---|
| `ContextMenu` | 超過八個操作、需要分組或快捷鍵提示 |
| `DropdownMenu` | 操作需要可見的觸發器 |
| `SegmentedControl` | 選取的是一個始終可見的值 |
| `CommandPalette` | 人們透過名稱搜尋命令 |
