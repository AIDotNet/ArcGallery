---
title: Feedback morph
summary: 一个反馈按钮：弹开成带心情表情和留言的小表单，再收成一枚致谢胶囊，始终是同一个形状。
source-hash: a464e8b69b24
---

<Demo name="Preview" />

## 何时使用

`FeedbackMorph` 让人在当前位置快速留下反馈，不必离开页面。一枚带对话气泡的小胶囊放在内容旁边。点击后胶囊长成一块面板：一个问题、一排从"不满"到"惊喜"的五个表情、一段留言和发送按钮。面板的各行依次进入，表情从中间向两侧展开。留言发送期间按钮转圈；随后面板收成一枚致谢胶囊，对勾自己画出来，几秒后胶囊再变回按钮。

- **就地收集对某个页面或功能的反馈**：更新日志条目、新界面、文档文章。
- **心情加一段留言**，五级量表已经足够、留言可写可不写的场合。
- **任务完成后的轻量满意度询问**，不需要问卷或对话框。

## 何时不用

- **需要联系方式、附件或必填项的表单**：使用带表单的 `Dialog`。
- **单独的评分**：使用 `Rating`。
- **其他锚定内容**：使用 `Popover`。

## 用法

为它设置问题和发送反馈的工作。`SubmitHandler` 收到一个 `FeedbackMorphSubmission`，其中有所选的心情（`Mood` 是它的 `Value`）和去掉首尾空白的留言。任务运行期间发送按钮转圈，留言只读。任务完成后显示致谢胶囊；任务抛出异常时，面板保持打开，显示错误并保留草稿。在此期间关闭面板会取消令牌。

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

`FeedbackMorph` 位于 Special 包中：在 `UseArcControls()` 旁边用 `UseArcSpecial()` 注册。控件在布局中只占按钮的大小。面板（宽 352 px）和致谢胶囊会越过它、盖在周围的内容上，所以要把它放在有空间的位置，并放在它要覆盖的内容之上。

### 方向与锚点

`Side` 决定面板向哪边长：`Bottom`（默认）保持按钮的上边缘不动、向下长；`Top` 保持下边缘不动、向上长，适合靠近页面底部的按钮。`Align` 让面板锚定在按钮的 `Start`、`Center`（默认）或 `End`，靠近一侧的按钮因此可以向内展开。预览可以在"顶部按钮向下展开"和"底部按钮向上展开"之间切换。

### 心情

不设置 `Moods` 时，显示五个内置表情（`FeedbackMood.CreateDefaults()`），从不满到惊喜。传入自己的量表即可更换表情、名称以及每个表情在留言中提出的问题；五个最好读，三个以上都可以。表情可以是内置图形之一（`Face`），也可以是你自己的 24 × 24 描边几何（`Glyph`）。

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

### 状态

| 状态 | 显示内容 |
|---|---|
| `Closed` | 按钮：对话气泡和 `Label` |
| `Open` | 面板：`Title`、表情及其说明、带提示的留言、状态行和发送按钮 |
| `Sending` | 发送按钮转圈并显示 `SendingLabel`；留言只读，表情变淡 |
| `Sent` | 显示 `SuccessLabel` 的致谢胶囊，停留 `ResetAfter`（3.2 秒）后变回按钮 |

有留言后，发送按钮旁的状态行倒数剩余字数（`MaxLength`，默认 500）。没有选表情、留言也为空时点发送不会发送：表情轻轻一顿，状态行显示 `EmptyHint`。发送失败时状态行显示 `ErrorMessage`，直到留言被修改。

关闭面板时草稿（所选表情和留言）保留，发送成功后清空。`SelectedMood` 和 `Message` 保存草稿并支持双向绑定；`SelectionChanged` 报告新选的表情，`Submitted` 报告一次发送，`StateChanged` 报告 `State` 的每次变化，发送失败时附带异常。`Open()`、`Close()`、`Submit()` 和 `ClearDraft()` 在代码中完成同样的事，`IsOpen` 也可以打开和关闭它。

### 内容准则

- **Label**：两三个词的邀请，例如 "Give feedback"、"Rate this page"。
- **Title**：一个关于按钮旁内容的简短问题，例如 "How do release notes read?"
- **Prompts**：每个表情一个追问细节的问题，表情越开心语气越温和，例如 "What went wrong?"、"What should we keep doing?"
- **SuccessLabel**：一句简短、有人情味、能放进一行的致谢。

## API 参考

<!-- api: FeedbackMorph -->

`FeedbackMood` 有 `Value`（`FeedbackMorphSubmission.Mood` 携带的值）、`Label`（说明文字和表情的无障碍名称）、`Prompt`（选中时留言中显示的问题）、`Face`（`Upset`、`Sad`、`Neutral`、`Happy` 或 `Delighted`）、可选的 `Glyph`，以及存放自定义数据的 `Tag`。

## 无障碍

- 按钮以 `Label` 命名。面板打开期间，按钮离开 Tab 顺序和无障碍树，面板以 `Title` 命名。
- 表情是一个单选组（以 `MoodsLabel` 命名），其中的单选按钮以各自的名称命名，只有一个 Tab 停靠点：已选的表情，或第一个表情。
- 留言以 `Placeholder` 命名，帮助文本是它当前显示的提示。发送按钮声明 Ctrl+Enter 为快捷键。
- 一个礼貌的实时区域播报 `SendingLabel`、`EmptyHint`、`ErrorMessage` 和 `SuccessLabel`。
- 面板打开时，焦点移到已选的表情或第一个表情。Escape 和关闭按钮把焦点还给按钮。发送后致谢胶囊获得焦点，胶囊收回时焦点回到按钮。
- 指针获得的焦点不画焦点环。键盘焦点在按钮、表情或发送按钮外 2 px 处画一圈 2 px 的焦点环。

### 键盘

| 按键 | 操作 |
|---|---|
| 在按钮上按 `Enter` / `Space` | 打开面板 |
| `→` `↓` / `←` `↑` | 选下一个或上一个表情（循环） |
| `Home` / `End` | 选第一个或最后一个表情 |
| `Tab` | 在表情、留言和发送按钮之间移动；从发送按钮再按 Tab 会收起面板 |
| 在留言中按 `Ctrl+Enter`（`⌘+Enter`） | 发送 |
| `Escape` | 把面板收回按钮，保留草稿 |

在面板外按下指针也会收起它。

## 动效

- 同一个形状在按钮、面板和致谢胶囊的尺寸之间弹动（0.36 秒的临界阻尼弹簧），圆角始终是 20 px，所以 40 px 高的胶囊是完整的圆端。阴影在 360 ms 内随之变大。
- 打开：按钮内容在 100 ms 内淡出并模糊；面板晚 60 ms 淡入，从 4 px 模糊变清晰，并从 0.92 放大。各行从 80 ms 起依次进入，间隔 50 ms（标题、表情、留言、操作），每行从上方 10 px（向上展开时从下方）落定并脱离轻微模糊；表情从 0.7 放大，从中间向两侧，间隔 35 ms。
- 选择表情：一块柔和的圆形高亮滑到它下面，所选表情放大到 1.14，其他缩小到 0.92，说明文字和提示短暂上移切换。剩余字数逐位滚动。
- 发送："Send" 向上离开，转圈和 "Sending" 升到它的位置。空发送让表情横向轻踢几个像素。
- 成功：面板在 80 ms 内淡出，形状同时缩成胶囊；对勾的圆环在 280 ms 内画出，接着画出勾，文字逐词升起，间隔 40 ms。收回时胶囊在 70 ms 内淡出，按钮晚 60 ms 出现。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 形状尺寸 | 弹簧 | 立即 | 立即 |
| 形状中的内容 | 淡变、模糊、缩放、逐行进入 | 短暂交叉淡变 | 立即 |
| 高亮、表情缩放 | 弹簧 | 立即 | 立即 |
| 按钮文字、说明、状态行 | 上移并淡变 | 淡变 | 立即 |
| 空发送轻踢 | 轻踢 | 无 | 无 |

## 响应式行为

- 面板宽 352 px；视口减去两侧各 16 px 更窄时取后者，所以能放进手机。靠近边缘时用 `Side` 和 `Align` 让它留在视口内。
- 表情是等宽列中 44 px 的点击目标。只有鼠标或手写笔悬停时才会点亮表情并显示名称。

## 相关组件

- `ActionMorph`：浮动按钮展开成快捷操作菜单及其表单。
- `Rating`：单独的评分。
- `Popover`：其他锚定内容。

| 组件 | 何时选择 |
|---|---|
| `FeedbackMorph` | 就地收集心情和可选留言，并致谢 |
| `ActionMorph` | 一个浮动按钮背后的多个快捷操作 |
| `Rating` | 单独的星级评分 |
