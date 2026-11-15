---
title: Branch graph
summary: 預覽拉取請求如何落地的 git 提交圖；分支以彩色泳道繪入，變基把提交抬起後重放，壓縮把它們合成一個，懸停提交可查看詳情。
source-hash: a093e51a33fc
---

<Demo name="PullRequest" />

## 何時使用

`BranchGraph` 像 git 用戶端那樣顯示一小段 git 歷史，最新的提交在最上面：每個分支有自己的彩色泳道，分支分出和合併回來的地方是一段
S 形彎線，合併提交畫成圓環並帶合併圖示，每個分支最新的提交上有一個名稱膠囊。標題列的策略控制項預覽拉取請求會怎樣落地：

- **Before** 是目前的歷史，摘要說明分支領先和落後多少個提交。
- **Merge** 在頂部騰出位置放一個合併提交，並把兩條線都畫進它。
- **Rebase** 把拉取請求的提交像卡片一樣抬起，依次沿弧線搬到目標分支的泳道，落下時帶一圈波紋，同時改寫它們的雜湊。
- **Squash** 把這些提交飛向同一個點，那裡出現一個以拉取請求命名的提交。

懸停某個提交，或用方向鍵移到它上面，會顯示一張卡片，列出它的訊息、作者和差異統計，並讓其他分支變暗。

- **程式碼審查和拉取請求頁面**：合併策略選擇器，展示目標分支之後的樣子。
- **開發者工具**：帶懸停詳情和鍵盤瀏覽的提交面板。
- **版本控制或 CI 產品的文件與登陸頁**：講解 merge、rebase 和 squash。

## 何時不用

- **很長的歷史**：每個提交佔一行，沒有虛擬化。它適合約五到十五個提交的一小段歷史；完整日誌請用 `Table`。
- **發行說明**：讀者需要看的是改了什麼而不是分支怎樣合併時，用 `Timeline`。
- **儲存庫的真實狀態**：merge、rebase、squash 檢視是根據資料算出的預覽。頁面可能被當成真實狀態時請說明這一點。

## 用法

沒有提交時，它預覽一個虛構的拉取請求（一個比 `main` 領先三個提交的功能分支，以及之前合併過的一個短修復分支）。首次出現時線條從最舊的提交向上繪出；之後在無人操作時，按
Before、Rebase、Merge、Squash 循環，每次變化穩定後等待 2.6 秒。指標停在提交上會暫停循環，第一次選擇、點按或按鍵會永久停止它。

```xml
<arc:BranchGraph />
```

### 合併預覽

按從舊到新加入 `BranchGraphCommit`；圖中最新的在最上面。`Branch` 決定泳道，`Parents` 給出提交的父提交（兩個即為合併）。`Hash`、`Date`、`Author`、`Body`、`Additions`、`Deletions`
和 `Files` 填充行和詳情卡片。`PullRequestTitle`、`PullRequestNumber`、`SourceBranch` 和 `TargetBranch` 描述拉取請求；不設定時，取最新且尚未合併的分支，並讓它落在第一個分支上。讀者自己操作的面板請設
`AutoPlay="False"`。

<Demo name="MergePreview" />

```xml
<arc:BranchGraph PullRequestTitle="Article search" PullRequestNumber="1024"
                 SourceBranch="feat/search" AutoPlay="False">
    <arc:BranchGraphCommit Id="a41c0e7" Branch="main" Message="Publish the help center" Date="2 days ago" />
    <arc:BranchGraphCommit Id="6d2e9b1" Branch="feat/search" Message="Index articles on save" Date="Yesterday" />
    <arc:BranchGraphCommit Id="f03b58c" Branch="main" Parents="a41c0e7" Message="Upgrade to .NET 10" Date="3 hours ago" />
    <arc:BranchGraphCommit Id="b7a4e20" Branch="feat/search" Message="Rank results by freshness" Date="1 hour ago" />
</arc:BranchGraph>
```

`SelectedStrategy`（雙向）是目前顯示的預覽，`SelectionChanged` 回報每次變化，帶 `OldValue`、`NewValue` 和 `IsUserChange`。`SelectedCommit`（雙向）是卡片被固定的提交。`ItemsSource`
從檢視模型取得提交，首次繪出時每個提交落下都會觸發 `CommitDrawn`，`RevealedCount` 記錄已繪出的數量，`StartsFinished` 一開始就全部繪出且不循環。`Labels` 保存它顯示或朗讀的所有文字。

循環由 `Explainer` 的講解時鐘驅動：循環時 `IsPlaying` 為 true，`Pause()` 暫停，`StepForward()` 切到下一個策略，`Restart()` 重新繪出圖形，`Speed`
縮放所有動畫（0 表示每個狀態立即顯示），`AreControlsVisible` 控制策略控制項的顯示。

## API 參考

<!-- api: BranchGraph -->

<!-- api: BranchGraphCommit -->

<!-- api: BranchGraphStrategy -->

<!-- api: BranchGraphSelectionChangedEventArgs -->

<!-- api: BranchGraphLabels -->

<!-- api: BranchGraphCommitEventArgs -->

`BranchGraphSamples.CreateCommits()` 回傳範例歷史的一份新副本。

## 無障礙

- 卡片是一個群組，名稱由標題和分支組成（"Search filters, history of feat/filters and main"）。
- 策略控制項是一排分段，只佔一個 Tab 停駐點；方向鍵、Home 和 End 會立即選中策略。
- 提交清單只佔一個 Tab 停駐點，最新的在前。用鍵盤進入時檢視最新的提交；上下方向鍵在提交間移動，Home 和 End 跳到第一個和最後一個，Escape 關閉卡片。清單的無障礙名稱是被檢視的提交（含雜湊、作者、日期、差異統計和指向它的分支），沒有時是摘要。
- 讀者每選擇一個策略，禮貌的即時區域都會朗讀摘要；自動循環不朗讀。
- 不只依賴顏色：膠囊裡寫著分支名，合併提交有圓環和圖示，差異統計帶 + 和 − 號。

## 動效

| 變化 | 完整 | 減弱 | 關閉 |
|---|---|---|---|
| 首次出現 | 線條從最舊的提交向上繪出，線到達時提交落下 | 立即顯示 | 立即顯示 |
| Rebase 與 Squash | 提交抬起成卡片沿弧線飛行，間隔 0.15 秒，落下時帶一圈波紋 | 直接跳到 | 直接跳到 |
| Merge | 各行騰出位置，合併提交落下，兩條線畫進它 | 直接跳到 | 直接跳到 |
| 改寫雜湊 | 新雜湊以強調色升起，再淡為次要色 | 原地變化 | 原地變化 |
| 檢視提交 | 其他分支變暗，圓點放大，卡片淡入 | 立即 | 立即 |
| 自動循環 | 執行 | 關閉 | 關閉 |

每次選擇都從目前狀態出發，所以在動畫中途換選策略也不會跳變。`Speed` 調整節奏。

## 響應式

- 最多三條泳道時圖形保留 80 px 的邊欄；每行 44 px，訊息過長時以省略號截斷。
- 寬度小於 720 px 時隱藏日期欄，小於 560 px 時隱藏作者，小於 400 px 時隱藏雜湊；卡片和無障礙名稱中仍保留這些資訊。寬度小於 600 px 時策略控制項單獨佔一行。
- 點按提交會固定它的卡片，再點它或點清單其他位置則關閉。

## 相關元件

| 元件 | 適用場景 |
|---|---|
| `Timeline` | 按順序排列的事件，沒有分支 |
| `ReasoningTree` | 講的是一個會分支和剪枝的搜尋 |
| `AgentTimeline` | 多個智慧體隨時間協作 |
