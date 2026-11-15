---
title: Keycap keyboard
summary: 造型立體的機械鍵帽，打字或點擊時會陷下並發光，打出的文字顯示在鍵盤上方。
source-hash: 724a80befc42
---

<Demo name="Preview" />

## 何時使用

`KeycapKeyboard` 繪製一塊機械鍵帽鍵盤，打出的文字顯示在它上方。真實打字和點擊會把對應的鍵帽按進底座，在它下面亮起一圈柔光，並把字元加到 `Value`。

- **試一試的時刻**：鍵盤優先的產品與開發者工具的著陸頁、空狀態。
- **教人認鍵**：新手引導裡展示一個快速鍵用到哪些鍵。
- **小鍵盤**：用 `Rows` 搭出幾個大鍵，打出一個詞或執行一個巨集。

## 何時不用

- **表單輸入**：請用文字方塊；這塊鍵盤是玩具，不是編輯器。
- **為無障礙提供的螢幕鍵盤**：請用系統內建的。
- **美式鍵盤以外的配置**：鍵帽按美式鍵盤擺放和標示。

## 用法

`KeycapKeyboard` 在 Special 套件中；在 `UseArcControls()` 旁邊用 `UseArcSpecial()` 註冊。不做任何設定時，它顯示一塊精簡的 60% 鍵盤，並在擁有焦點時監聽按鍵。`Tracking="Window"` 會監聽視窗裡任何地方的打字，但文字方塊裡的輸入和帶 Ctrl、Alt 或 Command 鍵的快速鍵除外。`Value` 保存文字（雙向），`ValueChanged` 回報每次變化。`MaxLength`（280）丟掉最早的字元，`Placeholder` 在文字為空時顯示。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:KeycapKeyboard Tracking="Window" Placeholder="Press any key to try it" />
</UserControl>
```

鍵帽按實體位置對應真實按鍵，所以任何鍵盤配置都會點亮正確的鍵帽；打出的字元是系統給出的那個。Delete 刪除最後一個字元，Return 另起一行，點擊 Shift 會讓下一個字元變成上檔字元，數字和符號也一樣。

### 字母

`Layout="Letters"` 顯示三列字母，加上 Shift、Delete 和空白鍵。空白鍵是強調鍵，就像精簡鍵盤上的 Return。

<Demo name="Letters" />

### 自訂列

`Rows` 用你自己的列取代配置。每一列的鍵用空格隔開：單一字元，或 `space`、`backspace`、`enter`、`shift`、`tab` 和 `caps`。自己顯示 `Value` 時，用 `ShowsText="False"` 隱藏文字區。

<Demo name="MacroPad" />

### 快速鍵

`Shortcut` 把一個組合鍵的鍵帽變成強調色，寫法如 "Ctrl+Shift+P"、"Cmd+K" 或 "⇧⌘P"。在鍵盤裡放入 `KeycapBinding`：恰好按住某個綁定的修飾鍵時（`ActiveModifiers`），它的鍵會印上標籤；有綁定時，點擊的修飾鍵會一直保持按住，直到下一個鍵。`Platform` 讓修飾鍵印上 Windows 的詞（ctrl、win、alt；預設是 ctrl、opt、cmd），`ShowsFunctionRow` 加上 Esc 和 F1 到 F12，`KeyInvoked` 回報每個按下的鍵以及它的修飾鍵和綁定。

<Demo name="Shortcut" />

### 最佳實踐

**建議**

- 在沒有其他輸入框的頁面上使用視窗監聽，讓鍵盤一出現就有反應。
- 自訂列要短：三到六個鍵的小鍵盤一眼就能看懂。
- 鍵盤有具體用途時用 `Label` 命名，例如 "Macro pad"。

**避免**

- 在有密碼的畫面上使用視窗監聽：鍵會顯示正在輸入的內容。
- 把文字當作表單輸入；`Value` 只用來展示。

## API 參考

<!-- api: KeycapKeyboard -->

<!-- api: KeycapRow -->

<!-- api: KeycapBinding -->

## 無障礙

- 鍵盤是一個以 `Label` 命名的群組。每個鍵帽都是一個按鈕，以它的字元或功能命名（Space、Delete、Return、Shift），並且各自是一個 Tab 停駐點；Enter 或 Space 按下取得焦點的鍵帽。
- 文字區是一個唯讀文字方塊，位於 Tab 順序的最前面；在依焦點監聽時，聚焦它就能用真實按鍵打字。
- 帶 Ctrl、Alt 或 Command 鍵的快速鍵原樣放行，視窗監聽也不會把要輸入到文字方塊裡的內容打進來。鍵盤的說明文字會讀出點亮的快速鍵和顯示中的綁定。
- 鍵盤焦點會在取得焦點的鍵帽或文字區周圍畫一個方形焦點環；指標焦點不畫。

### 鍵盤互動

| 按鍵 | 作用 |
|---|---|
| 任意可列印鍵 | 打出字元，並在按住期間讓它的鍵帽保持按下。 |
| Backspace | 刪除最後一個字元；按住會重複。 |
| Enter | 另起一行。 |
| Shift、Tab、Caps、修飾鍵 | 點亮它們的鍵帽。Tab 仍然移動焦點。 |
| Tab 到一個鍵帽，再按 Enter 或 Space | 像按鈕一樣按下這個鍵帽。 |

## 動效

- 按下時，鍵帽在 35 ms 內落下大部分行程，200 ms 內回彈。
- 按下的鍵帽下方，底板上亮起一圈柔光，在 700 ms 內淡去，所以快速打字會留下一串光痕；鍵帽邊緣隨之著色，字元變亮。
- 新字元在 150 ms 內從輕微的模糊中淡入。打字時游標保持不動，停下 600 ms 後再開始閃爍。
- 切換鍵盤時，底板按彈簧變形，新的各列淡入。

| 變化 | 完整 | 減弱 | 關閉 |
|---|---|---|---|
| 按鍵 | 鍵帽陷下，柔光淡入淡出 | 沒有行程；柔光與字元顏色立即變化 | 鍵帽直接落下 |
| 新字元 | 從模糊中淡入 | 直接出現 | 直接出現 |
| 游標 | 閒置時閃爍 | 常亮 | 常亮 |
| 切換鍵盤 | 底板變形，各列淡入 | 直接跳變 | 直接跳變 |

## 響應式

- 一個鍵單位決定所有尺寸（間距、行程、圓角和字元），隨寬度增大，最大 54 px，所以最寬的一列總能放下；`KeySize` 可以把上限設得更低。
- 窄寬度下文字從 32 px 縮到 20 px，保持兩行，並捲動到最新一行。
- 輕觸在按下時就按下鍵帽，手指抬起前一直保持按下。觸控時不顯示懸停效果。

## 相關元件

- `ShortcutRecorder` 記錄人們按下的快速鍵。
- `TypewriterTerminal` 自行打出一段預設的終端機工作階段。

| 元件 | 適用場景 |
|---|---|
| `KeycapKeyboard` | 一塊會回應打字和點擊的趣味鍵盤 |
| `ShortcutRecorder` | 讓人設定自己的快速鍵 |
| `TypewriterTerminal` | 播放一段無需輸入的終端機工作階段 |
