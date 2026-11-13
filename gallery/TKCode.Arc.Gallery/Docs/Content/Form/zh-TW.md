---
title: 表單
summary: 輕量的表單範圍：帶有必填標記和說明文字的帶標籤項目，在使用者操作時顯示檢視模型的驗證錯誤，提交時捲動到第一個無效欄位。
source-hash: 24c45d61692b
---

<Demo name="Preview" />

## 何時使用

- **註冊、結帳、設定和對話方塊**：幾個欄位一起提交，使用者應當在送出之前看到需要修正的地方。
- **規則已經位於檢視模型中**（`INotifyDataErrorInfo`，例如 CommunityToolkit.Mvvm 的 `ObservableValidator` 搭配 DataAnnotations）：表單會在合適的時機、以合適的位置顯示這些錯誤。
- **所有欄位使用同一種版面配置**：相同的標籤、必填標記、說明文字和錯誤列，標籤位於上方，或在左側對齊成一欄。

## 何時不用

- **單一欄位**：Arc 文字方塊已經帶有標籤、描述和錯誤列。
- **在 XAML 中撰寫的規則**：表單沒有規則引擎。必填、電子郵件、長度、範圍和正規表示式規則應放在檢視模型中。
- **逐一屬性地編輯一個物件**：使用 `PropertyGrid`。
- **分步驟的表單**：使用 `MultiStepForm` 區塊。

## 用法

`Form` 衍生自 Avalonia 的 `ContentControl`，自身不做任何版面配置：在其中放入一個面板即可。其中的每個 `FormItem` 進入邏輯樹時都會加入最近的表單，被移除時離開，因此無需手動註冊。`FormItem` 衍生自 `Field`：`Label`、`Description` 和 `ErrorMessage` 的用法與任何 Arc 欄位相同。

```xml
<arc:Form ValidateCommand="{Binding ValidateCommand}"
          SubmitCommand="{Binding CreateAccountCommand}"
          ResetCommand="{Binding ResetCommand}">
    <StackPanel Spacing="20">
        <arc:FormItem Label="Email" IsRequired="True">
            <arc:Input Text="{Binding Email}" Placeholder="you@example.com" />
        </arc:FormItem>
        <arc:FormItem Label="Password" IsRequired="True" Description="At least 8 characters.">
            <arc:PasswordField Text="{Binding Password}" />
        </arc:FormItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <arc:Button arc:Form.Action="Submit" IsDefault="True" Content="Create account" />
            <arc:Button arc:Form.Action="Reset" Variant="Secondary" Content="Reset" />
        </StackPanel>
    </StackPanel>
</arc:Form>
```

### 檢視模型中的規則

由檢視模型決定什麼是有效的；由表單決定錯誤何時、何處顯示。錯誤透過 Avalonia 的資料驗證（`DataValidationErrors`）傳遞到各個項目，因此表單從不讀取檢視模型，對 AOT 和修剪保持安全。本頁的範例透過手寫程式碼實作 `INotifyDataErrorInfo`；使用 CommunityToolkit.Mvvm 時，同一個檢視模型如下所示：

```csharp
public partial class SignUpViewModel : ObservableValidator
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter an email like name@example.com.")]
    private string _email = "";

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    private string _password = "";

    [RelayCommand]
    private void Validate() => ValidateAllProperties();        // Form.ValidateCommand

    [RelayCommand]
    private async Task CreateAccountAsync()                    // Form.SubmitCommand
    {
        ValidateAllProperties();                               // the form only sees mounted fields
        if (HasErrors)
        {
            return;
        }
        await _api.CreateAsync(Email, Password);
    }
}
```

一個項目會顯示第一條錯誤，並把所有錯誤保存在 `Errors` 中。同一屬性上的多條規則按順序回報，因此顯示的是第一條不通過的規則。繫結無法轉換的值（例如繫結到 `int` 的數字欄位中輸入的 `abc`）同樣會被收集，並阻止提交，儘管檢視模型看不到它。

不要在 `ObservableValidator` 的同時啟用 Avalonia 的 DataAnnotations 外掛程式（`WithDataAnnotationsValidation()`）：兩者會回報相同的規則，每條錯誤都會顯示兩次。修剪和 AOT 應用程式最好使用手寫規則，因為 .NET 的 DataAnnotations `Validator` 需要反射。

### 提交

`SubmitAsync()`（或帶有 `arc:Form.Action="Submit"` 的按鈕；設定 `IsDefault="True"` 時按 Enter 即提交）執行以下步驟：

1. 顯示每個項目的錯誤，並執行 `ValidateCommand`，因此使用者沒有碰過的欄位也會被檢查。
2. 在任一項目 `IsValidating` 為 true 時等待。此期間再次提交會傳回同一個工作；`ResetFields()` 或表單卸載會停止等待。
3. 有錯誤時：引發 `SubmitFailed`，其中的無效項目依文件順序排列；把第一個無效項目捲動到可見區域，連同其標籤一起顯示，並聚焦其欄位，同時播報還有幾個欄位需要處理。在處理常式中設定 `SuppressFocus`，即可自行移動焦點，例如在切換索引標籤之後。
4. 沒有錯誤時：引發 `Submitted`，然後以 `SubmitCommandParameter` 作為參數執行 `SubmitCommand`。

預覽中的提交按鈕位於摺線以下：提交一個空白表單時，檢視會捲動回第一個錯誤。

| 方法 | 作用 |
|---|---|
| `Validate()` | 顯示所有錯誤，執行 `ValidateCommand`，並傳回表單是否有效（檢查進行中時為 false） |
| `ValidateAsync()` | 與 `Validate()` 相同，但會先等待正在進行的檢查 |
| `SubmitAsync()` | 即上面的提交；表單有效且命令已執行時傳回 true |
| `ResetFields()` | 執行 `ResetCommand`，然後呼叫 `ClearValidation()`，最後引發 `FieldsReset` |
| `ClearValidation()` | 隱藏已顯示的錯誤並忘記提交狀態；檢視模型的錯誤保留（即 issue 中的 `ClearValidate()`） |
| `FocusFirstError()` | 捲動到並聚焦第一個無效項目，例如在收到伺服器錯誤之後 |
| `GetInvalidItems()` | 依文件順序傳回無效項目，方便你自行彙總 |

`HasErrors`、`ErrorCount`、`IsValidating` 和 `IsSubmitAttempted` 是唯讀屬性，可以用於繫結。

### 何時顯示錯誤

`ErrorTrigger` 決定項目何時開始顯示其已收集的錯誤。它本身不執行任何規則：檢視模型何時檢查某個值取決於繫結（`UpdateSourceTrigger=LostFocus` 只在離開欄位時檢查）。

| `ErrorTrigger` | 顯示錯誤的時機 |
|---|---|
| `Change` | 錯誤一出現就顯示，與獨立欄位相同 |
| `Blur`（預設） | 焦點離開該項目之後（開啟其下拉清單不算），或在提交時 |
| `Submit` | 僅在提交或呼叫 `Validate()` 時 |

一旦某個項目開始顯示錯誤，修正值會立即隱藏該錯誤；之後出現的新錯誤會在抵達時顯示。`ErrorTrigger` 是可繼承的附加屬性：可以設定在表單上、分區面板上或某一個項目上。

<Demo name="ErrorTrigger" />

### 非同步檢查

檢視模型會做防抖、呼叫伺服器，並透過 `ErrorsChanged` 回報結果；它還保存一個旗標，項目將其繫結到 `IsValidating`。該旗標為 true 時，項目會顯示「正在檢查…」，而不是舊的錯誤，同時提交會等待檢查結束。在 `finally` 區塊中清除該旗標：表單會無限期等待，沒有逾時。

<Demo name="AsyncCheck" />

### 跨欄位規則

比較兩個欄位的規則（例如密碼確認）應放在檢視模型中：密碼變更時，重新檢查確認欄位。錯誤顯示在回報它的那個項目上；使用 `Blur` 時，在確認欄位被離開之後顯示。預覽就是這樣實作的。

### 錯誤顯示在哪裡

`ErrorPlacement`：`Inline`（預設）在欄位下方開啟一列顯示錯誤；`Tooltip` 不佔用空間，在滑鼠移入以及項目取得鍵盤焦點時，於欄位下方顯示錯誤，適用於高密度的表單；`None` 只顯示無效狀態，文字由你自行處理（`FormItem.Errors`、`GetInvalidItems()`）。`ErrorTemplate` 可以依你的方式呈現錯誤物件，無論是內嵌顯示還是在提示框中。明確設定的 `FormItem.ErrorMessage`（例如伺服器錯誤）會立即顯示，並優先於繫結產生的錯誤。

<Demo name="ErrorPlacement" />

### 標籤位置

`LabelPlacement="Left"` 把標籤放在一欄中。`LabelWidth` 保持 NaN 時，這一欄取表單中最寬標籤的寬度（最多 240 px；更長的標籤會換行）；在分區上設定 `Grid.IsSharedSizeScope="True"` 可以只讓該分區內部對齊，或將 `LabelWidth` 設為固定寬度。標籤相對於項目的 `Size` 所對應的欄位置中（為小欄位的項目設定 `Size="Small"`）。

<Demo name="LabelPlacement" />

`RequiredIndicator` 以星號標記必填項目（預設），或將其他項目標記為「選填」，也可以不顯示任何標記。`IsRequired` 本身不新增任何規則。

### 動態增減的項目

`ItemsControl` 範本中的項目會在其列被加入時加入表單，在列被移除時離開表單。每一列保留自己的規則；父檢視模型在提交時檢查這些列。

<Demo name="DynamicItems" />

表單只統計已掛載的項目：未選取索引標籤上的欄位、虛擬化清單尚未產生的列，以及已關閉的彈出層都不在其中。請在檢視模型的提交命令中繼續檢查 `HasErrors`，或將 `ErrorSource` 設為檢視模型：此時它的 `HasErrors` 會阻止提交（`SubmitFailed.FirstInvalidItem` 為 null），其實體層級錯誤（`GetErrors(null)`）會出現在 `FormErrors` 中，供你自行顯示提示。某些實作（例如 `ObservableValidator`）會為 `GetErrors(null)` 傳回所有錯誤。

### 唯讀與關閉驗證

`IsReadOnly`（可繼承，因此也可用於分區）會讓 Arc 文字欄位、`Cascader`、`TreeSelect`、`Rating`、`TokenTextEditor` 和 `RichTextEditor` 變為唯讀，並隱藏必填標記。核取方塊、切換開關、選取器、日期時間選擇器和滑桿目前還沒有唯讀模式，仍然可以編輯：請將它們停用，或用 `IsEnabled="False"` 停用整個表單。在表單或某個項目上設定 `IsValidationEnabled="False"`，會隱藏其錯誤、停止統計，並允許提交通過；同時也應在檢視模型中關閉對應的規則。已停用、已隱藏和唯讀的項目會被自動略過。

<Demo name="ReadOnly" />

### 項目內的欄位

項目內的 Arc 欄位把標籤和錯誤列交給項目來顯示，只有在項目顯示錯誤時才變為紅色；它們自己的說明列保留。下表中的值屬性會回報其繫結的驗證錯誤，因此可以在表單中使用：

| 控制項 | 已驗證的屬性 |
|---|---|
| `Input`、`Textarea`、`PasswordField`、`SearchField` | `Text` |
| `NumberField`、`MoneyInput`、`PhoneInput` | `Value`（以及 `Text`） |
| `DatePicker`、`TimePicker`、`DateRangePicker`、`ColorPicker` | `SelectedDate`、`SelectedTime`、`SelectedRange`、`Color` |
| `Checkbox`、`Switch` | `IsChecked` |
| `Select`、`Combobox`、`MorphSelect`、`MultiSelect`、`RadioGroup`、`RadioCards`、`SegmentedControl`、`ChipGroup`、`ToggleGroup` | `SelectedItem`、`SelectedValue`、`SelectedIndex`（`MultiSelect` 和 `ChipGroup` 上為 `SelectedItems`） |
| `Cascader`、`TreeSelect` | `SelectedItem`、`SelectedValue`（`TreeSelect` 上為 `SelectedValues`） |
| `TagInput`、`Slider`、`Rating`、`ScrubInput`、`ShortcutRecorder`、`EnumSelector` | `Value`（範圍 `Slider` 上為 `LowerValue` 和 `UpperValue`），`SelectedEnum` |

一個控制項只回報一組錯誤：當其兩個已驗證的屬性都被繫結時，最後一次回報生效。其他程式庫的控制項保留自己的錯誤顯示（Fluent 主題的 `DataValidationErrors`），它會顯示在項目的列旁邊；如需關閉，請使用樣式。

## API 參考

<!-- api: Form -->

<!-- api: FormItem -->

<!-- api: FormSubmitFailedEventArgs -->

<!-- api: FormLabelPlacement -->

<!-- api: FormErrorTrigger -->

<!-- api: FormErrorPlacement -->

<!-- api: FormRequiredIndicator -->

<!-- api: FormAction -->

## 協助工具

- 表單是一個表單地標；請使用 `AutomationProperties.Name` 為它命名。
- 項目為其欄位提供標籤（`Target`，或第一個可聚焦的編輯器）：標籤為 `LabeledBy`，除非你另行設定，否則標籤文字即名稱；`IsRequired` 對應 `IsRequiredForForm`；說明文字包含描述，以及顯示後的錯誤。尚未顯示的錯誤也不會被朗讀。
- 新錯誤以強提示方式播報；使用 `Tooltip` 或 `None` 位置時，一段隱藏的即時文字會播報相同內容。一次提交或 `Validate()` 顯示多個錯誤時，只播報一則摘要（「2 個欄位需要處理」），然後焦點移到第一個無效欄位，朗讀其名稱和錯誤。
- 「正在檢查…」以禮貌方式播報。星號和「選填」是裝飾性的；文字跟隨應用程式語言（en-US、zh-CN、zh-TW）。

### 鍵盤

| 按鍵 | 行為 |
|---|---|
| Tab | 依內容順序在各欄位之間移動 |
| Enter | 透過帶有 `arc:Form.Action="Submit"` 和 `IsDefault="True"` 的按鈕提交 |

提交失敗後，焦點以鍵盤焦點的方式移到第一個無效欄位，因此非文字控制項也會顯示焦點環。表單不處理其他任何按鍵。

## 動態效果

- 錯誤、說明和「正在檢查…」列以平滑的彈簧開啟和關閉，並且只改寫發生變化的文字，與欄位的訊息列相同。欄位的框線以其一般過渡變為紅色；提示框的移動與任何提示框相同。
- 由 `ErrorTemplate` 呈現的錯誤無動畫地出現，捲動到第一個錯誤也是立即完成的。
- 減少動態效果時，各列以完整高度出現，文字淡入；關閉動態效果時一切立即完成。

## 響應式

- 表單佔用所分配的寬度，項目會延展到該寬度。
- 左側標籤共用一欄，超過 240 px 後換行。表單不會在窄螢幕上把標籤切換到頂部：在窄版面中請選擇 `LabelPlacement="Top"`（例如依據 `ArcViewport.IsCompact`）。
- 提示框的位置可以防止在出現錯誤時，高密度表單的列被撐高。

## 相關元件

- `Field`：單一編輯器的標籤、說明和錯誤結構。
- `Input`、`PasswordField`、`Select`、`Checkbox`：可放入項目中的欄位。
- `PropertyGrid`：無需撰寫表單即可編輯物件的屬性。
- `MultiStepForm`：拆分為多個步驟的表單。
