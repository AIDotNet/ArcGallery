---
title: Import wizard
summary: 單張卡片完成的四步驟 CSV 匯入：放入檔案並預覽，把欄對應到你的欄位（每個猜測都附信心度），在計數遞減的同時修正或清理有問題的儲存格，然後分批匯入並閱讀摘要。
source-hash: 011ba077be94
---

<Demo name="Preview" />

## 何時使用

`ImportWizard` 把試算表匯出檔中的列帶入你的產品。使用者放入 CSV 檔案，確認各欄都落在正確的欄位，修正有誤的部分，然後匯入其餘的列：

- **匯入聯絡人、客戶、產品或使用者**，資料來自其他工具匯出的 CSV。
- **導入流程**，需要客戶既有的資料，產品才能發揮作用。
- **管理工具一次更新大量記錄**，既有的鍵值會變成更新，而不是重複新增。

## 何時不使用

- **把檔案附加到記錄上。** 請改用 `FileUpload` 或 `FileDropzone`；這裡不會解析檔案。
- **對應欄位卻不檢查值。** 每欄放一個 `Select` 的表單就夠了。
- **與其他工具保持資料同步。** 請使用依排程同步的整合；精靈適合的是一次性匯入。

## 用法

卡片衍生自 Avalonia 的 `TemplatedControl`，不需要任何資料：未設定任何屬性時，它使用聯絡人結構描述（姓名、電子郵件、公司、職稱、電話、國家、方案），在「Try a sample file」後面提供範例客戶清單，並模擬匯入。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:ImportWizard BatchSize="200" Noun="contact" PluralNoun="contacts" />
</UserControl>
```

### 四個步驟

`Step` 表示目前顯示的步驟，每次變更都會觸發 `StepChanged`。步驟列中已完成的步驟可跳回該步驟；匯入執行期間無法跳轉。

1. **上傳**：把 CSV 拖放到區域上、點擊區域（或按 Enter）選擇檔案，或載入範例。讀取檔案時會顯示一行簡短的進度，之後顯示檔名、大小、列數和欄數，以及前五列。欄數少於兩欄、沒有資料列、引號未閉合，或列數超過 `MaxRows` 的檔案會被拒絕，並在區域下方顯示訊息。
2. **對應欄位**：檔案的每一欄各佔一列，包含兩個範例值和一個欄位下拉選單。各欄依序解析，每欄都附有信心度：90 以上為綠色，80 到 89 為中性色，低於 80 為琥珀色。手動選定的欄位顯示「Chosen」，未使用的欄顯示「Ignored」，同一欄位被兩欄選取時，兩欄都顯示「Duplicate」。每個必填欄位都必須恰好對應一欄，「Check rows」才會啟用。
3. **檢查**：有問題的列排在最前面（「To fix」），每個有問題的儲存格都以色調標示，並在值下方顯示訊息。點擊儲存格（或按 Enter）即可編輯；按 Enter 或離開儲存格會儲存，按 Escape 則取消。「Clean up n values」會逐個儲存格套用所有安全的修正。列尾的 x 會把該列排除在外。仍有問題的列會被排除在匯入之外，但不會阻擋匯入。
4. **匯入**：就緒的列以 `BatchSize` 為一批送出，並顯示進度條；之後摘要會顯示新增、更新（其鍵值存在於 `ExistingKeys` 中）與被排除的筆數。「Download left-out rows」會把這些列存成 CSV，「Start a new import」則重新開始。

### 連接你的伺服器

`ImportBatchAction` 會接收每一批資料，以及一個取消權杖；按下 Cancel、重新開始，或卡片離開視窗時都會取消它。發生例外時匯入會停止，顯示其訊息並提供「Try again」，同時觸發 `Failed`；重試時會重新送出失敗的那一批。所有批次完成後，`Completed`（以及 `CompletedCommand`）會回報總計。

```csharp
wizard.ImportBatchAction = async (records, cancellation) =>
{
    var response = await http.PostAsJsonAsync("/api/contacts/bulk", records.Select(r => r.Values), cancellation);
    response.EnsureSuccessStatusCode();
};

wizard.Completed += (_, e) => toast.Show($"{e.Created + e.Updated} contacts imported");
```

<Demo name="Handlers" />

### 你自己的欄位

把欄位描述為 `ImportTarget`。`Headers` 是確切代表該欄位的名稱，`Synonyms` 是常見的替代名稱，而 `Sniff` 會在標題沒有提示時辨識值的內容。`Validate` 對錯誤的值回傳簡短訊息；`Fix` 回傳安全的清理後值（去除空白、轉為小寫、展開縮寫）或 null，並供「Clean up」使用。用 `IsKey` 標記識別既有記錄的欄位，並以 `ExistingKeys` 傳入已知的鍵值。`Noun` 和 `PluralNoun` 指定文案中記錄的名稱，`SampleCsv` 取代範例檔（設為 null 則隱藏該按鈕）。

<Demo name="CustomFields" />

比對器對以下情況給分：標題等於欄位的標籤、鍵或其中一個 `Headers` 為 99 分；同義詞且值看起來正確為 95 分；只有同義詞為 88 分；只有值為 71 分；只共用一個詞為 64 分。每個欄位會對應到得分最高的那一欄。

### 窄容器

卡片會填滿容器，最寬 760。低於 `CompactWidth`（520）時，步驟列只顯示「Step 1 of 4」計數器與標記，對應列會把下拉選單堆疊在欄名下方，審查表格上方的列則會換行。預覽與審查表格都在各自的框架內橫向捲動。

<Demo name="Narrow" />

### 文字

卡片顯示或播報的每個字詞都是 `ImportWizardLabels` 的屬性；指派新的執行個體給 `Labels` 即可翻譯或改寫。含數字的文字是傳回格式字串的函式：`{0}` 和 `{1}` 標示滾動數字的位置，`{noun}` 則代入單數或複數名詞。

## API 參考

<!-- api: ImportWizard -->

<!-- api: ImportTarget -->

<!-- api: ImportRecord -->

<!-- api: ImportWizardCompletedEventArgs -->

## 無障礙

- 步驟列是名為「Import steps」的清單；每個步驟按鈕會讀出其名稱、編號以及是否已完成（「Upload, step 1 of 4, done」），目前的步驟會被標示。
- 拖放區是名為「Choose a CSV file」的按鈕；預覽與審查表格是可取得焦點的區域，可用方向鍵橫向捲動。
- 每個欄位下拉選單都以其欄名命名（「Field for Email」）。信心度以數字呈現，不只是長條；「Chosen」、「Ignored」和「Duplicate」都是文字。
- 有問題的儲存格會在名稱中帶有訊息（「Email, line 3: Check the email format. Press Enter to edit」）。
- 審查的計數與匯入的進度是禮貌型即時區域；錯誤（檔案無法讀取、缺少欄位、批次失敗）會立即播報。進度條會回報目前的值和總數。

### 鍵盤

- **Tab** 依序經過步驟列中已完成的步驟、該步驟的控制項和頁尾。
- 在拖放區上按 **Enter** 或 **Space** 會開啟檔案選擇器。
- 在審查儲存格上按 **Enter** 或 **Space** 會開啟其編輯器；**Enter** 儲存，**Escape** 取消，焦點回到該儲存格。

## 動態效果

- **卡片**：高度依平滑彈簧隨每個步驟調整。步驟以 28 像素滑動、4 像素模糊，朝前進的方向移動，下一步驟則從另一側滑入。
- **上傳**：拖放區縮小消失的同時，檔案卡片升起；2 像素的線條在 420 毫秒內讀取檔案，之後五列預覽依序升起，每列相隔 40 毫秒。
- **對應**：每欄在解析前會脈動顯示「Matching」，每 117 毫秒解析一欄；分數清晰浮現，其計量條依平滑彈簧填滿。
- **審查**：「Clean up」每 90 毫秒修正一個儲存格，讓計數以串聯的方式向下滾動；修正後的儲存格綠色發光 900 毫秒，在「To fix」中，這些列會一起滑出，其餘的列再向上收合。
- **匯入**：計數與進度條隨每一批更新；摘要的數字只計數一次，其勾選標記會自行繪製。
- 減少動態效果時保留淡入淡出，去掉移動、讀取線和串聯效果，並立即顯示最終數字。關閉動態效果時，所有變化立即生效。

## 響應式

- `ImportWizard` 依自身寬度而非視窗寬度改變版面。寬度低於 `CompactWidth`（預設 520 px）時設定 `:compact`：步驟列顯示計數器與純標記，對應列堆疊，審查列換行。
- 卡片在任何尺寸下最寬為 760 px（`MaxWidth`）。
- 檔名與過長的儲存格值以省略號截斷；表格在各自的框架內橫向捲動，因此頁面本身不會橫向捲動。
- 觸控：沒有任何操作依賴懸停。點擊儲存格即可開啟編輯器，所有操作都是可見的按鈕。

## 相關

- `FileDropzone`、`Select`、`SegmentedControl`、`Progress`、`AnimatedCounter` 和 `InlineEdit`，即此卡片所遵循模式的控制項。
- File upload，用於附加檔案而非匯入其列。
- API keys、Webhooks、Integrations、Env variables、Domain verify 和 Webhook tester，即 Developer 分組裡的其他區塊。
