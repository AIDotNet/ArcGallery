---
title: Keycap keyboard
summary: 造型立体的机械键帽，打字或点击时会陷下并发光，打出的文字显示在键盘上方。
source-hash: 724a80befc42
---

<Demo name="Preview" />

## 何时使用

`KeycapKeyboard` 绘制一块机械键帽键盘，打出的文字显示在它上方。真实打字和点击会把对应的键帽按进底座，在它下面亮起一圈柔光，并把字符加到 `Value`。

- **试一试的时刻**：键盘优先的产品与开发者工具的落地页、空状态。
- **教人认键**：新手引导里展示一个快捷键用到哪些键。
- **小键盘**：用 `Rows` 搭出几个大键，打出一个词或执行一个宏。

## 何时不用

- **表单输入**：请用文本框；这块键盘是玩具，不是编辑器。
- **为无障碍提供的屏幕键盘**：请用系统自带的。
- **美式键盘以外的布局**：键帽按美式键盘摆放和标注。

## 用法

`KeycapKeyboard` 在 Special 包中；在 `UseArcControls()` 旁边用 `UseArcSpecial()` 注册。不做任何设置时，它显示一块紧凑的 60% 键盘，并在拥有焦点时监听按键。`Tracking="Window"` 会监听窗口里任何地方的打字，但文本框里的输入和带 Ctrl、Alt 或 Command 键的快捷键除外。`Value` 保存文字（双向），`ValueChanged` 报告每次变化。`MaxLength`（280）丢掉最早的字符，`Placeholder` 在文字为空时显示。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:KeycapKeyboard Tracking="Window" Placeholder="Press any key to try it" />
</UserControl>
```

键帽按物理位置对应真实按键，所以任何键盘布局都会点亮正确的键帽；打出的字符是系统给出的那个。Delete 删除最后一个字符，Return 另起一行，点击 Shift 会让下一个字符变成上档字符，数字和符号也一样。

### 字母

`Layout="Letters"` 显示三行字母，加上 Shift、Delete 和空格键。空格键是强调键，就像紧凑键盘上的 Return。

<Demo name="Letters" />

### 自定义行

`Rows` 用你自己的行替换布局。每一行的键用空格隔开：单个字符，或 `space`、`backspace`、`enter`、`shift`、`tab` 和 `caps`。自己显示 `Value` 时，用 `ShowsText="False"` 隐藏文字区。

<Demo name="MacroPad" />

### 快捷键

`Shortcut` 把一个组合键的键帽变成强调色，写法如 "Ctrl+Shift+P"、"Cmd+K" 或 "⇧⌘P"。在键盘里放入 `KeycapBinding`：恰好按住某个绑定的修饰键时（`ActiveModifiers`），它的键会印上标签；有绑定时，点击的修饰键会一直保持按住，直到下一个键。`Platform` 让修饰键印上 Windows 的词（ctrl、win、alt；默认是 ctrl、opt、cmd），`ShowsFunctionRow` 加上 Esc 和 F1 到 F12，`KeyInvoked` 报告每个按下的键以及它的修饰键和绑定。

<Demo name="Shortcut" />

### 最佳实践

**建议**

- 在没有其他输入框的页面上使用窗口监听，让键盘一出现就有反应。
- 自定义行要短：三到六个键的小键盘一眼就能看懂。
- 键盘有具体用途时用 `Label` 命名，例如 "Macro pad"。

**避免**

- 在有密码的界面上使用窗口监听：键会显示正在输入的内容。
- 把文字当作表单输入；`Value` 只用来展示。

## API 参考

<!-- api: KeycapKeyboard -->

<!-- api: KeycapRow -->

<!-- api: KeycapBinding -->

## 无障碍

- 键盘是一个以 `Label` 命名的分组。每个键帽都是一个按钮，以它的字符或功能命名（Space、Delete、Return、Shift），并且各自是一个 Tab 停靠点；Enter 或 Space 按下获得焦点的键帽。
- 文字区是一个只读文本框，位于 Tab 顺序的最前面；在按焦点监听时，聚焦它就能用真实按键打字。
- 带 Ctrl、Alt 或 Command 键的快捷键原样放行，窗口监听也不会把要输入到文本框里的内容打进来。键盘的帮助文本会读出点亮的快捷键和显示中的绑定。
- 键盘焦点会在获得焦点的键帽或文字区周围画一个方形焦点环；指针焦点不画。

### 键盘交互

| 按键 | 作用 |
|---|---|
| 任意可打印键 | 打出字符，并在按住期间让它的键帽保持按下。 |
| Backspace | 删除最后一个字符；按住会重复。 |
| Enter | 另起一行。 |
| Shift、Tab、Caps、修饰键 | 点亮它们的键帽。Tab 仍然移动焦点。 |
| Tab 到一个键帽，再按 Enter 或 Space | 像按钮一样按下这个键帽。 |

## 动效

- 按下时，键帽在 35 ms 内落下大部分行程，200 ms 内回弹。
- 按下的键帽下方，底板上亮起一圈柔光，在 700 ms 内淡去，所以快速打字会留下一串光痕；键帽边缘随之着色，字符变亮。
- 新字符在 150 ms 内从轻微的模糊中淡入。打字时光标保持不动，停下 600 ms 后再开始闪烁。
- 切换键盘时，底板按弹簧变形，新的各行淡入。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 按键 | 键帽陷下，柔光淡入淡出 | 没有行程；柔光与字符颜色立即变化 | 键帽直接落下 |
| 新字符 | 从模糊中淡入 | 直接出现 | 直接出现 |
| 光标 | 空闲时闪烁 | 常亮 | 常亮 |
| 切换键盘 | 底板变形，各行淡入 | 直接跳变 | 直接跳变 |

## 响应式

- 一个键单位决定所有尺寸（间距、行程、圆角和字符），随宽度增大，最大 54 px，所以最宽的一行总能放下；`KeySize` 可以把上限设得更低。
- 窄宽度下文字从 32 px 缩到 20 px，保持两行，并滚动到最新一行。
- 轻触在按下时就按下键帽，手指抬起前一直保持按下。触摸时不显示悬停效果。

## 相关组件

- `ShortcutRecorder` 记录人们按下的快捷键。
- `TypewriterTerminal` 自行打出一段预设的终端会话。

| 组件 | 适用场景 |
|---|---|
| `KeycapKeyboard` | 一块会回应打字和点击的趣味键盘 |
| `ShortcutRecorder` | 让人设置自己的快捷键 |
| `TypewriterTerminal` | 播放一段无需输入的终端会话 |
