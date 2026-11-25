---
title: 差異編輯器
summary: "像 Monaco 的差異編輯器那樣比較兩段文字：並排或內嵌顯示，標示變更的行與字元，標出移動的程式碼區塊，摺疊未變更的區域，並可用箭頭還原某處變更。"
source-hash: 1801441c330b
---

<Demo name="Preview" />

左側是原始文字，右側是修改後的文字。變更的行帶底色，行內變更的字元顏色更深，捲動時兩側的行保持對齊。編輯右側，差異會隨之更新。

## 何時使用

- 在儲存、提交或套用之前檢閱一處變更：產生的編輯、合併結果、設定遷移。
- 呈現一個檔案、一條查詢或一份設定在兩個版本之間的變化。
- 讓使用者還原變更中的一部分：每處變更都有一個箭頭，按一下即可還原。

## 何時不用

- 只編輯一段文字、沒有可比較的版本：使用 `CodeEditor`。
- 一句話或一個值的簡短前後對照：把兩者都顯示出來，或在一般文字中標出變更的詞。
- 同時比較許多檔案：差異編輯器一次只比較一對文字。

## 用法

`DiffEditor` 與 `CodeEditor` 同在 `TKCode.Arc.Editor.Controls` 套件中，`UseArcCodeEditor()` 會同時註冊兩者：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

給它兩段文字和一種用於標示的語言。`OriginalText` 唯讀；`ModifiedText` 雙向繫結，使用者輸入的內容會回到檢視模型。

```xml
<arc:DiffEditor Height="400"
                LanguageId="csharp"
                OriginalText="{Binding SavedSource}"
                ModifiedText="{Binding Source}" />
```

差異演算法與 Monaco 相同：先比較行，再比較變更行內的字元，並偵測與連接移動過的程式碼區塊。計算在 UI 執行緒之外、根據兩段文字的快照進行，每次編輯後稍待片刻會重新計算；特別大的一對文字超過時間限制後會停止，顯示已經找到的行。每次計算完成都會引發 `DiffComputed`，帶有變更數、新增與刪除的行數，以及移動的程式碼區塊數。

```csharp
Diff.DiffComputed += (_, e) => Summary.Text = e.IsIdentical ? "No changes" : $"{e.ChangeCount} changes";
```

### 選項

<Demo name="Options" />

- `RenderSideBySide` 並排顯示兩段文字（預設開啟）；關閉後由一個編輯器顯示修改後的文字，刪除的行插在它的行之間。
- `HideUnchangedRegions` 把較長的未變更行摺疊成一條橫列，橫列上寫著隱藏了多少行，按一下即可展開（預設關閉）。
- `IgnoreTrimWhitespace` 忽略行首與行尾空白的變化，與 Monaco 的預設行為相同（預設開啟）。
- `IsReadOnly` 讓修改後的文字也不能變更；原始文字始終不能變更。

### 鍵盤

| 按鍵 | 動作 |
|---|---|
| F7 | 移至下一處變更 |
| Shift+F7 | 移至上一處變更 |

在每一側內部，`CodeEditor` 的按鍵都適用：尋找、多游標、摺疊等。

## API 參考

<!-- api: DiffEditor -->

## 無障礙

- 用 `AutomationProperties.Name` 為差異編輯器命名，說明比較的是什麼，例如「Program.cs 的變更」。
- 變更既用顏色表示，也用邊欄中的 + 與 − 號表示，不只依賴顏色。
- 兩側都是程式碼編輯器，會把文字讀給螢幕閱讀器；目前還沒有供螢幕閱讀器使用的變更清單。

## 動效

- 沒有自動播放的動效。移至某處變更時，兩側會一起捲動到那裡。

## 回應式行為

- 差異編輯器佔滿給定的寬度，並在兩側之間平分。在窄螢幕上請關閉 `RenderSideBySide`：內嵌檢視只需要一欄。
- 請為它設定高度：它會捲動內容，不會隨內容變高。

## 相關

- `CodeEditor`：編輯一段文字。
- `CodeBlock`：只顯示程式碼、不編輯。
