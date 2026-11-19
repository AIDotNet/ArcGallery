---
title: 功能區
summary: "Office 式功能區：索引標籤下是由大、中、小三種命令組成的群組，視窗變窄時逐級縮小並摺疊為下拉；支援切換、下拉和分割按鈕、附說明的工具提示、可摺疊的功能區和快速存取工具列。"
source-hash: 3bb67182c265
---

<Demo name="Preview" />

## 何時使用

- 命令很多的桌面應用程式：建模與流程圖編輯器、文件與試算表編輯器、CAD 和設計工具。
- 命令可以依幾個工作領域劃分（常用、插入、檢視），每個領域一個索引標籤，索引標籤裡是具名的群組。
- 視窗尺寸變化較大：功能區透過縮小和摺疊群組讓所有命令都保持可用，而不是把它們截掉。

## 何時不用

- 只為一個檢視提供少量操作：用一排 `Button` 或 `ButtonGroup`。
- 出現在選取內容旁邊、針對選取內容的操作：用 `ContextMenu` 或 Selection toolbar 區塊。
- 依名稱尋找並執行任意命令：用 Command palette 區塊（功能區和命令面板可以搭配使用）。
- 在同一內容的不同檢視之間切換：用 `Tabs`。

## 用法

加入套件並在 `UseArc` 中註冊：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcRibbon());
```

`Ribbon` 包含若干 `RibbonTab`，每個索引標籤包含若干 `RibbonGroup`，每個群組包含命令。命令有 `Label`、`Icon`（24x24 的 Lucide 幾何圖形，例如 `{x:Static arc:ArcIcons.Copy}`，也可以是任意控制項），並像一般按鈕一樣執行 `Command` 和 `Click`：

```xml
<arc:Ribbon>
    <arc:RibbonTab Header="Home">
        <arc:RibbonGroup Header="Clipboard" Icon="{x:Static arc:ArcIcons.ClipboardPaste}">
            <arc:RibbonButton Label="Paste" Icon="{x:Static arc:ArcIcons.ClipboardPaste}" Command="{Binding PasteCommand}" />
            <arc:RibbonButton Label="Copy" Icon="{x:Static arc:ArcIcons.Copy}" Size="Medium" Command="{Binding CopyCommand}" />
        </arc:RibbonGroup>
    </arc:RibbonTab>
</arc:Ribbon>
```

`ItemInvoked` 回報使用者執行的每個命令，不論來自功能區、摺疊的群組還是快速存取工具列；`Item` 是按鈕，命令來自選單時 `Entry` 是選單列。`SelectionChanged` 回報選取的索引標籤（`SelectedIndex`、`SelectedTab`）。`LeftContent` 和 `RightContent` 在索引標籤列兩端放置應用程式層級的內容，例如「檔案」選單或搜尋框。

### 尺寸

`Size` 決定命令的版面：`Large`（大圖示在上、名稱在下，佔滿群組高度；預設值）、`Medium`（小圖示在名稱左側）或 `Small`（只有圖示，名稱移到工具提示裡）。中、小命令每欄最多疊放三個。`RibbonSeparator` 畫一條分隔線並開始新的一欄。其他控制項（例如 Arc 的 `Select` 或 `NumberField`）依中號命令疊放。

<Demo name="Sizes" />

### 命令類型

`RibbonToggleButton` 保持開或關（`IsChecked`、`IsCheckedChanged`），開啟時以強調色填滿。`RibbonDropDownButton` 不執行命令，而是開啟一個由 `MenuEntry` 列組成的選單（支援分隔線和子選單）；`ItemsSource` 和 `ItemTemplate` 可以依資料產生選單列。`RibbonSplitButton` 兩者兼具：圖示部分執行預設操作，箭頭部分開啟其他選項。帶選單的命令都有一個向下箭頭。`Description` 和 `Shortcut` 在工具提示中名稱的下方加上一句說明和快速鍵。

<Demo name="CommandKinds" />

### 自適應群組

功能區比群組窄時，群組逐級縮小：大命令先變成中號，然後全部變成小號，最後整個群組摺疊成一個按鈕，點按後在下拉中顯示完整的群組。每個命令最小只縮到它的 `MinSize`，不會讓群組變窄的那一級會被略過。`ReductionPriority` 較高的群組先縮到底；優先順序相同的群組從右往左輪流縮小。`CanCollapse="False"` 讓群組不進入最後一級。版面只取決於寬度，所以把視窗拉寬後會恢復成完全相同的版面。`IsCollapsed` 和 `IsDropDownOpen` 說明群組目前的顯示狀態。

<Demo name="Adaptive" />

`IsLauncherVisible` 在群組標題末端加一個小按鈕，用來開啟該群組的完整對話方塊；處理 `LauncherClick` 或設定 `LauncherCommand`。

### 快速存取工具列

`QuickAccessToolBar` 讓幾個命令始終可見，不論選取哪個索引標籤。把它放在應用程式需要的位置（功能區上方或下方，或透過 `TitleBar.LeftContent` 放進 `ArcWindow` 的標題列），並用 `Ribbon` 連結功能區。它的 `Items` 是工具列自己的命令，以小號顯示。設定了 `QuickAccessKey` 的功能區命令可以釘選到工具列：在該命令上按右鍵，或使用工具列末端的向下箭頭。`Keys` 依序列出已釘選命令的鍵；儲存它即可記住使用者的選擇，啟動時再填回去。釘選的命令顯示為一個小號副本，它執行功能區中的原命令（開啟其選單、切換其開關），並跟隨原命令的圖示和啟用狀態。

<Demo name="QuickAccess" />

```csharp
// Restore the user's choice, and save it when it changes.
quickAccess.Keys.AddRange(settings.QuickAccessKeys);
quickAccess.Keys.CollectionChanged += (_, _) => settings.QuickAccessKeys = quickAccess.Keys.ToArray();
```

### 摺疊的功能區

`IsMinimized` 把功能區摺疊為只有索引標籤列。此時點按索引標籤會在內容上方開啟它的群組；執行命令、在外部點按或按 Escape 會關閉它們。使用者可以用索引標籤列末端的按鈕、Ctrl+F1 或按兩下索引標籤來摺疊或固定功能區。

<Demo name="Minimized" />

### 內容相關索引標籤

`IsContextual` 把索引標籤標記為屬於目前選取內容的工具索引標籤（表格設計、節點工具），它的標題以強調色顯示。用 `IsVisible` 顯示或隱藏它：選取的索引標籤被隱藏時，功能區會選取第一個可見的索引標籤。

<Demo name="Contextual" />

## API 參考

<!-- api: Ribbon -->

<!-- api: RibbonTab -->

<!-- api: RibbonGroup -->

<!-- api: RibbonButton -->

<!-- api: RibbonToggleButton -->

<!-- api: RibbonDropDownButton -->

<!-- api: RibbonSplitButton -->

<!-- api: RibbonSeparator -->

<!-- api: QuickAccessToolBar -->

## 無障礙

- 索引標籤列只佔一個 Tab 停駐點：選取的索引標籤可以用 Tab 鍵聚焦，其他索引標籤用方向鍵到達。螢幕閱讀器把每個標題讀作索引標籤。
- 每個命令的 `Label` 就是它的無障礙名稱，只顯示圖示時也一樣。切換按鈕會回報開或關的狀態；群組的啟動器按鈕名為「<標題>選項」。
- 摺疊群組的按鈕帶有群組名稱，並在下拉中開啟該群組；Escape 關閉下拉並把焦點還給按鈕。
- 工具提示重複名稱、快速鍵和說明，所以小號命令對視力正常的使用者也不只是一個圖示。

### 鍵盤

| 按鍵 | 位置 | 操作 |
|---|---|---|
| ArrowLeft / ArrowRight | 索引標籤列 | 選取上一個或下一個索引標籤（略過隱藏和停用的索引標籤）。 |
| Home / End | 索引標籤列 | 選取第一個或最後一個索引標籤。 |
| ArrowDown | 索引標籤列 | 進入選取索引標籤的命令（功能區摺疊時先把它們開啟）。 |
| Enter / Space | 索引標籤列 | 功能區摺疊時，開啟選取索引標籤的命令。 |
| ArrowLeft / ArrowRight | 命令 | 移到上一個或下一個命令。 |
| Escape | 命令 | 回到選取的索引標籤；在下拉中則關閉下拉。 |
| Enter / Space | 命令 | 執行命令；在下拉按鈕上開啟選單並醒目提示第一列。 |
| ArrowDown / F4 | 下拉、分割或摺疊群組按鈕 | 開啟選單或下拉。 |
| Ctrl+F1 | 功能區內任意位置 | 把功能區摺疊為索引標籤列，或重新固定展開。 |
| Shift+F10 / Menu | 設定了 `QuickAccessKey` 的命令 | 提供新增至快速存取工具列或從中移除的選項。 |

## 動效

- 強調色底線以彈簧從舊索引標籤滑到新索引標籤。
- 新索引標籤的群組從移動方向一側滑入 8px 並淡入（160 ms）。減少動效時只淡入；關閉動效時直接出現。
- 命令的填色和文字顏色一步完成變化；選單、下拉和摺疊功能區的彈出層使用 Arc 的彈出層動效開啟和關閉。

## 回應式行為

- 群組逐級縮小以符合寬度（見「自適應群組」）；所有命令都保持可用，必要時透過摺疊群組的下拉存取。
- 索引標籤列放不下的索引標籤會在列尾被截掉；請保持索引標籤名稱簡短，或在小視窗中用 `IsMinimized` 摺疊功能區。
- 功能區針對桌面上的滑鼠和鍵盤設計。在手機上請改用底部工具列或選單。

## 相關

- `Tabs`：在同一內容的不同檢視之間切換。
- `ButtonGroup` 和 `SplitButton`：不需要功能區時的幾個相關操作。
- `DropdownMenu` 和 `ContextMenu`：功能區按鈕開啟的選單。
- `DockLayout`（`TKCode.Arc.Dock`）：IDE 式應用程式中功能區下方的窗格和文件。
