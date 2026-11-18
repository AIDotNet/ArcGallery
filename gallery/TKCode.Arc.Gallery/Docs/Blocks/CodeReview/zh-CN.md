---
title: Code review
summary: 单张卡片内的拉取请求评审：文件标签记录哪些文件已看过，语法着色的差异里任何一行都能评论，已解决的讨论折叠成一行，建议的修改直接应用到差异中，Approve 按钮会变成 Approved。
source-hash: 4d1a0603bc22
---

<Demo name="Preview" />

## 何时使用

`CodeReview` 把一次完整的拉取请求评审放进一张卡片，适合在改动上线前需要有人检查的工具：

- **评审拉取请求**，对代码行发表评论并提出修改建议。
- **在内部工具里逐个文件批准生成的改动。**
- **评审队列**，需要清楚哪些文件已经看过、哪些讨论仍未解决。

## 何时不使用

- **展示一段没人评论的代码。** 请用 `CodeBlock`。
- **跟随智能体实时写代码。** 请用 Agent run 区块。
- **比较两份普通文档**而不需要评审操作；普通的差异视图就够了。

## 用法

卡片派生自 Avalonia 的 `TemplatedControl`，不需要任何数据：不设置属性时，它展示一个示例拉取请求，包含五个文件、一个已解决的讨论和一个带修改建议的未解决讨论，并模拟批准。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:CodeReview Pull="{Binding Pull}" Files="{Binding Files}" Threads="{Binding Threads}" />
</UserControl>
```

### 数据

- `Pull` 是一个 `CodeReviewPullRequest`：标题、编号、作者、目标分支与来源分支以及提交数，显示在页头。
- `Files` 列出改动的文件，按此顺序每个文件一个标签。`CodeReviewFile` 包含 id、路径、用于着色的语言（`ts`、`md`、`csharp`，以及代码高亮器认识的任何语言）和以 `CodeReviewDiffLine` 表示的差异：一个带旧文件与新文件起始行号的块头，后面跟未改动、新增和删除的行。行号由块头推算，所以改写一行后其余行会重新编号。
- `Threads` 是已有的讨论。每个 `CodeReviewThread` 通过行的 id（而不是行号，因此建议改写文件后它仍留在原处）挂在某一行下面，带有 "Line 9" 这样的标签、评论、是否已解决，以及可选的 `CodeReviewSuggestion`（要替换的行 id 和新文本）。
- `Viewer` 是正在评审的人；新评论和回复都以此人的身份发出。

卡片先显示第一个有未解决讨论的文件；`SelectedFileId` 可以选择其他文件，并随标签切换而更新。

### 评审

- **评论某一行**：指针移到一行上，行号边缘会出现一个加号；点击它（或聚焦行号后按 Enter）会在这一行下面打开评论框。"Suggest a change" 会加入一个填好该行内容的代码输入框。新讨论在评审被批准之前显示 "Pending"，并通过 `CommentAdded` 报告。
- **讨论**："Reply" 就地打开回复框。"Resolve" 把讨论折叠成带参与者头像的一行；点击这一行可以重新展开，然后可以 "Hide" 或 "Reopen"。`ReplyPosted` 和 `ResolvedChanged` 报告每次变化。
- **修改建议**："Apply suggestion" 替换差异中的对应行，把讨论标记为已解决，并在稍后折叠它。`SuggestionApplied` 交出文件的新差异，以便你的服务器提交。
- **已查看**：文件头里的复选框把文件标记为已读；它的标签显示一个对勾，进度随之增长。

### 连接你的服务器

`SubmitReviewAction` 发送批准以及评审期间新增的评论。任务运行期间按钮显示进度；抛出异常时按钮变成 "Try again"。发送成功后按钮显示 "Approved"，页头下方展开一条状态栏并触发 `ReviewSubmitted`；"Undo" 撤回批准并触发 `ApprovalWithdrawn`。未设置该动作时，发送是模拟的。

```csharp
review.SubmitReviewAction = async (submission, cancellation) =>
{
    var response = await http.PostAsJsonAsync($"/api/pulls/{pull.Number}/reviews",
        new { body = submission.Body, comments = submission.Comments.Select(t => t.Comments[0].Text) }, cancellation);
    response.EnsureSuccessStatusCode();
};

review.SuggestionApplied += (_, e) => commits.Queue(e.File.Path, e.Lines);
```

### 文本

卡片显示或播报的每个词都是 `CodeReviewLabels` 的属性；给 `Labels` 赋一个新实例即可翻译或改写。带数字或名字的文本是函数，因此可以调整词序。

## API 参考

<!-- api: CodeReview -->

<!-- api: CodeReviewFile -->

<!-- api: CodeReviewThread -->

## 无障碍

- 卡片以拉取请求命名（"Review of pull request 3172"），标题是一个标题级元素。
- 文件是一个名为 "Changed files" 的标签列表，只占一个 Tab 停靠点；每个标签读出路径、是否已查看以及未解决讨论的数量（"src/mail/limiter.ts, viewed, 1 open conversation"）。
- 每个行号都是以该行命名的按钮（"Comment on added line 9"、"Comment on deleted line 27"），因此不看颜色也能正确理解差异。
- Viewed 和 "Suggest a change" 是会报告状态的复选框；折叠的讨论是一个可展开它的按钮。
- 添加评论与回复、解决讨论、应用建议、标记文件已查看以及批准，都会在礼貌型实时区域中播报。

### 键盘

- **Tab** 依次经过 Approve 按钮、文件标签、Viewed 复选框、每个文件的一个行号以及讨论里的操作。
- 在标签中按 **Left** / **Right**、**Home** / **End** 选择上一个、下一个、第一个或最后一个文件。
- 在行号上按 **Up** / **Down**、**Home** / **End** 移到上一行、下一行、第一行或最后一行。
- 在行号上按 **Enter** 或 **Space** 打开或关闭该行的评论框。
- **Ctrl** + **Enter**（macOS 上为 **Cmd** + **Enter**）发出正在编写的评论或回复；**Escape** 关闭评论框并回到行号。

## 动效

- **条带**：评论框、讨论和批准状态都在差异的文档流中展开；高度以一个临界阻尼弹簧（约 420 ms）跟随内容，下方的行会滑动而不是跳动，内容随之淡入。
- **发表评论**：评论框保留所在的条带并变成新的讨论，高度在两者之间弹簧过渡。
- **应用建议**：旧行折叠消失，新行在同一位置展开，并带着较深的绿色停留片刻，再缓和到普通的新增行颜色；"Apply suggestion" 变成 "Applied"，讨论在 650 ms 后折叠。
- **添加评论标记**：指针到达 70 ms 后在行号边缘从 60% 放大出现，因此指针只是经过时不会点亮任何行；行号随之变亮。
- **标签**：高亮在文件之间滑动，面板交叉淡化并沿切换方向滑动 16 px，高度随弹簧变化；已查看进度以同一弹簧增长，计数滚动变化。
- **Approve** 先显示转圈，然后标签和宽度变为 Approved，页头下方展开状态栏。
- 减弱动效时所有变化立即生效，只保留短暂的淡入淡出；关闭动效时全部立即生效。各个等级下焦点与播报都相同。

## 响应式行为

- `CodeReview` 按自身宽度改变布局。低于 `CompactWidth`（默认 560 px）时设置 `:compact`：页头纵向排列，Approve 位于标题下方，进度移到标签上方，Viewed 只显示复选框，行号只剩一列。
- 卡片最宽 760 px。标签放不下时可以横向滚动。长行从不换行：代码横向滚动（Shift + 滚轮、横向滚轮或差异下方的滚动条），行号保持固定，讨论保持卡片宽度。
- 触屏：添加评论标记不依赖悬停；点按行号即可打开评论框。

## 相关

- `CodeBlock`，差异所用的代码高亮（通过 `UseArcHighlighting()` 安装）。
- `Tabs`、`Checkbox`、`Button`、`Textarea` 和 `AnimatedCounter`，卡片沿用了它们的交互模式。
- API keys、Webhooks、Integrations、Import wizard、Env variables、Domain verify 和 Webhook tester，其他开发者区块。
