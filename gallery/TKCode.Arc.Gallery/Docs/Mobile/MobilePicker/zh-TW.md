---
title: 移動端選擇器
summary: 列尾顯示目前值的清單列，點按後開啟一個操作面板，列出可選項並勾選目前項。
source-hash: 4d94d890328e
---

<Demo name="Basic" />

## 何時使用

`Picker` 是設定頁裡從短清單中選擇一個值的方式：列上顯示目前選取的內容，點按後從底部邊緣彈出可選項。

- **有幾個命名值的設定項**：語言、文字大小、預設日曆。
- **選項能放進面板的表單欄位**：短清單中的國家、貨幣、尺寸。
- **與其他列並列的一列**：它位於 `ListGroup` 中，與 `ListItem` 和開關並排，外觀一致。

即時範例在手機外框中執行，那是桌面上的模擬；真機上的觸控回饋和面板的拖曳這裡尚未驗證。

## 何時不用

- **兩三個都應始終可見的選項**：使用 `MobileSegmented` 或 `MobileRadio`。
- **更適合用圖片表達的選擇**：使用 `MobileRadioCards`。
- **選項很多，或需要搜尋的選項**：改為推入一個包含 `ListGroup` 和 `Searchbar` 的頁面；操作面板超過大約八列就會捲動。
- **操作而非取值**：直接使用 `MobileActionSheet`。

## 用法

把選擇器放進 `ListGroup`。它的內容是列的標題；選項用內嵌的 `Items` 寫出，或者用 `ItemsSource`。`SelectedIndex` 和 `SelectedItem`（都是雙向繫結）相互跟隨。點按後會開啟一個操作面板，列出各個選項並勾選目前項；選擇一列會選取它並關閉面板，點取消則關閉面板且不做更改。

```xml
<arcm:ListGroup xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                Header="General">
    <arcm:Picker Icon="{x:Static arc:ArcIcons.Languages}" Content="Language"
                 ItemsSource="{Binding Languages}" SelectedItem="{Binding Language}" />
    <arcm:Picker Content="Text size" SheetMessage="Applies to every screen">
        <arcm:Picker.Items>
            <x:String>Small</x:String>
            <x:String>Medium</x:String>
            <x:String>Large</x:String>
        </arcm:Picker.Items>
    </arcm:Picker>
</arcm:ListGroup>
```

選擇器會在它所在的 `Page` 上方開啟面板，效果與 `Page.Overlay` 中的面板覆蓋頁面相同，因此無需在頁面中新增任何東西；面板會自行為頁面底部保留安全區。不在頁面中時，則在視窗上方開啟。未選取任何項時，列上顯示 `PlaceholderText`（"Not set"），顏色較淡。面板的標題是 `SheetTitle`，若未設定則取列的標題（當它是文字時）；`SheetMessage` 會在標題下方增加一行說明，`CancelText` 設定取消列的文字（設為 null 則隱藏該列）。`IsOpen` 可在程式碼中開啟和關閉面板。停用的選擇器，或沒有選項的選擇器，不會開啟面板。

選擇器是一個 `ListItem`，因此 `Icon`、`Leading`、`Subtitle` 和 `ShowChevron`（預設開啟）與任何列一樣可用；`Trailing` 不顯示，因為列尾已經用來顯示值。

### 文字與範本

選項顯示其 `ToString()` 的結果。`DisplayMemberBinding` 從選項的某個成員讀取文字；`ItemTemplate` 同時建構列尾的值和面板中的每一列，例如一個符號加一個名稱。在以 NativeAOT 編譯的應用程式中，請為範本設定 `x:DataType`，使其繫結被編譯：

<Demo name="Templates" />

```xml
<arcm:Picker Content="Currency" ItemsSource="{Binding Currencies}" SheetTitle="Show prices in">
    <arcm:Picker.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <TextBlock Text="{Binding Symbol}" FontWeight="SemiBold" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>
    </arcm:Picker.ItemTemplate>
</arcm:Picker>
```

### 事件

只要選取項發生變化，就會觸發 `SelectionChanged`：在面板中選擇了一列、透過程式碼或繫結設定了 `SelectedItem` 或 `SelectedIndex`，或者清空了選取。它的參數包含 `OldItem`、`NewItem`、`OldIndex` 和 `NewIndex`。當使用者選擇一列時會觸發 `ItemInvoked`，即使選取的是已經選取的那一列，因此它適合用於「選擇後關閉此頁面」這類情境。

```csharp
picker.SelectionChanged += (_, e) => Save((Currency?)e.NewItem);
```

在選項到達之前設定的索引會等待選項就緒。選項變化時，選取項保持原位，索引隨之跟隨；不再位於選項之中的項仍會顯示，其索引為 -1。

### Token

`PickerToken` 保存 `ValueMaxWidth`（160），即值在以省略號結尾之前能佔用的最大寬度，以便為標題保留空間。列的其餘部分是一個 `ListItem`（`ListItemToken`），面板則是一個 `ActionSheet`（`ActionSheetToken`、`ActionSheetItemToken`）。顏色來自 ArcDesign token：值使用次要文字顏色，佔位文字使用弱化文字顏色。

## API 參考

<!-- api: MobilePicker -->

## 無障礙

列是一個按鈕，其名稱即標題；它的說明文字為目前值（或佔位文字），因此螢幕閱讀器會同時讀出兩者。Tab 可以移到該列，按 Enter 或空白鍵即可開啟面板；此時已勾選的列獲得鍵盤焦點，Tab 在各列之間移動，Esc 關閉面板。面板關閉後，焦點返回選擇器。使用鍵盤聚焦的選擇器會像按下的列一樣著色。每個選項都是 56 px 高的列，目前項以勾號標記，而不只靠顏色區分。

## 動效

面板是一個 `ActionSheet`，依其物理特性運動：它以平滑的彈簧升起，拖曳時跟隨手指，放開時把手指的速度交給彈簧；減弱動效時則直接跳到位置。列在按下時以短暫的淡入改變底色，與 `ListItem` 一樣。勾號隨選取項出現，本身沒有動效。

## 響應式

- `Picker` 沒有斷點。它是一個寬度占滿、高度至少 48 px 的列，其面板在任何寬度下都橫跨頁面，兩側各留 8 px。
- 列尾的值最寬為 `ValueMaxWidth`（160 px），超出部分以省略號結尾，因此過長的值不會擠掉標題；標題佔用列中的其餘部分。
- 面板隨列數增高，最高為頁面的 90%，之後縱向捲動，從不橫向捲動。它會為頁面底部保留安全區，因此取消列不會被 Home 指示條遮住。
- 沒有任何功能依賴懸停：手指按下時列會變深，輕觸即開啟面板。

## 相關元件

- `MobileActionSheet`：選擇器所開啟的面板，及其 `IsChecked` 列。
- `MobileListGroup`：選擇器所在的群組。
- `MobileRadioCards`：適合更宜用圖片表達的選擇。
- `MobileSegmented`：適合兩到四個需要始終可見的選項。
