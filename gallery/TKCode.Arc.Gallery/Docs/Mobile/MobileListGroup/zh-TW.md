---
title: 移動端清單
summary: 分組的列，用於設定和選單：圖示、標題、副標題、列尾的值或控制項，以及箭頭。
source-hash: 5f0bbe05de52
---

<Demo name="Basic" />

## 何時使用

`ListGroup` 和 `ListItem` 排出一屏的列：設定、選單和帳戶頁，每一列都是可以點按的標題。

- **設定**：把相關的列放在一個簡短的 `Header` 下，用 `Footer` 作說明。
- **目的地選單**：帶 `Icon` 和箭頭、會開啟其他頁面的列。
- **列尾的值或開關**：放進 `Trailing`。
- **列首的人或帳戶**：把頭像放進 `Leading`。
- **不是操作的內容**：QR 碼、一段文字或自帶的按鈕放進 `ListCell`，這種列不回應按壓。

即時範例在手機外框裡執行，那是桌面上的模擬；真機上的觸控回饋這裡沒有驗證。

## 何時不用

- **很長或無限的資料**：應該用虛擬化清單，這個還沒有做。
- **帶圖片和多個操作的卡片**：自己排版。
- **桌面選單**：用桌面的選單控制項；這裡的列沒有懸停狀態。

## 用法

把 `ListItem` 放進 `ListGroup`，再把組放進可捲動的 `Page`。項的內容就是它的標題。`ListItem` 是一個按鈕，給它 `Command` 或處理 `Click`；手指按在列上時，列會變暗。

```xml
<arcm:Page xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
           xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
           IsScrollable="True">
    <arcm:ListGroup Header="帳戶">
        <arcm:ListItem Icon="{x:Static arc:ArcIcons.User}" Content="個人資料" Subtitle="Ada Lovelace" ShowChevron="True" />
        <arcm:ListItem Content="每週摘要" Trailing="週日" ShowChevron="True" />
    </arcm:ListGroup>
</arcm:Page>
```

一組的最後一列下面沒有分隔線。

### 內容儲存格

`ListCell` 是放任意內容、不回應按壓的列：沒有按壓變暗，沒有懸停，本身也不取得焦點（裡面的內容可以）。它的外框和 `ListItem` 相同：同樣 48 px 的最小高度、兩側內距和分隔線，所以儲存格和列項可以混在同一組裡，組裡最後一列（不論是儲存格還是列項）下面沒有分隔線。用它放 QR 碼、一段文字、自帶的按鈕，或者在列之間放一個 `MobileRadioCards` 網格。

<Demo name="Cells" />

```xml
<arcm:ListGroup Header="邀請碼">
    <arcm:ListCell HorizontalContentAlignment="Center">
        <Image Source="{Binding QrCode}" Width="{arc:ArcDesignTokenResource MediaSizeMD}" />
    </arcm:ListCell>
    <arcm:ListCell>
        <arcm:Button Content="複製邀請碼" HorizontalAlignment="Left" />
    </arcm:ListCell>
    <arcm:ListItem Content="透過電子郵件邀請" ShowChevron="True" />
</arcm:ListGroup>
```

直接寫在組裡、或作為 `ItemTemplate` 根元素傳回的 `ListCell` 就是這一列本身，不會再被包進 `ListItem`。

### 前置內容

`Leading` 接受標題前的任意內容，例如頭像。它和 `Icon` 共用第一欄（兩者都設定時取代圖示），所以標題和 `Subtitle` 一起留在它旁邊。前置內容保持自己的尺寸，列會長高以容納它。

<Demo name="Leading" />

```xml
<arcm:ListItem Content="Ada Lovelace" Subtitle="帳戶、裝置與登入" ShowChevron="True">
    <arcm:ListItem.Leading>
        <Border Width="{arcm:ListItemTokenResource AvatarSizeXL}"
                Height="{arcm:ListItemTokenResource AvatarSizeXL}"
                CornerRadius="{arc:ArcDesignTokenResource RadiusPill}"
                Background="{arc:ArcDesignTokenResource AccentSubtle}" />
    </arcm:ListItem.Leading>
</arcm:ListItem>
```

頭像尺寸是 `ListItemToken` 的 token，既用於前置插槽，也用於頁面上其他位置的個人資料列和頁首：

| Token | 尺寸 | 用途 |
|---|---|---|
| `AvatarSizeSM` | 32 | 緊湊的列、留言 |
| `AvatarSizeMD` | 40 | 聯絡人或帳戶列 |
| `AvatarSizeLG` | 48 | 以人為主的列 |
| `AvatarSizeXL` | 64 | 設定頁頂部的個人資料列 |
| `AvatarSize2XL` | 88 | 個人資料頁首 |

### Token

`ListItemToken` 保存列的幾何：`RowHeight`（48）、`ContentPadding`（16 乘 10）、`IconExtent`（22）、`IconGap`（12）、`TrailingMargin`、`ChevronExtent`（16）、`SeparatorInset`（16）、`LeadingMargin`（圖示或前置內容之後的 12）以及上面的頭像尺寸。`ListCellToken` 保存儲存格的幾何：`RowHeight`（48）、`ContentPadding`（16 乘 12）和 `SeparatorInset`（16）。`ListGroupToken` 保存組的幾何：`GroupPadding`（16 乘 8）、`CardBorderThickness`（1）、`HeaderMargin` 和 `FooterMargin`。顏色、圓角和字型來自 ArcDesign token。

## API 參考

<!-- api: MobileListGroup -->

<!-- api: MobileListItem -->

<!-- api: MobileListCell -->

## 無障礙

一列是一個按鈕，名稱來自標題和副標題。每一列都要有標題，圖示或頭像不能是它唯一的名稱。每一列至少 48 px 高，高於 44 px 的觸控目標。`ListCell` 不是按鈕，也不取得焦點；它裡面的控制項（按鈕、單選卡片）各自取得焦點、各自命名。

## 動效

手指按在列上時，背景按快速曲線變暗；減弱動效時，立即變暗。`ListCell` 沒有自己的動效：它不回應按壓。

## 響應式

- `ListGroup` 沒有斷點。它是一張佔滿寬度的卡片，周圍有 `GroupPadding`（16 x 8 px），`Header` 與 `Footer` 換行（`TextWrapping="Wrap"`），而不會被截斷。
- 每個 `ListItem` 列至少 48 px 高，副標題以省略號（`CharacterEllipsis`）截成一行。
- 列末的內容佔用標題之外剩下的空間，所以過長的列末值會把標題擠短。請保持簡短。
- 前置內容保持自己的尺寸，列會長高以容納它；40 px 的頭像讓列變成 60 px。`ListCell` 隨內容長高，所以其中的長文字應換行，而不是一直延伸。
- 手指按下時以淡色底面標示該列，沒有任何列依賴懸停。輕觸該列即可對它執行操作。

## 相關元件

- `MobilePage`：可捲動的頁面承載這些組。
- `MobileSwitch`：設定項常用的 `Trailing` 控制項。
- `MobileNavbar`：頂列，根設定頁可用大標題。
- `MobilePicker`：顯示目前值、在動作表裡開啟選項的列。
- `MobileRadioCards`：可以放進 `ListCell` 的卡片網格。
