---
title: Code review
summary: 單張卡片內的拉取請求審查：檔案標籤記錄哪些檔案已看過，語法著色的差異裡任何一行都能留言，已解決的討論摺疊成一行，建議的修改直接套用到差異中，Approve 按鈕會變成 Approved。
source-hash: 4d1a0603bc22
---

<Demo name="Preview" />

## 何時使用

`CodeReview` 把一次完整的拉取請求審查放進一張卡片，適合在變更上線前需要有人檢查的工具：

- **審查拉取請求**，對程式碼行留言並提出修改建議。
- **在內部工具裡逐個檔案核准產生的變更。**
- **審查佇列**，需要清楚哪些檔案已經看過、哪些討論仍未解決。

## 何時不使用

- **展示一段沒人留言的程式碼。** 請用 `CodeBlock`。
- **跟隨智慧代理即時撰寫程式碼。** 請用 Agent run 區塊。
- **比較兩份一般文件**而不需要審查操作；一般的差異檢視就夠了。

## 用法

卡片衍生自 Avalonia 的 `TemplatedControl`，不需要任何資料：未設定屬性時，它展示一個範例拉取請求，包含五個檔案、一個已解決的討論和一個帶修改建議的未解決討論，並模擬核准。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:CodeReview Pull="{Binding Pull}" Files="{Binding Files}" Threads="{Binding Threads}" />
</UserControl>
```

### 資料

- `Pull` 是一個 `CodeReviewPullRequest`：標題、編號、作者、目標分支與來源分支以及提交數，顯示在頁首。
- `Files` 列出變更的檔案，依此順序每個檔案一個標籤。`CodeReviewFile` 包含 id、路徑、用於著色的語言（`ts`、`md`、`csharp`，以及程式碼醒目提示器認得的任何語言）和以 `CodeReviewDiffLine` 表示的差異：一個帶舊檔案與新檔案起始行號的區塊標頭，後面接未變更、新增和刪除的行。行號由區塊標頭推算，所以改寫一行後其餘行會重新編號。
- `Threads` 是既有的討論。每個 `CodeReviewThread` 透過行的 id（而不是行號，因此建議改寫檔案後它仍留在原處）掛在某一行下方，帶有 "Line 9" 這樣的標籤、留言、是否已解決，以及選用的 `CodeReviewSuggestion`（要取代的行 id 和新文字）。
- `Viewer` 是正在審查的人；新留言和回覆都以此人的身分送出。

卡片先顯示第一個有未解決討論的檔案；`SelectedFileId` 可以選擇其他檔案，並隨標籤切換而更新。

### 審查

- **對某一行留言**：指標移到一行上，行號邊緣會出現一個加號；點按它（或聚焦行號後按 Enter）會在這一行下方開啟留言框。"Suggest a change" 會加入一個填好該行內容的程式碼輸入框。新討論在審查被核准之前顯示 "Pending"，並透過 `CommentAdded` 回報。
- **討論**："Reply" 就地開啟回覆框。"Resolve" 把討論摺疊成帶參與者頭像的一行；點按這一行可以重新展開，然後可以 "Hide" 或 "Reopen"。`ReplyPosted` 和 `ResolvedChanged` 回報每次變化。
- **修改建議**："Apply suggestion" 取代差異中的對應行，把討論標記為已解決，並在稍後摺疊它。`SuggestionApplied` 交出檔案的新差異，讓你的伺服器提交。
- **已檢視**：檔案標頭裡的核取方塊把檔案標記為已讀；它的標籤顯示一個勾號，進度隨之增加。

### 連接你的伺服器

`SubmitReviewAction` 送出核准以及審查期間新增的留言。工作執行期間按鈕顯示進度；擲回例外時按鈕變成 "Try again"。送出成功後按鈕顯示 "Approved"，頁首下方展開一條狀態列並引發 `ReviewSubmitted`；"Undo" 撤回核准並引發 `ApprovalWithdrawn`。未設定該動作時，送出是模擬的。

```csharp
review.SubmitReviewAction = async (submission, cancellation) =>
{
    var response = await http.PostAsJsonAsync($"/api/pulls/{pull.Number}/reviews",
        new { body = submission.Body, comments = submission.Comments.Select(t => t.Comments[0].Text) }, cancellation);
    response.EnsureSuccessStatusCode();
};

review.SuggestionApplied += (_, e) => commits.Queue(e.File.Path, e.Lines);
```

### 文字

卡片顯示或播報的每個詞都是 `CodeReviewLabels` 的屬性；給 `Labels` 指派一個新執行個體即可翻譯或改寫。帶數字或名字的文字是函式，因此可以調整詞序。

## API 參考

<!-- api: CodeReview -->

<!-- api: CodeReviewFile -->

<!-- api: CodeReviewThread -->

## 無障礙

- 卡片以拉取請求命名（"Review of pull request 3172"），標題是一個標題層級元素。
- 檔案是一個名為 "Changed files" 的標籤清單，只占一個 Tab 停駐點；每個標籤讀出路徑、是否已檢視以及未解決討論的數量（"src/mail/limiter.ts, viewed, 1 open conversation"）。
- 每個行號都是以該行命名的按鈕（"Comment on added line 9"、"Comment on deleted line 27"），因此不看顏色也能正確理解差異。
- Viewed 和 "Suggest a change" 是會回報狀態的核取方塊；摺疊的討論是一個可展開它的按鈕。
- 新增留言與回覆、解決討論、套用建議、標記檔案已檢視以及核准，都會在禮貌型即時區域中播報。

### 鍵盤

- **Tab** 依序經過 Approve 按鈕、檔案標籤、Viewed 核取方塊、每個檔案的一個行號以及討論裡的操作。
- 在標籤中按 **Left** / **Right**、**Home** / **End** 選擇上一個、下一個、第一個或最後一個檔案。
- 在行號上按 **Up** / **Down**、**Home** / **End** 移到上一行、下一行、第一行或最後一行。
- 在行號上按 **Enter** 或 **Space** 開啟或關閉該行的留言框。
- **Ctrl** + **Enter**（macOS 上為 **Cmd** + **Enter**）送出正在撰寫的留言或回覆；**Escape** 關閉留言框並回到行號。

## 動態效果

- **條帶**：留言框、討論和核准狀態都在差異的文件流中展開；高度以一個臨界阻尼彈簧（約 420 ms）跟隨內容，下方的行會滑動而不是跳動，內容隨之淡入。
- **送出留言**：留言框保留所在的條帶並變成新的討論，高度在兩者之間以彈簧過渡。
- **套用建議**：舊行摺疊消失，新行在同一位置展開，並帶著較深的綠色停留片刻，再緩和到一般的新增行顏色；"Apply suggestion" 變成 "Applied"，討論在 650 ms 後摺疊。
- **新增留言標記**：指標到達 70 ms 後在行號邊緣從 60% 放大出現，因此指標只是經過時不會點亮任何行；行號隨之變亮。
- **標籤**：醒目區塊在檔案之間滑動，面板交叉淡化並沿切換方向滑動 16 px，高度隨彈簧變化；已檢視進度以同一彈簧增加，計數滾動變化。
- **Approve** 先顯示轉圈，然後標籤和寬度變為 Approved，頁首下方展開狀態列。
- 減少動態效果時所有變化立即生效，只保留短暫的淡入淡出；關閉動態效果時全部立即生效。各個等級下焦點與播報都相同。

## 響應式

- `CodeReview` 依自身寬度改變版面。低於 `CompactWidth`（預設 560 px）時設定 `:compact`：頁首直向排列，Approve 位於標題下方，進度移到標籤上方，Viewed 只顯示核取方塊，行號只剩一欄。
- 卡片最寬 760 px。標籤放不下時可以橫向捲動。長行從不換行：程式碼橫向捲動（Shift + 滾輪、橫向滾輪或差異下方的捲軸），行號保持固定，討論保持卡片寬度。
- 觸控：新增留言標記不依賴懸停；點按行號即可開啟留言框。

## 相關

- `CodeBlock`，差異所用的程式碼醒目提示（透過 `UseArcHighlighting()` 安裝）。
- `Tabs`、`Checkbox`、`Button`、`Textarea` 和 `AnimatedCounter`，卡片沿用了它們的互動模式。
- API keys、Webhooks、Integrations、Import wizard、Env variables、Domain verify 和 Webhook tester，其他開發者區塊。
