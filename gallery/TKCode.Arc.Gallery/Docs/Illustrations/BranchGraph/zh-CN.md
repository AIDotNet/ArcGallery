---
title: Branch graph
summary: 预览拉取请求如何落地的 git 提交图；分支以彩色泳道绘入，变基把提交抬起后重放，压缩把它们合成一个，悬停提交可查看详情。
source-hash: a093e51a33fc
---

<Demo name="PullRequest" />

## 何时使用

`BranchGraph` 像 git 客户端那样显示一小段 git 历史，最新的提交在最上面：每个分支有自己的彩色泳道，分支分出和合并回来的地方是一段
S 形弯线，合并提交画成圆环并带合并图标，每个分支最新的提交上有一个名称胶囊。标题栏的策略控件预览拉取请求会怎样落地：

- **Before** 是当前的历史，摘要说明分支领先和落后多少个提交。
- **Merge** 在顶部腾出位置放一个合并提交，并把两条线都画进它。
- **Rebase** 把拉取请求的提交像卡片一样抬起，依次沿弧线搬到目标分支的泳道，落下时带一圈波纹，同时改写它们的哈希。
- **Squash** 把这些提交飞向同一个点，那里出现一个以拉取请求命名的提交。

悬停某个提交，或用方向键移到它上面，会显示一张卡片，列出它的消息、作者和差异统计，并让其他分支变暗。

- **代码评审和拉取请求页面**：合并策略选择器，展示目标分支之后的样子。
- **开发者工具**：带悬停详情和键盘浏览的提交面板。
- **版本控制或 CI 产品的文档与落地页**：讲解 merge、rebase 和 squash。

## 何时不用

- **很长的历史**：每个提交占一行，没有虚拟化。它适合约五到十五个提交的一小段历史；完整日志请用 `Table`。
- **发布说明**：读者需要看的是改了什么而不是分支怎样合并时，用 `Timeline`。
- **仓库的真实状态**：merge、rebase、squash 视图是根据数据算出的预览。页面可能被当成真实状态时请说明这一点。

## 用法

没有提交时，它预览一个虚构的拉取请求（一个比 `main` 领先三个提交的功能分支，以及之前合并过的一个短修复分支）。首次出现时线条从最旧的提交向上绘出；之后在无人操作时，按
Before、Rebase、Merge、Squash 循环，每次变化稳定后等待 2.6 秒。指针停在提交上会暂停循环，第一次选择、点按或按键会永久停止它。

```xml
<arc:BranchGraph />
```

### 合并预览

按从旧到新添加 `BranchGraphCommit`；图中最新的在最上面。`Branch` 决定泳道，`Parents` 给出提交的父提交（两个即为合并）。`Hash`、`Date`、`Author`、`Body`、`Additions`、`Deletions`
和 `Files` 填充行和详情卡片。`PullRequestTitle`、`PullRequestNumber`、`SourceBranch` 和 `TargetBranch` 描述拉取请求；不设置时，取最新且尚未合并的分支，并让它落在第一个分支上。读者自己操作的面板请设
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

`SelectedStrategy`（双向）是当前显示的预览，`SelectionChanged` 报告每次变化，带 `OldValue`、`NewValue` 和 `IsUserChange`。`SelectedCommit`（双向）是卡片被固定的提交。`ItemsSource`
从视图模型获取提交，首次绘出时每个提交落下都会触发 `CommitDrawn`，`RevealedCount` 记录已绘出的数量，`StartsFinished` 一开始就全部绘出且不循环。`Labels` 保存它显示或朗读的所有文字。

循环由 `Explainer` 的讲解时钟驱动：循环时 `IsPlaying` 为 true，`Pause()` 暂停，`StepForward()` 切到下一个策略，`Restart()` 重新绘出图形，`Speed`
缩放所有动画（0 表示每个状态立即显示），`AreControlsVisible` 控制策略控件的显示。

## API 参考

<!-- api: BranchGraph -->

<!-- api: BranchGraphCommit -->

<!-- api: BranchGraphStrategy -->

<!-- api: BranchGraphSelectionChangedEventArgs -->

<!-- api: BranchGraphLabels -->

<!-- api: BranchGraphCommitEventArgs -->

`BranchGraphSamples.CreateCommits()` 返回示例历史的一份新副本。

## 无障碍

- 卡片是一个分组，名称由标题和分支组成（"Search filters, history of feat/filters and main"）。
- 策略控件是一排分段，只占一个 Tab 停靠点；方向键、Home 和 End 会立即选中策略。
- 提交列表只占一个 Tab 停靠点，最新的在前。用键盘进入时检视最新的提交；上下方向键在提交间移动，Home 和 End 跳到第一个和最后一个，Escape 关闭卡片。列表的无障碍名称是被检视的提交（含哈希、作者、日期、差异统计和指向它的分支），没有时是摘要。
- 读者每选择一个策略，礼貌的实时区域都会朗读摘要；自动循环不朗读。
- 不只依赖颜色：胶囊里写着分支名，合并提交有圆环和图标，差异统计带 + 和 − 号。

## 动效

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 首次出现 | 线条从最旧的提交向上绘出，线到达时提交落下 | 立即显示 | 立即显示 |
| Rebase 与 Squash | 提交抬起成卡片沿弧线飞行，间隔 0.15 秒，落下时带一圈波纹 | 直接跳到 | 直接跳到 |
| Merge | 各行腾出位置，合并提交落下，两条线画进它 | 直接跳到 | 直接跳到 |
| 改写哈希 | 新哈希以强调色升起，再淡为次要色 | 原地变化 | 原地变化 |
| 检视提交 | 其他分支变暗，圆点放大，卡片淡入 | 立即 | 立即 |
| 自动循环 | 运行 | 关闭 | 关闭 |

每次选择都从当前状态出发，所以在动画中途换选策略也不会跳变。`Speed` 调整节奏。

## 响应式

- 最多三条泳道时图形保留 80 px 的边栏；每行 44 px，消息过长时以省略号截断。
- 宽度小于 720 px 时隐藏日期列，小于 560 px 时隐藏作者，小于 400 px 时隐藏哈希；卡片和无障碍名称中仍保留这些信息。宽度小于 600 px 时策略控件单独占一行。
- 点按提交会固定它的卡片，再点它或点列表其他位置则关闭。

## 相关组件

| 组件 | 适用场景 |
|---|---|
| `Timeline` | 按顺序排列的事件，没有分支 |
| `ReasoningTree` | 讲的是一个会分支和剪枝的搜索 |
| `AgentTimeline` | 多个智能体随时间协作 |
