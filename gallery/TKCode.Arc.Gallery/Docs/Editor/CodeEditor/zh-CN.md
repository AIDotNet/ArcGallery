---
title: 代码编辑器
summary: "像在 Monaco 中一样编辑源代码：89 种语言的语法高亮、查找替换、代码折叠、多光标、minimap、来自语言服务器的补全与悬停提示、Monaco 的快捷键、撤销与重做。"
source-hash: 3879e8cd8bb3
---

<Demo name="Preview" />

点进编辑器即可输入。方向键、Home、End 和 Page Up / Page Down 移动光标，同时按 Shift 则选择；Tab 和 Shift+Tab 增加和减少缩进；Ctrl+Z（macOS 上为 ⌘Z）撤销。

## 何时使用

- 用户在应用里编写或修改的源代码、配置文件、查询和脚本。
- 制表符、缩进和行号都有意义、用等宽字体显示的文本。
- 很长的文档：文本存放在 piece tree 里，几 MB 的文件也能很快打开和编辑，并且只绘制可见的行。

## 何时不用

- 只展示的代码：用 `CodeBlock`，它会高亮代码并带复制按钮。
- 表单里的普通文本：用 `Input` 或 `Textarea`。
- 带标题、列表和链接的格式化文本：用 `RichTextEditor`。

## 用法

`CodeEditor` 在 `TKCode.Arc.Editor.Controls` 包里，这个包会带上编辑器引擎 `TKCode.Arc.Editor`。在 `UseArc(...)` 中注册它：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

`Text` 保存文档，支持双向绑定。设置它会替换整个文档并清空撤销历史；用户的输入会更新它，两种情况都会触发 `TextChanged`。

```xml
<arc:CodeEditor Height="320"
                Text="{Binding Source}"
                TabSize="2" />
```

```csharp
Editor.TextChanged += (_, _) => Save(Editor.Text);
```

引擎移植自 Monaco 编辑器（Visual Studio Code 的编辑器），所以输入的行为与那里相同：括号和引号在语言允许的地方自动闭合，回车保留当前行的缩进，输入一个自动插入的右括号时会直接跳过它。`LanguageId` 指定使用哪种语言的配置；新建的编辑器是 `plaintext`。

支持输入法：正在组合的文字带下划线显示在光标处，确认后才写入文档。

### 语言

`LanguageId` 可以是 Monaco 编辑器自带的 89 种语言中的任意一种，用 id 或名称都可以：`csharp` 或 `C#`、`typescript`、`javascript`、`python`、`json`、`html`、`css`、`sql`、`yaml`、`markdown`、`rust`、`go`、`java`、`cpp` 等。它决定语法高亮，以及该语言的括号、注释、自动闭合对和缩进规则。颜色采用 Monaco 的浅色或深色主题，跟随应用的明暗外观，其余文字使用编辑器自身的文字颜色。屏幕上的行会立即着色，长文件的其余部分在应用空闲时完成。括号按嵌套深度着色，和 Visual Studio Code 一样；字符串和注释里的括号不算，没有对应左括号的右括号显示为红色。

```xml
<arc:CodeEditor LanguageId="typescript" Text="{Binding Source}" />
```

括号按嵌套深度着色，光标处括号的配对括号会加上边框。

### 语言服务器

补全、悬停提示、问题、转到定义等语言功能来自语言服务器。`TKCode.Arc.Editor.Lsp` 包提供了一个 Language Server Protocol 客户端：启动服务器，把它的输出流和输入流交给客户端，再用服务器认识该文件的 URI 连接编辑器。

```csharp
var process = Process.Start(new ProcessStartInfo("typescript-language-server", "--stdio")
{
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
});
var client = new LanguageClient(new LanguageClientOptions { RootUri = new Uri("file:///work/") });
await client.StartAsync(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
var attachment = Editor.AttachLanguageClient(client, new Uri("file:///work/main.ts"));
```

之后，编辑器会在输入时（或按 Ctrl+Space 时）显示建议，悬停时显示文档和问题，在错误和警告下画波浪线，在调用中显示参数提示，为代码操作显示灯泡，显示 inlay hint 和 code lens，并使用服务器提供的语义颜色——每项都取决于服务器是否提供。没有服务器时，建议来自文档中的单词。释放 `attachment` 即可断开。

### 代码片段

`InsertSnippet` 在每个光标处插入一段 Visual Studio Code 语法的代码片段：`$1` 和 `${2:default}` 是制表位，`${1|one,two|}` 是选项，`$TM_SELECTED_TEXT` 或 `$CLIPBOARD` 是变量。Tab 和 Shift+Tab 在制表位之间移动，Escape 退出。

```csharp
Editor.InsertSnippet("for (var ${1:i} = 0; $1 < ${2:count}; $1++)\n{\n\t$0\n}");
```

### 选项

<Demo name="Options" />

- `TabSize` 是一个制表符的宽度（以空格计，默认 4），也是一级缩进的大小；`InsertSpaces` 让 Tab 插入空格而不是制表符（默认开启）。
- `WordWrap` 让长行在编辑器宽度处折行；折出来的部分没有行号。
- `ShowLineNumbers` 在左侧显示行号（默认开启）。
- `ShowIndentGuides` 在每一级缩进处画一条细线，光标所在代码块的那一条更深，对应 Monaco 的 `guides.indentation` 选项（默认开启）。
- `SmoothScrolling` 让滚轮滚动和滚到光标处带动画，对应 Monaco 的 `smoothScrolling` 选项（默认关闭）。
- `ShowMinimap` 在右侧显示文档的缩略图；单击它或拖动其滑块即可滚动（默认开启）。旁边的概览标尺标出整个文档中的光标、问题和搜索结果。
- `Folding` 允许用边栏中的折叠箭头和折叠命令折叠代码区域（默认开启）。
- `StickyScroll` 在滚动经过类、函数和代码块时，把它们的起始行固定在顶部，最多 `StickyScrollMaxLineCount` 行（默认开启）。
- `ColorDecorators` 在 CSS 颜色前放一个色块；单击它会打开颜色选择器，并把选中的颜色写回（默认开启）。
- `FormatOnType` 和 `FormatOnPaste` 在输入或粘贴时格式化代码，前提是语言有格式化器（默认关闭）。`LinkedEditing` 让关联的范围一起改变，例如 HTML 标签的开始和结束名称（默认关闭）。
- `TabFocusMode` 让 Tab 和 Shift+Tab 把焦点移出编辑器，而不是缩进；Ctrl+M 切换它（默认关闭）。
- `IsReadOnly` 让文本不能修改；光标仍可移动，文本仍可选择和复制。

### 键盘

编辑器使用 Monaco 的快捷键；在 macOS 上 Ctrl 对应 ⌘。

| 按键 | 作用 |
|---|---|
| 方向键、Home、End、Page Up、Page Down | 移动光标 |
| 以上任一键加 Shift | 选择 |
| Ctrl+Home、Ctrl+End | 跳到文档开头或末尾 |
| Ctrl+A | 全选 |
| Ctrl+Shift+Alt 加方向键 | 列选择：每行一个光标 |
| Escape | 取消选择，再按一次回到单个光标 |
| Tab、Shift+Tab | 增加缩进、减少缩进 |
| Ctrl+Z，Ctrl+Y 或 Ctrl+Shift+Z | 撤销、重做 |
| Ctrl+C、Ctrl+X、Ctrl+V | 复制、剪切、粘贴；没有选区时，复制和剪切作用于整行 |
| Ctrl+F、Ctrl+H | 查找、查找并替换；F3 和 Shift+F3 转到下一个和上一个匹配 |
| Ctrl+D、Ctrl+Shift+L | 添加选区的下一个出现处、选择所有出现处 |
| Ctrl+Alt+Up、Ctrl+Alt+Down | 在上方或下方添加光标 |
| Alt+Up、Alt+Down、Shift+Alt+Up、Shift+Alt+Down | 上移或下移行，向上或向下复制行 |
| Ctrl+Shift+K、Ctrl+Enter、Ctrl+Shift+Enter | 删除行，在下方或上方插入一行 |
| Ctrl+/、Shift+Alt+A | 切换行注释、块注释 |
| Ctrl+Shift+\\ | 转到匹配的括号 |
| Ctrl+Shift+[、Ctrl+Shift+] | 折叠、展开；Ctrl+K Ctrl+0 全部折叠，Ctrl+K Ctrl+J 全部展开 |
| Shift+Alt+Right、Shift+Alt+Left | 扩大、缩小选区 |
| Ctrl+Space、Ctrl+Shift+Space | 显示建议、显示参数提示 |
| F12、Shift+F12、Alt+F12 | 转到定义、转到引用、速览定义 |
| F2、Ctrl+. | 重命名符号、显示代码操作 |
| F8、Shift+F8 | 转到下一个或上一个问题 |
| Shift+Alt+F | 格式化文档 |
| Ctrl+M | 切换 Tab 是否移动焦点 |

用鼠标时，单击放置光标，拖动或 Shift+单击选择，双击选择一个词，三击选择一行；双击或三击后继续拖动，按词或按行扩展选区。Alt+单击添加光标（再次 Alt+单击可移除），Alt+Shift+拖动或按住中键拖动进行列选择，单击或拖动行号选择整行。拖出边缘时会自动滚动。拖动选中的文本可以移动它（按住 Ctrl，macOS 上按住 Option，则为复制）；Ctrl+单击链接可打开它，`LinkOpening` 事件让应用自行打开链接。

## API 参考

<!-- api: CodeEditor -->

## 无障碍

- 用 `AutomationProperties.Name` 给编辑器命名（如示例所示），或在旁边放一个标签。
- Tab 是编辑器自己的按键：它用来缩进，不会把焦点移出去。Ctrl+M（macOS 上为 Ctrl+Shift+M）会打开 `TabFocusMode`，此时 Tab 移动焦点；请告诉键盘用户这一点，或在编辑器只是表单中的一个字段时设置 `TabFocusMode`。
- 屏幕阅读器看到的是一个编辑控件，其值就是文档。光标移动时，编辑器会读出当前行、字符或选区；Ctrl+Alt+Shift+G 读出光标所在的行和列。

## 动效

- 编辑器有焦点时光标闪烁；每次按键后光标会先停止闪烁一会儿，保证输入时能看见它。查找框打开时会滑入；除此之外没有其他动画。

## 响应式行为

- 编辑器占满给它的宽度，最小高度为 80。请自己设定高度：内容在编辑器内滚动，编辑器不会随内容变高。
- 开启 `WordWrap` 时，编辑器宽度变化后会重新折行。
- 在触屏上，轻点放置光标，双击或长按选择一个词并显示可拖动的手柄，手指滑动可带惯性地滚动。
- 编辑器较窄时，关闭 `ShowMinimap` 把空间留给文本。

## 相关

- `CodeBlock`：带高亮、只展示不编辑的代码。
- `Textarea`：表单里的多行纯文本。
- `RichTextEditor`：格式化文本。
- `DiffEditor`：比较一段文本的两个版本。
