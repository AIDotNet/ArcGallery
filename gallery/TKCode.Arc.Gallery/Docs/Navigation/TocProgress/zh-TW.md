---
title: Table of contents progress
summary: 長文章的目錄：一條線隨標題層級向內折入，隨著閱讀逐段填滿，並標出目前章節；標題旁顯示剩餘的閱讀分鐘數，選擇一個標題文章就會平滑捲動過去。
source-hash: 39920a64ff77
---

<Demo name="Preview" />

## 何時使用

`TocProgress` 放在長文章、指南或條款頁面旁邊，跟上讀者的進度。一條細線沿著標題清單向下延伸，在層級變化處用一段短短的 S 形彎折進入該層級的縮排。已經讀過的部分會一節一節地用強調色填滿，一個小標記跟在填充的末端：它到達某個標題時，這個標題就會亮起，上面的標題顯示為已讀，下面的保持柔和。標題旁邊顯示剩餘的閱讀分鐘數，讀完時變為 Finished。選擇一個標題，文章就會平滑捲動過去，這條線也會一路描到那裡。

- **文件和指南**：人們在章節之間跳轉，並想知道自己在哪裡。
- **技術文章和長篇閱讀**：知道還剩多少，讀者更願意讀完。
- **在自己的捲動容器裡的文章**：說明面板、閱讀檢視或對話框。

## 何時不用

- **標題少於四個的短頁面**：一個普通的連結清單就夠了。
- **章節是彼此替代的選項**，而不是同一篇文件的各個部分：使用 `Tabs`。
- **讀者必須按順序完成的步驟**：使用 `Stepper`，它顯示完成情況，而不是位置。
- **只需要一個整體進度提示**而不需要導覽：顯示一個普通的 `Progress` 進度條。

## 用法

`TocProgress` 位於 Special 套件中：在 `UseArc()` 旁邊用 `UseArcSpecial()` 註冊它。把 `Target` 指向文章的 `ScrollViewer`，並用 `TocProgress.Heading` 標記其中的標題（子章節再加上 `TocProgress.Level` 2 或 3）。清單按文件順序收集這些標記過的元素；層級每加深一級，線就向右移一條通道。

```xml
<Grid ColumnDefinitions="*,224" ColumnSpacing="16" Height="400">
    <ScrollViewer x:Name="Article">
        <StackPanel Spacing="12">
            <TextBlock Text="Install" arc:TocProgress.Heading="Install" FontSize="17" />
            <TextBlock Text="Add the package to your project." TextWrapping="Wrap" />
            <TextBlock Text="Options" arc:TocProgress.Heading="Options" arc:TocProgress.Level="2" FontSize="15" />
            <TextBlock Text="Every option has a sensible default." TextWrapping="Wrap" />
        </StackPanel>
    </ScrollViewer>
    <arc:TocProgress Grid.Column="1" Target="{Binding #Article}" ReadingOffset="28" ReadingTime="5" />
</Grid>
```

標題越過閱讀線時算作已到達。把 `ReadingOffset` 設為固定頁首的高度再加一點留白（16 到 32px）；選擇標題時，文章會正好捲動到這條線。不設定時，閱讀線位於檢視區向下四分之一處（`ActivationRatio`），選擇的標題停在頂部下方 `ScrollMargin` 處。接近文章末尾時，閱讀線會緩緩移到檢視區底部，所以捲動到底時，較短的最後幾節也能被到達。

`ActiveIndex` 和 `ActiveChanged` 事件回報標記所在的章節（範例用它來更新底部的文字）；`Progress` 是文章已捲動的比例。`ReadingTime` 設定整篇文章的分鐘數；不設定時，控制項按每分鐘 230 個詞統計文章中的詞數來估算。把 `ShowsTimeLeft` 設為 false 可以改為顯示已讀百分比（`ShowsPercent`）；當清單中的文字應與標題不同時，使用 `TocProgressEntry` 子元素。

### 精簡

`DisplayMode` 為 Compact 時顯示一個膠囊按鈕，裡面有進度環和目前章節的標題；點擊後在彈出層中開啟同一個清單。預設值 Auto 會在檢視區窄於 `ArcViewport.CompactWidth`（640px）時切換到它；把 `DisplayMode` 設為 List 可以保留清單，例如像第一個範例那樣在手機上放在文章上方。

<Demo name="Compact" />

### 結構

1. **頁首**：13px 中等字重的標題，和 12px 柔和色等寬數字顯示的剩餘分鐘數；讀完時顯示一個勾號和 Finished。
2. **線**：1.5px 的軌道，每個層級一條通道，間隔 12px，兩個層級相接處有 14px 高的 S 形彎折；已讀部分用強調色，寬 2px，末端是一個 7px 的標記，周圍留出 2px 的空隙。
3. **列**：13px 的標題，內距 4px，第一層級縮排 22px，每深一級再加 12px。未讀的標題柔和，已讀的為次要文字色，目前標題為完整文字色；較長的標題會換行。

### 內容建議

- 大多數文章保持兩個層級，最多三個；每一列都切換層級會讓線變成鋸齒狀。
- 標題盡量簡短；它們會換行，線會跟隨每一列的高度。
- 已知閱讀時間時傳入 `ReadingTime`；估算會統計文章中的每一個詞，包括程式碼。

## API 參考

<!-- api: TocProgress -->

<!-- api: TocProgressEntry -->

<!-- api: TocProgressLabels -->

<!-- api: TocProgressPosition -->

<!-- api: TocProgressActiveChangedEventArgs -->

## 無障礙

- 控制項是一個導覽區域，以它的標題 "On this page"（`Labels.Title`）命名。清單是一組連結：每一列以標題命名，目前列的說明文字為 "current section"，已讀的列為 "read"。已讀、目前和未讀的列在文字對比度上也有區別，從不只靠這條線來區分。
- 線、填充和標記只是繪製效果；剩餘時間是普通文字，不會在每次變化時朗讀。
- 每一列都在 Tab 順序中。鍵盤焦點顯示一層淡淡的底色和共用的 2px 焦點環；指標焦點兩者都不顯示。
- 精簡按鈕以目前章節和百分比命名，例如 "Contents, Results, 42% read"。

### 鍵盤

| 按鍵 | 作用 |
|---|---|
| Tab | 在各列之間移動，每一列都是一個連結 |
| Up / Down | 移到上一列 / 下一列 |
| Home / End | 移到第一列 / 最後一列 |
| Enter / Space | 把文章捲動到該章節 |
| Escape | 關閉精簡彈出層並回到它的按鈕 |

## 動效

- 閱讀位置是章節序號加上讀到該章節的程度，從該章節標題越過閱讀線開始，到下一個標題越過閱讀線為止。它透過一個臨界阻尼彈簧跟隨捲動（約 100ms 走完一半，約 0.4s 停穩），所以滾輪捲動一格時是平滑滑動，標記也不會越過目前章節。
- 每一節填滿從它的標題到下一個標題之間的那段線，包括彎折處，所以填充總是在標題被到達的那一刻正好抵達它。
- 選擇的標題用平滑彈簧滑到位，線會一路描過中間的每一節；任何滾輪或捲軸操作都會立即接管。如果文章在標題到達閱讀線之前就結束了，它會保持為目前章節，直到讀者繼續捲動。
- 列的顏色在 240ms 內變化。剩餘分鐘數捲動到每個新值，伴隨 8px 的位移和短暫的模糊，最後變為 Finished。
- 減少動態效果時，填充和標記直接跟隨捲動，選擇的標題直接跳到位，剩餘時間以淡入淡出切換。關閉動效時一切立即變化。

## 響應式

- 清單佔用給它的寬度，標題會換行；線會跟隨每一列量測出的高度，所以換行的標題之間仍保留彎折。
- 清單、文章或其捲動檢視改變大小時，列、線和標題位置都會重新量測。
- 列佔滿清單的整個寬度，便於觸控。在窄螢幕上把清單放在文章上方，或者讓 `DisplayMode` 為 Auto 時收成精簡按鈕。

## 相關元件

- `Progress`：普通的進度條。
- `Stepper`：使用者按順序完成的步驟進度。
- `ScrollArea`：帶 Arc 捲軸的捲動容器，用來放文章。
- `Tabs`：在檢視之間切換，而不是在同一頁內移動。
