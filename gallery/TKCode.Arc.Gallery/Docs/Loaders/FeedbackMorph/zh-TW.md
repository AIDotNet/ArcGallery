---
title: Feedback morph
summary: 一個意見回饋按鈕：彈開成帶心情表情和留言的小表單，再收成一枚致謝膠囊，始終是同一個形狀。
source-hash: a464e8b69b24
---

<Demo name="Preview" />

## 何時使用

`FeedbackMorph` 讓人在目前的位置快速留下意見，不必離開頁面。一枚帶對話氣泡的小膠囊放在內容旁邊。點選後膠囊長成一塊面板：一個問題、一排從「不滿」到「驚喜」的五個表情、一段留言和傳送按鈕。面板的各列依序進入，表情從中間向兩側展開。留言傳送期間按鈕轉圈；隨後面板收成一枚致謝膠囊，勾號自己畫出來，幾秒後膠囊再變回按鈕。

- **就地收集對某個頁面或功能的意見**：更新日誌條目、新畫面、文件文章。
- **心情加一段留言**，五級量表已經足夠、留言可寫可不寫的場合。
- **任務完成後的輕量滿意度詢問**，不需要問卷或對話框。

## 何時不用

- **需要聯絡資訊、附件或必填欄位的表單**：使用帶表單的 `Dialog`。
- **單獨的評分**：使用 `Rating`。
- **其他錨定內容**：使用 `Popover`。

## 用法

為它設定問題和傳送意見的工作。`SubmitHandler` 收到一個 `FeedbackMorphSubmission`，其中有所選的心情（`Mood` 是它的 `Value`）和去掉首尾空白的留言。任務執行期間傳送按鈕轉圈，留言唯讀。任務完成後顯示致謝膠囊；任務擲回例外時，面板保持開啟，顯示錯誤並保留草稿。在此期間關閉面板會取消權杖。

```xml
<arc:FeedbackMorph x:Name="Feedback"
                   Title="How do release notes read?"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Top" />
```

```csharp
Feedback.SubmitHandler = async (submission, cancellation) =>
{
    var body = JsonContent.Create(new { mood = submission.Mood, message = submission.Message });
    var response = await http.PostAsync("/api/feedback", body, cancellation);
    response.EnsureSuccessStatusCode();
};
```

`FeedbackMorph` 位於 Special 套件中：在 `UseArcControls()` 旁邊用 `UseArcSpecial()` 註冊。控制項在版面中只佔按鈕的大小。面板（寬 352 px）和致謝膠囊會越過它、蓋在周圍的內容上，所以要把它放在有空間的位置，並放在它要覆蓋的內容之上。

### 方向與錨點

`Side` 決定面板往哪邊長：`Bottom`（預設）保持按鈕的上邊緣不動、向下長；`Top` 保持下邊緣不動、向上長，適合靠近頁面底部的按鈕。`Align` 讓面板錨定在按鈕的 `Start`、`Center`（預設）或 `End`，靠近一側的按鈕因此可以向內展開。預覽可以在「頂部按鈕向下展開」和「底部按鈕向上展開」之間切換。

### 心情

不設定 `Moods` 時，顯示五個內建表情（`FeedbackMood.CreateDefaults()`），從不滿到驚喜。傳入自己的量表即可更換表情、名稱以及每個表情在留言中提出的問題；五個最好讀，三個以上都可以。表情可以是內建圖形之一（`Face`），也可以是你自己的 24 × 24 描邊幾何（`Glyph`）。

```xml
<arc:FeedbackMorph Title="Did this article help?">
    <arc:FeedbackMorph.Moods>
        <arc:FeedbackMoodCollection>
            <arc:FeedbackMood Value="no" Label="Not really" Face="Sad" Prompt="What were you looking for?" />
            <arc:FeedbackMood Value="some" Label="Somewhat" Face="Neutral" Prompt="What was missing?" />
            <arc:FeedbackMood Value="yes" Label="Yes" Face="Happy" Prompt="Anything we should add?" />
        </arc:FeedbackMoodCollection>
    </arc:FeedbackMorph.Moods>
</arc:FeedbackMorph>
```

### 狀態

| 狀態 | 顯示內容 |
|---|---|
| `Closed` | 按鈕：對話氣泡和 `Label` |
| `Open` | 面板：`Title`、表情及其說明、帶提示的留言、狀態列和傳送按鈕 |
| `Sending` | 傳送按鈕轉圈並顯示 `SendingLabel`；留言唯讀，表情變淡 |
| `Sent` | 顯示 `SuccessLabel` 的致謝膠囊，停留 `ResetAfter`（3.2 秒）後變回按鈕 |

有留言後，傳送按鈕旁的狀態列倒數剩餘字數（`MaxLength`，預設 500）。沒有選表情、留言也為空時按傳送不會傳送：表情輕輕一頓，狀態列顯示 `EmptyHint`。傳送失敗時狀態列顯示 `ErrorMessage`，直到留言被修改。

關閉面板時草稿（所選表情和留言）保留，傳送成功後清空。`SelectedMood` 和 `Message` 保存草稿並支援雙向繫結；`SelectionChanged` 回報新選的表情，`Submitted` 回報一次傳送，`StateChanged` 回報 `State` 的每次變化，傳送失敗時附帶例外。`Open()`、`Close()`、`Submit()` 和 `ClearDraft()` 在程式碼中完成同樣的事，`IsOpen` 也可以開啟和關閉它。

### 內容準則

- **Label**：兩三個詞的邀請，例如 "Give feedback"、"Rate this page"。
- **Title**：一個關於按鈕旁內容的簡短問題，例如 "How do release notes read?"
- **Prompts**：每個表情一個追問細節的問題，表情越開心語氣越溫和，例如 "What went wrong?"、"What should we keep doing?"
- **SuccessLabel**：一句簡短、有人情味、能放進一行的致謝。

## API 參考

<!-- api: FeedbackMorph -->

`FeedbackMood` 有 `Value`（`FeedbackMorphSubmission.Mood` 攜帶的值）、`Label`（說明文字和表情的無障礙名稱）、`Prompt`（選中時留言中顯示的問題）、`Face`（`Upset`、`Sad`、`Neutral`、`Happy` 或 `Delighted`）、選用的 `Glyph`，以及存放自訂資料的 `Tag`。

## 無障礙

- 按鈕以 `Label` 命名。面板開啟期間，按鈕離開 Tab 順序和無障礙樹，面板以 `Title` 命名。
- 表情是一個單選群組（以 `MoodsLabel` 命名），其中的選項按鈕以各自的名稱命名，只有一個 Tab 停駐點：已選的表情，或第一個表情。
- 留言以 `Placeholder` 命名，說明文字是它目前顯示的提示。傳送按鈕宣告 Ctrl+Enter 為快速鍵。
- 一個禮貌的即時區域播報 `SendingLabel`、`EmptyHint`、`ErrorMessage` 和 `SuccessLabel`。
- 面板開啟時，焦點移到已選的表情或第一個表情。Escape 和關閉按鈕把焦點還給按鈕。傳送後致謝膠囊取得焦點，膠囊收回時焦點回到按鈕。
- 指標取得的焦點不畫焦點環。鍵盤焦點在按鈕、表情或傳送按鈕外 2 px 處畫一圈 2 px 的焦點環。

### 鍵盤

| 按鍵 | 操作 |
|---|---|
| 在按鈕上按 `Enter` / `Space` | 開啟面板 |
| `→` `↓` / `←` `↑` | 選下一個或上一個表情（循環） |
| `Home` / `End` | 選第一個或最後一個表情 |
| `Tab` | 在表情、留言和傳送按鈕之間移動；從傳送按鈕再按 Tab 會收起面板 |
| 在留言中按 `Ctrl+Enter`（`⌘+Enter`） | 傳送 |
| `Escape` | 把面板收回按鈕，保留草稿 |

在面板外按下指標也會收起它。

## 動態效果

- 同一個形狀在按鈕、面板和致謝膠囊的尺寸之間彈動（0.36 秒的臨界阻尼彈簧），圓角始終是 20 px，所以 40 px 高的膠囊是完整的圓端。陰影在 360 ms 內隨之變大。
- 開啟：按鈕內容在 100 ms 內淡出並模糊；面板晚 60 ms 淡入，從 4 px 模糊變清晰，並從 0.92 放大。各列從 80 ms 起依序進入，間隔 50 ms（標題、表情、留言、操作），每列從上方 10 px（向上展開時從下方）落定並脫離輕微模糊；表情從 0.7 放大，從中間向兩側，間隔 35 ms。
- 選擇表情：一塊柔和的圓形醒目提示滑到它下面，所選表情放大到 1.14，其他縮小到 0.92，說明文字和提示短暫上移切換。剩餘字數逐位滾動。
- 傳送："Send" 向上離開，轉圈和 "Sending" 升到它的位置。空傳送讓表情橫向輕踢幾個像素。
- 成功：面板在 80 ms 內淡出，形狀同時縮成膠囊；勾號的圓環在 280 ms 內畫出，接著畫出勾，文字逐詞升起，間隔 40 ms。收回時膠囊在 70 ms 內淡出，按鈕晚 60 ms 出現。

| 變化 | 完整 | 減弱 | 關閉 |
|---|---|---|---|
| 形狀尺寸 | 彈簧 | 立即 | 立即 |
| 形狀中的內容 | 淡變、模糊、縮放、逐列進入 | 短暫交叉淡變 | 立即 |
| 醒目提示、表情縮放 | 彈簧 | 立即 | 立即 |
| 按鈕文字、說明、狀態列 | 上移並淡變 | 淡變 | 立即 |
| 空傳送輕踢 | 輕踢 | 無 | 無 |

## 響應式行為

- 面板寬 352 px；檢視區減去兩側各 16 px 更窄時取後者，所以能放進手機。靠近邊緣時用 `Side` 和 `Align` 讓它留在檢視區內。
- 表情是等寬欄中 44 px 的點選目標。只有滑鼠或手寫筆懸停時才會點亮表情並顯示名稱。

## 相關元件

- `ActionMorph`：浮動按鈕展開成快速操作選單及其表單。
- `Rating`：單獨的評分。
- `Popover`：其他錨定內容。

| 元件 | 何時選擇 |
|---|---|
| `FeedbackMorph` | 就地收集心情和選用留言，並致謝 |
| `ActionMorph` | 一個浮動按鈕背後的多個快速操作 |
| `Rating` | 單獨的星級評分 |
