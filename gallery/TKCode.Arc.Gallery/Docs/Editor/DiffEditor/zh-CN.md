---
title: 差异编辑器
summary: "像 Monaco 的差异编辑器那样比较两段文本：并排或内联显示，高亮改动的行和字符，标出移动的代码块，折叠未改动的区域，并可用箭头撤回某处改动。"
source-hash: 1801441c330b
---

<Demo name="Preview" />

左侧是原始文本，右侧是修改后的文本。改动的行带底色，行内改动的字符颜色更深，滚动时两侧的行保持对齐。编辑右侧，差异会随之更新。

## 何时使用

- 在保存、提交或应用之前审阅一处改动：生成的编辑、合并结果、设置迁移。
- 展示一个文件、一条查询或一份配置在两个版本之间的变化。
- 让用户撤回改动中的一部分：每处改动都有一个箭头，点击即可撤回。

## 何时不用

- 只编辑一段文本、没有可比较的版本：使用 `CodeEditor`。
- 一句话或一个值的简短前后对比：把两者都显示出来，或在普通文本里标出改动的词。
- 同时比较许多文件：差异编辑器一次只比较一对文本。

## 用法

`DiffEditor` 和 `CodeEditor` 同在 `TKCode.Arc.Editor.Controls` 包中，`UseArcCodeEditor()` 会同时注册两者：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

给它两段文本和一种用于高亮的语言。`OriginalText` 只读；`ModifiedText` 双向绑定，用户输入的内容会回到视图模型。

```xml
<arc:DiffEditor Height="400"
                LanguageId="csharp"
                OriginalText="{Binding SavedSource}"
                ModifiedText="{Binding Source}" />
```

差异算法与 Monaco 相同：先比较行，再比较改动行内的字符，并识别和连接移动过的代码块。计算在 UI 线程之外、基于两段文本的快照进行，每次编辑后稍等片刻会重新计算；特别大的一对文本超过时间限制后会停止，显示已经找到的行。每次计算完成都会触发 `DiffComputed`，带有改动数、增加和删除的行数以及移动的代码块数。

```csharp
Diff.DiffComputed += (_, e) => Summary.Text = e.IsIdentical ? "No changes" : $"{e.ChangeCount} changes";
```

### 选项

<Demo name="Options" />

- `RenderSideBySide` 并排显示两段文本（默认开启）；关闭后由一个编辑器显示修改后的文本，删除的行插在它的行之间。
- `HideUnchangedRegions` 把较长的未改动行折叠成一条横栏，横栏上写着隐藏了多少行，点击即可展开（默认关闭）。
- `IgnoreTrimWhitespace` 忽略行首和行尾空白的变化，与 Monaco 的默认行为一致（默认开启）。
- `IsReadOnly` 让修改后的文本也不能更改；原始文本始终不能更改。

### 键盘

| 按键 | 操作 |
|---|---|
| F7 | 转到下一处改动 |
| Shift+F7 | 转到上一处改动 |

在每一侧内部，`CodeEditor` 的按键都适用：查找、多光标、折叠等。

## API 参考

<!-- api: DiffEditor -->

## 无障碍

- 用 `AutomationProperties.Name` 为差异编辑器命名，说明比较的是什么，例如“Program.cs 的改动”。
- 改动既用颜色表示，也用边栏中的 + 和 − 号表示，不只依赖颜色。
- 两侧都是代码编辑器，会把文本读给屏幕阅读器；目前还没有供屏幕阅读器使用的改动列表。

## 动效

- 没有自动播放的动效。转到某处改动时，两侧会一起滚动到那里。

## 响应式行为

- 差异编辑器占满给定的宽度，并在两侧之间平分。在窄屏上请关闭 `RenderSideBySide`：内联视图只需要一列。
- 请给它设定高度：它会滚动内容，不会随内容变高。

## 相关

- `CodeEditor`：编辑一段文本。
- `CodeBlock`：只显示代码、不编辑。
