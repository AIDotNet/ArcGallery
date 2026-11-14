---
title: Table of contents progress
summary: 长文章的目录：一条线随标题层级向内折入，随着阅读逐段填满，并标出当前章节；标题旁显示剩余的阅读分钟数，选择一个标题文章就会平滑滚动过去。
source-hash: 39920a64ff77
---

<Demo name="Preview" />

## 何时使用

`TocProgress` 放在长文章、指南或条款页面旁边，跟上读者的进度。一条细线沿着标题列表向下延伸，在层级变化处用一段短短的 S 形弯折进入该层级的缩进。已经读过的部分会一节一节地用强调色填满，一个小标记跟在填充的末端：它到达某个标题时，这个标题就会亮起，上面的标题显示为已读，下面的保持柔和。标题旁边显示剩余的阅读分钟数，读完时变为 Finished。选择一个标题，文章就会平滑滚动过去，这条线也会一路描到那里。

- **文档和指南**：人们在章节之间跳转，并想知道自己在哪里。
- **技术文章和长篇阅读**：知道还剩多少，读者更愿意读完。
- **在自己的滚动容器里的文章**：帮助面板、阅读视图或对话框。

## 何时不用

- **标题少于四个的短页面**：一个普通的链接列表就够了。
- **章节是彼此替代的选项**，而不是同一篇文档的各个部分：使用 `Tabs`。
- **读者必须按顺序完成的步骤**：使用 `Stepper`，它显示完成情况，而不是位置。
- **只需要一个整体进度提示**而不需要导航：显示一个普通的 `Progress` 进度条。

## 用法

`TocProgress` 位于 Special 包中：在 `UseArc()` 旁边用 `UseArcSpecial()` 注册它。把 `Target` 指向文章的 `ScrollViewer`，并用 `TocProgress.Heading` 标记其中的标题（子章节再加上 `TocProgress.Level` 2 或 3）。列表按文档顺序收集这些标记过的元素；层级每加深一级，线就向右移一条通道。

```xml
<Grid ColumnDefinitions="*,224" ColumnSpacing="16" Height="400">
    <ScrollViewer x:Name="Article">
        <StackPanel Spacing="12">
            <TextBlock Text="Install" arc:TocProgress.Heading="Install" FontSize="17" />
            <TextBlock Text="Add the package to your project." TextWrapping="Wrap" />
            <TextBlock Text="Options" arc:TocProgress.Heading="Options" arc:TocProgress.Level="2" FontSize="15" />
            <TextBlock Text="Every option has a sensible default." TextWrapping="Wrap" />
        </StackPanel>
    </ScrollViewer>
    <arc:TocProgress Grid.Column="1" Target="{Binding #Article}" ReadingOffset="28" ReadingTime="5" />
</Grid>
```

标题越过阅读线时算作已到达。把 `ReadingOffset` 设为固定页头的高度再加一点留白（16 到 32px）；选择标题时，文章会正好滚动到这条线。不设置时，阅读线位于视口向下四分之一处（`ActivationRatio`），选择的标题停在顶部下方 `ScrollMargin` 处。接近文章末尾时，阅读线会缓缓移到视口底部，所以滚动到底时，较短的最后几节也能被到达。

`ActiveIndex` 和 `ActiveChanged` 事件报告标记所在的章节（示例用它来更新底部的文字）；`Progress` 是文章已滚动的比例。`ReadingTime` 设置整篇文章的分钟数；不设置时，控件按每分钟 230 个词统计文章中的词数来估算。把 `ShowsTimeLeft` 设为 false 可以改为显示已读百分比（`ShowsPercent`）；当列表中的文字应与标题不同时，使用 `TocProgressEntry` 子元素。

### 紧凑

`DisplayMode` 为 Compact 时显示一个胶囊按钮，里面有进度环和当前章节的标题；点击后在弹出层中打开同一个列表。默认值 Auto 会在视口窄于 `ArcViewport.CompactWidth`（640px）时切换到它；把 `DisplayMode` 设为 List 可以保留列表，例如像第一个示例那样在手机上放在文章上方。

<Demo name="Compact" />

### 结构

1. **页头**：13px 中等字重的标题，和 12px 柔和色等宽数字显示的剩余分钟数；读完时显示一个对勾和 Finished。
2. **线**：1.5px 的轨道，每个层级一条通道，间隔 12px，两个层级相接处有 14px 高的 S 形弯折；已读部分用强调色，宽 2px，末端是一个 7px 的标记，周围留出 2px 的空隙。
3. **行**：13px 的标题，内边距 4px，第一层级缩进 22px，每深一级再加 12px。未读的标题柔和，已读的为次要文字色，当前标题为完整文字色；较长的标题会换行。

### 内容建议

- 大多数文章保持两个层级，最多三个；每一行都切换层级会让线变成锯齿状。
- 标题尽量简短；它们会换行，线会跟随每一行的高度。
- 已知阅读时间时传入 `ReadingTime`；估算会统计文章中的每一个词，包括代码。

## API 参考

<!-- api: TocProgress -->

<!-- api: TocProgressEntry -->

<!-- api: TocProgressLabels -->

<!-- api: TocProgressPosition -->

<!-- api: TocProgressActiveChangedEventArgs -->

## 无障碍

- 控件是一个导航区域，以它的标题 "On this page"（`Labels.Title`）命名。列表是一组链接：每一行以标题命名，当前行的帮助文本为 "current section"，已读的行为 "read"。已读、当前和未读的行在文字对比度上也有区别，从不只靠这条线来区分。
- 线、填充和标记只是绘制效果；剩余时间是普通文本，不会在每次变化时朗读。
- 每一行都在 Tab 顺序中。键盘焦点显示一层淡淡的底色和共用的 2px 焦点环；指针焦点两者都不显示。
- 紧凑按钮以当前章节和百分比命名，例如 "Contents, Results, 42% read"。

### 键盘

| 按键 | 操作 |
|---|---|
| Tab | 在各行之间移动，每一行都是一个链接 |
| Up / Down | 移到上一行 / 下一行 |
| Home / End | 移到第一行 / 最后一行 |
| Enter / Space | 把文章滚动到该章节 |
| Escape | 关闭紧凑弹出层并回到它的按钮 |

## 动效

- 阅读位置是章节序号加上读到该章节的程度，从该章节标题越过阅读线开始，到下一个标题越过阅读线为止。它通过一个临界阻尼弹簧跟随滚动（约 100ms 走完一半，约 0.4s 停稳），所以滚轮滚动一格时是平滑滑动，标记也不会越过当前章节。
- 每一节填满从它的标题到下一个标题之间的那段线，包括弯折处，所以填充总是在标题被到达的那一刻正好抵达它。
- 选择的标题用平滑弹簧滑到位，线会一路描过中间的每一节；任何滚轮或滚动条操作都会立即接管。如果文章在标题到达阅读线之前就结束了，它会保持为当前章节，直到读者继续滚动。
- 行的颜色在 240ms 内变化。剩余分钟数滚动到每个新值，伴随 8px 的位移和短暂的模糊，最后变为 Finished。
- 减少动态效果时，填充和标记直接跟随滚动，选择的标题直接跳到位，剩余时间以淡入淡出切换。关闭动效时一切立即变化。

## 响应式

- 列表占用给它的宽度，标题会换行；线会跟随每一行测量出的高度，所以换行的标题之间仍保留弯折。
- 列表、文章或其滚动视图改变大小时，行、线和标题位置都会重新测量。
- 行占满列表的整个宽度，便于触摸。在窄屏上把列表放在文章上方，或者让 `DisplayMode` 为 Auto 时收成紧凑按钮。

## 相关组件

- `Progress`：普通的进度条。
- `Stepper`：用户按顺序完成的步骤进度。
- `ScrollArea`：带 Arc 滚动条的滚动容器，用来放文章。
- `Tabs`：在视图之间切换，而不是在同一页内移动。
