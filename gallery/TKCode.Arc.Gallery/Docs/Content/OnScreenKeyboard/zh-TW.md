---
title: 螢幕鍵盤
summary: 為沒有實體鍵盤的自助機與面板提供的觸控鍵盤：全鍵盤、數字鍵盤、PIN 鍵盤或你自己的版面配置，向有焦點的欄位輸入文字，且不搶走焦點。
source-hash: 24c01c172220
---

<Demo name="Preview" />

## 何時使用

`OnScreenKeyboard` 在螢幕上繪製一個鍵盤，並向同一視窗中有焦點的文字輸入框輸入文字。適用於使用者只有觸控螢幕、且無法依賴系統鍵盤的情境：系統鍵盤可能不會自動彈出（例如 Windows 自助機），或者根本沒有系統鍵盤（例如嵌入式 Linux 面板）。

- **自助服務機**：報到、點餐或取票機，訪客在上面輸入姓名、電子郵件或預訂代碼。
- **觸控面板與工業 HMI**：操作員在沒有連接鍵盤的工控機上輸入批號、數量或設定值。
- **PIN 與代碼輸入**：門禁面板或收銀台要求輸入 PIN。PIN 鍵盤可以把數字送到你自己的顯示內容中，因此數字從不以文字形式出現。
- **數量與價格欄位**：數字鍵盤帶有 `Culture` 的小數分隔符、退格鍵和游標鍵，用拇指就能快速填好數量欄位。
- **特殊小鍵盤**：十六進位鍵盤、單位鍵盤，或其他語言的鍵盤，都可以基於同一個版面配置模型建構。

## 何時不用

- **手機與平板**：系統鍵盤在那裡更好（有預測、語音輸入與無障礙設定）。請改為在文字框上用 `TextInputOptions` 要求合適類型的鍵盤。
- **附有實體鍵盤的桌上型電腦**：螢幕上的第二個鍵盤只會佔用空間。
- **從一個範圍中選取數值**：使用 `Slider` 或 `NumberField` 的步進按鈕，它們完全不需要輸入。
- **從已知清單中選擇**：使用 `Select`、`Combobox` 或 `SegmentedControl`；點選一個選項比輸入它更快。

## 用法

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <DockPanel>
        <arc:OnScreenKeyboard DockPanel.Dock="Bottom" MaxWidth="720" />
        <StackPanel Spacing="12" Margin="24">
            <arc:Input Label="Name" />
            <arc:Input Label="Email" />
        </StackPanel>
    </DockPanel>
</UserControl>
```

先輕點一個欄位，然後按鍵。鍵盤會輸入到所在視窗中有焦點的文字輸入框：Arc 的 `Input`、`Textarea`、`PasswordField` 或 `NumberField`、Avalonia 的 `TextBox`，或者你自己處理文字輸入的控制項。按鍵從不移動焦點，因此游標留在原處，Tab 鍵或輕點另一個欄位照常移動焦點。

每個按鍵如何到達欄位：

- **字母、數字、符號與空白鍵**以文字輸入的形式傳送，與系統鍵盤或輸入法觸發的事件相同。文字框會在游標處插入文字、取代選取內容，並保留 `MaxLength`、`IsReadOnly` 與復原歷程。
- **Backspace、Enter、Tab 與方向鍵**以按鍵的形式傳送。文字框自行處理它們：Enter 在多行文字框中開始新行，在單行文字框中則冒泡到預設按鈕或你自己的 `KeyDown` 處理常式。Tab 把焦點移到下一個欄位。

按鍵在按下時生效，而不是放開時，因此雙指快速輸入的順序是正確的。

### 結構

1. **托盤**：按鍵背後的柔和面板，帶 1 px 的細微邊框、20 px 圓角與 8 px 內距。
2. **字元鍵**：最淺的按鍵，帶 1 px 邊緣與 2 px 底部厚邊、12 px 圓角與 22 px 標籤。
3. **功能鍵**：Shift、Backspace、頁面鍵與方向鍵，比字元鍵深一級。標籤超過一個字元（"123"、"ABC"）時使用小號中等字重。
4. **Enter**：主要按鍵，與主按鈕一樣以前景色填滿。
5. **列**：每一列都置中，因此鍵單位數少於最寬列的列兩側會縮排（QWERTY 的基準列縮排半個鍵）。按鍵高 46 px，鍵與鍵之間 6 px，列與列之間 8 px。

### 數字與 PIN 鍵盤

`Preset` 選擇內建版面配置。`Numeric` 是數字鍵盤，小數分隔符來自 `Culture`（或目前文化），包含退格、左右游標鍵與 Enter；`SignedNumeric` 額外增加一個負號鍵；`Digits` 是 PIN 鍵盤，只有數字、退格與 Enter。鍵盤會把按鍵拉伸到你給它的尺寸。

<Demo name="NumberPad" />

```xml
<arc:OnScreenKeyboard Preset="Numeric" Culture="de-DE" Width="280" Height="260" />
```

讓鍵盤與它所填入的欄位使用相同的文化：`NumberField` 使用自己的 `Culture` 解析數字（除非你設定了它，否則為 en-US），因此德語的逗號在那裡無法被理解。

### 自行處理按鍵

`ItemInvoked` 會在鍵盤執行動作之前，為每次按鍵以及按住按鍵時的每次重複觸發。它的 `Item` 是按鍵，`Text` 是即將輸入的文字（已套用 Shift），`Target` 是將接收它的元素，`IsRepeat` 表示按鍵是否被按住。將 `Handled` 設為 true 後，鍵盤不再做任何其他動作：它不輸入任何文字，也不改變 Shift 與頁面的狀態。下面的 PIN 鍵盤會填入四個圓點，從不以文字形式顯示數字。

<Demo name="PinPad" />

```csharp
keypad.ItemInvoked += (_, e) =>
{
    e.Handled = true;
    switch (e.Item.Kind)
    {
        case OnScreenKeyboardKeyKind.Text:
            pin.Append(e.Text);
            break;
        case OnScreenKeyboardKeyKind.Backspace when pin.Length > 0:
            pin.Length--;
            break;
        case OnScreenKeyboardKeyKind.Enter:
            await UnlockAsync(pin.ToString());
            break;
    }
};
```

只想新增內容時，可以只處理少數幾個按鍵，其餘按鍵照常放行，例如一個清空欄位的 `Custom` 鍵 "Clear"。`Invoke(key)` 從程式碼中按下一個按鍵，效果與輕點相同。

### 在欄位下方開啟鍵盤

與停靠的鍵盤不同，可以在欄位上設定 `OnScreenKeyboard.Popup`。欄位取得焦點時，該類型的鍵盤會在其下方開啟（下方沒有空間時則在其上方），並且只向它輸入。焦點離開欄位、在單行欄位中按下 Enter，或按下 `Hide` 鍵時它會關閉；再次輕點欄位會重新開啟它。`OnScreenKeyboard.PopupLayout` 則可以改為開啟你自己的版面配置。

<Demo name="Popup" />

```xml
<arc:NumberField Label="Quantity" arc:OnScreenKeyboard.Popup="Numeric" />
<arc:PasswordField Label="Door code" arc:OnScreenKeyboard.Popup="Digits" />
```

### 自訂版面配置

版面配置就是普通資料：`OnScreenKeyboardLayout` 包含頁面，頁面包含列，列包含按鍵。每個 `OnScreenKeyboardKey` 說明它做什麼（`Kind`）、輸入什麼（`Text`，以及按下 Shift 時的 `ShiftText`）、顯示什麼（`Label`，或 24 × 24 的 Lucide `Glyph`），以及它有多寬（`Width`，以標準鍵為單位）。按鍵可以在顯示期間繫結與修改：停用的按鍵變暗且不作用，隱藏的按鍵不佔空間，它所在的列會收合。

<Demo name="CustomLayout" />

| 類型 | 作用 |
|---|---|
| `Text` | 輸入 `Text`；按下 Shift 時輸入 `ShiftText`（否則輸入大寫的 `Text`） |
| `Space` | 輸入一個空白 |
| `Backspace` | 刪除游標前的字元或選取內容；按住時重複 |
| `Enter` | 在多行文字框中開始新行；否則作為按鍵冒泡 |
| `Tab` | 把焦點移到下一個欄位；開啟 Shift 時則向前移動 |
| `Shift` | 作用於下一個按鍵；快速點按兩次鎖定大寫 |
| `Left`、`Right`、`Up`、`Down` | 移動游標（開啟 Shift 時擴展選取）；按住時重複 |
| `Page` | 切換到 `TargetPage` 指定的頁面 |
| `Hide` | 引發 `HideRequested`；由 `Popup` 開啟的鍵盤隨之關閉 |
| `Key` | 以按鍵的形式傳送 `KeyCode`（Escape、Delete、Home...） |
| `Custom` | 本身不做任何事：請處理 `ItemInvoked` |
| `Spacer` | 一個寬度為 `Width` 單位的空白間隔 |

`OnScreenKeyboardLayout.Create(preset, culture)` 以及 `CreateQwerty`、`CreateNumeric` 與 `CreateDigits` 會回傳內建版面配置的新物件，你可以在使用前修改它們。`OnScreenKeyboardRow.FromCharacters("qwertyuiop")` 從一個字串產生一列文字鍵。

### 其他語言

內建的 QWERTY 是英文的。其他語言同樣是普通的版面配置：從內建版面配置開始，取代其中的字母列，或者自己撰寫每一頁。鍵盤會依 `Culture` 的規則把字母轉為大寫，因此德語、土耳其語或希臘語的大寫字母都能正確顯示。

<Demo name="Languages" />

```csharp
var layout = OnScreenKeyboardLayout.CreateQwerty();
var letters = layout.Pages[0].Rows;
letters[0] = OnScreenKeyboardRow.FromCharacters("qwertzuiopü");
letters[1] = OnScreenKeyboardRow.FromCharacters("asdfghjklöä");
keyboard.Culture = CultureInfo.GetCultureInfo("de-DE");
keyboard.Layout = layout;
```

按鍵輸入的是文字，因此文字框能夠顯示的任何文字都可以輸入。需要從候選詞組合文字的語言，例如中文拼音或日文假名轉漢字，需要輸入法支援，不在鍵盤的涵蓋範圍之內。

### Shift、大寫鎖定與頁面

輕點一次 Shift 會把下一個按鍵大寫（`ShiftState` 為 `On`）；再點一次則關閉。快速連續點按兩次會鎖定大寫（`Locked`），直到再次點按 Shift 為止。與實體鍵盤的大寫鎖定一樣，大寫鎖定只改變字母；單次 Shift 還會讓數字與符號使用它們的 `ShiftText`。Shift 鍵開啟時填滿其圖示，鎖定時在圖示下方加一條橫線，因此狀態不只依賴顏色表示。`ShiftState` 支援雙向繫結。

QWERTY 版面配置有三頁：字母、數字與符號（"123"），以及更多符號（"#+="）。`CurrentPage` 指明目前顯示的頁面（`OnScreenKeyboardLayout.LettersPage`、`SymbolsPage`、`MoreSymbolsPage`），可以設定或繫結。切換頁面會關閉單次 Shift，但保留大寫鎖定。

### 使用 Target 取代焦點

設定 `Target` 可以把每個按鍵都傳送到一個元素，無論它是否有焦點。當鍵盤專屬於某一個欄位，或者頁面上其他地方的按壓會搶走焦點時，就使用它。沒有目標且沒有任何元素取得焦點時，按鍵只會引發 `ItemInvoked`。

```xml
<arc:Input x:Name="Code" Label="Booking code" />
<arc:OnScreenKeyboard Target="{Binding #Code}" />
```

### 按鍵重複

按住 Backspace 或方向鍵時，它會在 `RepeatDelay`（500 ms）之後，每隔 `RepeatInterval`（50 ms）重複一次，直到放開為止。其他按鍵不重複；如需改變，可在按鍵上設定 `IsRepeatable`。計時使用 `TimeProvider`，因此測試可以用自己的時鐘驅動重複。

### 尺寸

沒有指定尺寸時，鍵盤的寬度與其最寬的列相同，每個鍵為 52 px（QWERTY 約 530 px，包括托盤），高度與列數相同，每列 46 px。給出更大的空間時，按鍵會隨之放大：寬度依按鍵單位平分，高度依列平分。在寬螢幕上請設定 `MaxWidth`（QWERTY 約 720 到 900 px），讓按鍵保持舒適的比例。停靠在視窗底部時，設定 `CornerRadius="0"` 與 `BorderThickness="0,1,0,0"`，即可得到平整的托盤。

### 內容規範

- **標籤**：按鍵標籤要簡短。一個字元保持大號；較長的標籤（"Done"、"Clear"）使用小號，按鍵太窄時以省略號截斷。
- **名稱**：顯示圖示或符號的按鍵，應設定 `AutomationName`（"Minus"、"Decimal separator"、"Hide keyboard"）。
- **欄位標籤**：欄位仍需要自己的標籤與提示；鍵盤不會說明該輸入什麼。

| 不要這樣寫 | 改為 |
|---|---|
| 標示 "OK" 卻只會關閉的按鍵 | `Kind="Hide"`，名稱為 "Hide keyboard" |
| 按鍵上拼寫出 "Enter" | Enter 圖示，或 "Done"、"Search" 之類的動詞 |
| 沒有名稱的符號鍵 | `AutomationName="Degree sign"` |

### 最佳做法

**建議**

- 為數字與 PIN 鍵盤設定適合拇指的尺寸，寬約 260 到 300 px。
- 讓 `Culture` 與欄位的文化一致，以正確輸入小數。
- 對於不能以文字形式出現的 PIN 與代碼，處理 `ItemInvoked`。
- 在寬螢幕上橫向拉伸的鍵盤，請設定 `MaxWidth`。

**避免**

- 在已有系統鍵盤的裝置上顯示螢幕鍵盤。
- 在自訂按鍵範本中或鍵盤周圍放置可取得焦點的控制項，以免按壓時誤使其取得焦點。
- 把螢幕鍵盤用於清單或滑桿能更快完成的選擇。

### 常見問題

**為什麼鍵盤輸入不了任何內容？** 沒有元素取得焦點，也沒有設定 `Target`。請先輕點一個欄位、設定 `Target`，或在沒有焦點時處理 `ItemInvoked`（上方的預覽從第一個欄位開始）。

**按鍵會搶走我文字框的焦點嗎？** 不會。鍵盤在 Avalonia 的焦點處理看到之前，就已接管其內部的每次按壓，而且按鍵本身不可取得焦點。

**Backspace 和方向鍵在普通的 `TextBox` 中能用嗎？** 能，但需要使用為 `TextBox` 提供範本的主題（Fluent 或 Simple）。Arc 自己的文字輸入控制項在任何應用程式中都能運作；沒有範本的普通 `TextBox` 仍能接收輸入的文字。

**能否在 `KeyDown` 中監聽字母鍵？** 字母以文字輸入的形式傳送，而不是按鍵，因為大多數字元沒有鍵碼。請改為處理 `ItemInvoked`。

**能否讓按鍵在放開時才生效？** 不能。按鍵在按下時生效，這樣快速輸入能保持順序，並且按住的鍵可以重複。

## API 參考

<!-- api: OnScreenKeyboard -->

<!-- api: OnScreenKeyboardKey -->

## 無障礙

- 鍵盤是一個名為 "Keyboard" 的群組；請設定 `AutomationProperties.Name`，以依用途為它命名（"PIN pad"）。
- 每個按鍵都是按鈕，螢幕閱讀器會依其 `AutomationName`、標籤或類型來朗讀（"Shift"、"Backspace"、"Enter"、"Space"、"Left"）；Invoke 會按下它。Shift 同時是切換按鈕，會回報它是否開啟。
- Shift 的變化會以禮貌方式播報（"Shift on"、"Caps lock on"、"Shift off"，來自 `ShiftOnText`、`CapsLockOnText` 與 `ShiftOffText`），切換到的頁面也會播報（頁面的 `Label`，例如 "Numbers and symbols"）。
- Shift 與大寫鎖定同時改變圖示和填色；停用的按鍵會變暗，並以淡化的文字顏色繪製。
- 按鍵在自然尺寸下高 46 px，並隨所得空間增大。

### 鍵盤

螢幕鍵盤本身就是鍵盤：它的按鍵不取得焦點，它也沒有自己的鍵盤互動。實體鍵盤在有焦點的欄位中照常運作。

| 螢幕上的按鍵 | 傳送給欄位的內容 |
|---|---|
| 字母、數字、符號、空白 | 在游標處輸入文字 |
| `Backspace` | `Back` 按鍵 |
| `Enter` | `Enter` 按鍵 |
| `Tab` / 開啟 Shift 時的 `Tab` | `Tab` / `Shift+Tab` 按鍵 |
| 方向鍵 / 開啟 Shift 時的方向鍵 | 方向鍵 / 帶 `Shift` 的方向鍵（擴展選取） |

## 動態效果

- 按下的按鍵在快速彈簧上下沉至 95%，並採用按下時的填色；放開時彈回。
- 滑鼠停留（僅滑鼠）和按下時的填色透過快速淡變切換。
- 標籤、Shift 與頁面立即切換。

| 變化 | 完整 | 減弱 | 關閉 |
|---|---|---|---|
| 按鍵按下 | 快速彈簧縮放至 95%，填色淡變（120 ms） | 填色淡變，不縮放 | 立即 |
| 滑鼠停留（滑鼠） | 120 ms 填色淡變 | 120 ms 填色淡變 | 立即 |
| Shift、大寫鎖定、頁面 | 立即 | 立即 | 立即 |
| `Popup` 鍵盤開啟 | 彈出層的上升與淡入 | 淡入 | 立即 |

## 響應式

- 按鍵平分鍵盤所得的寬度；在手機寬度下，QWERTY 的頂列仍保留全部十個鍵，按鍵變窄，圓角隨之縮小，以保持按鍵的形狀。
- 除非給出了高度（之後各列平分該高度），鍵盤的高度與其列數相同。
- 沒有任何功能依賴滑鼠停留：每個按鍵都能透過輕點使用，滑鼠停留只為滑鼠增加填色。
- `Popup` 鍵盤在其欄位下方開啟，下方沒有空間時則在其上方開啟。

## 相關元件

- `NumberField` 用於帶步進按鈕的數字；`Input`、`Textarea` 與 `PasswordField` 是它輸入的欄位。
- `OtpInput`（Blocks）用於一次性驗證碼，可由 `Digits` 鍵盤輸入。
- `ShortcutRecorder` 用於從實體鍵盤錄入組合鍵。

| 元件 | 何時選擇 |
|---|---|
| `NumberField` | 有界的數字集合，需要步進、滾輪或拖曳調整 |
| `Slider` | 在一個範圍內選取近似值 |
| `Select` | 從已知清單中選擇一個選項 |
| `OtpInput` | 分格輸入的一次性驗證碼 |
| `ShortcutRecorder` | 從實體鍵盤錄入組合鍵 |
