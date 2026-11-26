---
title: 樹狀選擇
summary: 在下拉樹中選擇一個節點或勾選多個節點，搜尋時保留每個符合項目的路徑，勾選父節點即勾選整個分支。
source-hash: 0a05b587c5d3
---

<Demo name="Preview" />

## 何時使用

- **檔案或資料夾**：用於審查、上傳目標或權限規則。
- **部門、團隊和人員**放在一個欄位裡，勾選團隊即選取其中所有人。
- **任意深度的分類**：使用者輸入關鍵字尋找節點，而不是一層一層展開。

## 何時不用

- **簡短的扁平清單**：使用 `Select` 或 `MultiSelect`。
- **固定 2 到 5 層、只有一個答案、需要看到同層項目**：使用 `Cascader`。
- **樹本身就是頁面的主要內容**：使用 `TreeView`。
- **按需從伺服器載入子節點**：本版本不支援。

## 用法

`TreeSelect` 衍生自 Avalonia 的 `TemplatedControl`。把根節點交給 `ItemsSource`，並用委派讀取每個節點，形式與 `Cascader`、`TreeView` 相同。不使用反射，因此支援 AOT 與修剪。

```csharp
public sealed record FileNode(string Path, string Name, IReadOnlyList<FileNode>? Children = null, bool IsLocked = false)
{
    public bool IsFolder => Children is not null;
}

treeSelect.ItemsSource = files;
treeSelect.ChildrenSelector = node => ((FileNode)node).Children;
treeSelect.TextSelector = node => ((FileNode)node).Name;
treeSelect.ValueSelector = node => ((FileNode)node).Path;   // unique across the tree
treeSelect.IconSelector = node => ((FileNode)node).IsFolder ? ArcIcons.Folder : ArcIcons.File;
treeSelect.DisabledSelector = node => ((FileNode)node).IsLocked;
```

### 選擇一個節點

`SelectionMode="Single"`（預設）選取一個節點後關閉。`SelectedItem` 與 `SelectedValue` 都是雙向的；`ShowFullPath` 顯示以 `PathSeparator` 連接的完整路徑。

<Demo name="Single" />

### 只選葉節點

`LeafOnly="True"`：點選父節點只會展開或收合它。

<Demo name="LeafOnly" />

### 勾選多個節點

`SelectionMode="Multiple"` 為每一列加上核取方塊，選單保持開啟。勾選父節點會勾選其所有可用的子孫；部分勾選的父節點顯示短橫線。停用節點保持原狀態。`CheckStrategy` 決定 `SelectedItems` 與 `SelectedValues` 回報哪些節點：

| `CheckStrategy` | 回報 |
|---|---|
| `Leaves`（預設） | 已勾選的葉節點 |
| `Parents` | 以全選的父節點代替其整個子樹 |
| `All` | 所有全選的節點 |

<Demo name="CheckStrategy" />

`SelectedValues` 是雙向的：設定它會勾選對應節點（父節點的值會勾選其可用的子孫），之後它保存依策略回報的結果。資料到達之前設定的值會在資料到達時生效。選取的節點顯示為標籤，超過 `MaxVisibleChips`（3）時摺疊為 "+N"。

### 搜尋

`IsSearchable`（預設 true）：在開啟的欄位中輸入即可篩選樹。符合項目保留並展開其祖先，符合的文字會被標示；符合的父節點保留其子節點。清除搜尋後恢復原本的展開狀態。`FilterPredicate` 可取代預設的不區分大小寫的包含比對。`SearchText` 是輸入的文字（雙向）。

### 大型樹

列是虛擬化的：只建立螢幕上的列。`ExpandDepth` 設定資料到達時展開的層數（`-1` 全部展開）。

<Demo name="LargeTree" />

### 選單寬度與長選項

選單不必與欄位等寬。它的寬度取最寬一列所需的寬度；欄位寬度或 `MenuMinWidth` 較大時取較大者，再受 `MenuMaxWidth` 與視窗寬度（視窗寬度減去兩側各 16 px）限制。因此較窄的欄位也能開啟足夠寬的選單來完整顯示最長的名稱，欄位本身的尺寸不變。

- 最寬一列只在前 200 個選項中測量，包含列內距、圖示、附註文字與勾選標記的預留空間，以及縮排、展開箭頭與核取方塊。更長清單的其餘部分不會再撐寬選單。樹狀主題規定的彈出層最小寬度仍然作為下限。
- 選單開啟時、開啟期間選項變更時、字型、`MenuMinWidth` 或 `MenuMaxWidth` 改變時，以及欄位調整大小時，都會重新計算寬度。
- `MenuMinWidth`（預設 0）是下限，`MenuMaxWidth`（預設不限）是上限。
- `ShowTrimmedOptionToolTip`（預設 `True`）：當選項的文字因 `MenuMaxWidth` 或視窗而仍被截斷時，滑鼠停留會在工具提示中顯示完整文字。放得下的選項不顯示工具提示。設為 `False` 即可關閉。

當選單不應蓋住一直留在螢幕上的內容（例如側邊欄），或不希望某個特別長的選項把選單撐滿整個視窗時，請設定 `MenuMaxWidth`。比上限更寬的選項會以省略號截斷，完整文字保留在工具提示中。

```xml
<StackPanel Width="360" Spacing="16">
    <arc:TreeSelect Width="220" Placeholder="Choose a folder" MenuMinWidth="320" />
    <arc:TreeSelect Width="220" Placeholder="Choose a folder" MenuMaxWidth="280" ShowTrimmedOptionToolTip="False" />
</StackPanel>
```

## API 參考

<!-- api: TreeSelect -->

<!-- api: TreeSelectSelectionChangedEventArgs -->

<!-- api: TreeSelectMode -->

<!-- api: TreeSelectCheckStrategy -->

## 協助工具

- 欄位是可展開、收合的下拉方塊；`Label` 是名稱，`Description` 是說明文字。它的值是 `SelectedText`（單選）或「已選 N 項」（多選）。
- 列是樹狀項目，支援展開 / 收合、選取，多選時還有三態切換。
- 移動醒目提示時會朗讀節點、層級和勾選狀態。
- 「沒有符合的項目」、「清除選擇」和朗讀內容跟隨應用程式語言（en-US、zh-CN、zh-TW）。

### 鍵盤

| 按鍵 | 時機 | 操作 |
|---|---|---|
| Enter、Space、Down、Up | 關閉時 | 開啟並定位到選取項目 |
| 任意字元 | 可搜尋時 | 搜尋（開啟選單） |
| Delete、Backspace | 關閉且有選取項目時 | 清除 |
| Up、Down、Home、End、Page Up、Page Down | 開啟時 | 移動醒目提示 |
| Right | 開啟時 | 展開父節點，或移到其第一個子節點 |
| Left | 開啟時 | 收合父節點，或移到父節點 |
| Enter | 開啟時 | 選取（單選）或切換勾選（多選） |
| Space | 開啟且搜尋為空時 | 選取（單選）或切換勾選（多選） |
| Backspace | 開啟、多選且搜尋為空時 | 刪除最後一個標籤 |
| Escape | 開啟時 | 先清除搜尋，再關閉 |

## 動態效果

- 選單的開啟與關閉與 `Cascader` 相同；標籤的進出與 `MultiSelect` 相同。
- 父節點的列依序展開、展開箭頭旋轉，與 `TreeView` 相同；勾選標記自行繪製並變形為短橫線，與 `Checkbox` 相同。醒目提示在列之間滑動。
- 減少動態效果時展開箭頭不旋轉，列和勾選狀態立即變化。搜尋結果一律立即出現。

## 響應式

- 沒有中斷點。欄位佔用給定的寬度（至少 200 px）。
- 選單寬度不小於欄位，且至少 240 px，會依最寬的列擴展（受 `MenuMaxWidth` 與視窗限制）；仍然放不下的列以省略號截斷標籤，並在工具提示中顯示。
- 樹在 `MaxDropDownHeight`（320 px）內捲動。下方空間不足時選單翻到欄位上方。

## 相關元件

- `Cascader`：逐層選擇一個節點。
- `MultiSelect`：從扁平清單中選擇多個值。
- `TreeView`：頁面上的樹。
