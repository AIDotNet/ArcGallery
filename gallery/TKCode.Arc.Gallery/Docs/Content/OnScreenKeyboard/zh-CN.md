---
title: 屏幕键盘
summary: 为没有实体键盘的自助机和面板提供的触摸键盘：全键盘、数字键盘、PIN 键盘或你自己的布局，向有焦点的字段输入文字，且不抢走焦点。
source-hash: 24c01c172220
---

<Demo name="Preview" />

## 何时使用

`OnScreenKeyboard` 在屏幕上绘制一个键盘，并向同一窗口中有焦点的文本输入框输入文字。适用于人们只有触摸屏、且无法依赖系统键盘的场景：系统键盘可能不会自动弹出（例如 Windows 自助机），或者根本没有系统键盘（例如嵌入式 Linux 面板）。

- **自助服务机**：值机、点餐或取票机，访客在上面输入姓名、邮箱或预订码。
- **触摸面板与工业 HMI**：操作员在没有连接键盘的工控机上输入批号、数量或设定值。
- **PIN 与验证码输入**：门禁面板或收银台要求输入 PIN。PIN 键盘可以把数字送到你自己的显示内容中，因此数字从不以文本形式出现。
- **数量与价格字段**：数字键盘带有 `Culture` 的小数分隔符、退格键和光标键，用拇指就能快速填好数量字段。
- **特殊小键盘**：十六进制键盘、单位键盘，或其他语言的键盘，都可以基于同一个布局模型构建。

## 何时不用

- **手机和平板**：系统键盘在那里更好（有预测、语音输入和无障碍设置）。请改为在文本框上用 `TextInputOptions` 请求合适类型的键盘。
- **带实体键盘的桌面**：屏幕上的第二个键盘只会占用空间。
- **从一个范围中选取数值**：使用 `Slider` 或 `NumberField` 的步进按钮，它们完全不需要输入。
- **从已知列表中选择**：使用 `Select`、`Combobox` 或 `SegmentedControl`；点选一个选项比输入它更快。

## 用法

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <DockPanel>
        <arc:OnScreenKeyboard DockPanel.Dock="Bottom" MaxWidth="720" />
        <StackPanel Spacing="12" Margin="24">
            <arc:Input Label="Name" />
            <arc:Input Label="Email" />
        </StackPanel>
    </DockPanel>
</UserControl>
```

先轻点一个字段，然后按键。键盘会输入到所在窗口中有焦点的文本输入框：Arc 的 `Input`、`Textarea`、`PasswordField` 或 `NumberField`、Avalonia 的 `TextBox`，或者你自己处理文本输入的控件。按键从不移动焦点，因此光标留在原处，Tab 键或轻点另一个字段照常移动焦点。

每个按键如何到达字段：

- **字母、数字、符号和空格键**以文本输入的形式发送，与系统键盘或输入法触发的事件相同。文本框会在光标处插入文字、替换选中内容，并保留 `MaxLength`、`IsReadOnly` 和撤销历史。
- **Backspace、Enter、Tab 和方向键**以按键的形式发送。文本框自行处理它们：Enter 在多行文本框中开始新行，在单行文本框中则冒泡到默认按钮或你自己的 `KeyDown` 处理程序。Tab 把焦点移到下一个字段。

按键在按下时生效，而不是松开时，因此双指快速输入的顺序是正确的。

### 结构

1. **托盘**：按键背后的柔和面板，带 1 px 的细边框、20 px 圆角和 8 px 内边距。
2. **字符键**：最浅的按键，带 1 px 边缘和 2 px 底部厚边、12 px 圆角和 22 px 标签。
3. **功能键**：Shift、Backspace、页面键和方向键，比字符键深一级。标签超过一个字符（"123"、"ABC"）时使用小号中等字重。
4. **Enter**：主要按键，与主按钮一样用前景色填充。
5. **行**：每一行都居中，因此键单位数少于最宽行的行两侧会缩进（QWERTY 的基准行缩进半个键）。按键高 46 px，键与键之间 6 px，行与行之间 8 px。

### 数字与 PIN 键盘

`Preset` 选择内置布局。`Numeric` 是数字键盘，小数分隔符来自 `Culture`（或当前文化），包含退格、左右光标键和 Enter；`SignedNumeric` 额外增加一个负号键；`Digits` 是 PIN 键盘，只有数字、退格和 Enter。键盘会把按键拉伸到你给它的尺寸。

<Demo name="NumberPad" />

```xml
<arc:OnScreenKeyboard Preset="Numeric" Culture="de-DE" Width="280" Height="260" />
```

让键盘与它所填充的字段使用相同的文化：`NumberField` 使用自己的 `Culture` 解析数字（除非你设置了它，否则为 en-US），因此德语的逗号在那里无法被理解。

### 自行处理按键

`ItemInvoked` 会在键盘执行操作之前，为每次按键以及按住按键时的每次重复触发。它的 `Item` 是按键，`Text` 是即将输入的文字（已应用 Shift），`Target` 是将接收它的元素，`IsRepeat` 表示按键是否被按住。将 `Handled` 设为 true 后，键盘不再做任何其他操作：它不输入任何文字，也不改变 Shift 和页面的状态。下面的 PIN 键盘会填入四个圆点，从不以文本形式显示数字。

<Demo name="PinPad" />

```csharp
keypad.ItemInvoked += (_, e) =>
{
    e.Handled = true;
    switch (e.Item.Kind)
    {
        case OnScreenKeyboardKeyKind.Text:
            pin.Append(e.Text);
            break;
        case OnScreenKeyboardKeyKind.Backspace when pin.Length > 0:
            pin.Length--;
            break;
        case OnScreenKeyboardKeyKind.Enter:
            await UnlockAsync(pin.ToString());
            break;
    }
};
```

只想添加内容时，可以只处理少数几个按键，其余按键照常放行，例如一个清空字段的 `Custom` 键 "Clear"。`Invoke(key)` 从代码中按下一个按键，效果与轻点相同。

### 在字段下方打开键盘

与停靠的键盘不同，可以在字段上设置 `OnScreenKeyboard.Popup`。字段获得焦点时，该类型的键盘会在其下方打开（下方没有空间时则在其上方），并且只向它输入。焦点离开字段、在单行字段中按下 Enter，或按下 `Hide` 键时它会关闭；再次轻点字段会重新打开它。`OnScreenKeyboard.PopupLayout` 则可以改为打开你自己的布局。

<Demo name="Popup" />

```xml
<arc:NumberField Label="Quantity" arc:OnScreenKeyboard.Popup="Numeric" />
<arc:PasswordField Label="Door code" arc:OnScreenKeyboard.Popup="Digits" />
```

### 自定义布局

布局就是普通数据：`OnScreenKeyboardLayout` 包含页面，页面包含行，行包含按键。每个 `OnScreenKeyboardKey` 说明它做什么（`Kind`）、输入什么（`Text`，以及按下 Shift 时的 `ShiftText`）、显示什么（`Label`，或 24 × 24 的 Lucide `Glyph`），以及它有多宽（`Width`，以标准键为单位）。按键可以在显示期间绑定和修改：禁用的按键变暗且不起作用，隐藏的按键不占空间，它所在的行会收拢。

<Demo name="CustomLayout" />

| 类型 | 作用 |
|---|---|
| `Text` | 输入 `Text`；按下 Shift 时输入 `ShiftText`（否则输入大写的 `Text`） |
| `Space` | 输入一个空格 |
| `Backspace` | 删除光标前的字符或选中内容；按住时重复 |
| `Enter` | 在多行文本框中开始新行；否则作为按键冒泡 |
| `Tab` | 把焦点移到下一个字段；按下 Shift 时则向前移动 |
| `Shift` | 作用于下一个按键；快速点按两次锁定大写 |
| `Left`、`Right`、`Up`、`Down` | 移动光标（按下 Shift 时扩展选择）；按住时重复 |
| `Page` | 切换到 `TargetPage` 指定的页面 |
| `Hide` | 引发 `HideRequested`；由 `Popup` 打开的键盘随之关闭 |
| `Key` | 以按键的形式发送 `KeyCode`（Escape、Delete、Home...） |
| `Custom` | 本身不做任何事：请处理 `ItemInvoked` |
| `Spacer` | 一个宽度为 `Width` 单位的空白间隔 |

`OnScreenKeyboardLayout.Create(preset, culture)` 以及 `CreateQwerty`、`CreateNumeric` 和 `CreateDigits` 会返回内置布局的新对象，你可以在使用前修改它们。`OnScreenKeyboardRow.FromCharacters("qwertyuiop")` 从一个字符串生成一行文本键。

### 其他语言

内置的 QWERTY 是英文的。其他语言同样是普通的布局：从内置布局开始，替换其中的字母行，或者自己编写每一页。键盘会按 `Culture` 的规则把字母转为大写，因此德语、土耳其语或希腊语的大写字母都能正确显示。

<Demo name="Languages" />

```csharp
var layout = OnScreenKeyboardLayout.CreateQwerty();
var letters = layout.Pages[0].Rows;
letters[0] = OnScreenKeyboardRow.FromCharacters("qwertzuiopü");
letters[1] = OnScreenKeyboardRow.FromCharacters("asdfghjklöä");
keyboard.Culture = CultureInfo.GetCultureInfo("de-DE");
keyboard.Layout = layout;
```

按键输入的是文本，因此文本框能够显示的任何文字都可以输入。需要从候选词组合文字的语言，例如中文拼音或日文假名转汉字，需要输入法支持，不在键盘的覆盖范围之内。

### Shift、大写锁定与页面

轻点一次 Shift 会把下一个按键大写（`ShiftState` 为 `On`）；再点一次则关闭。快速连续点按两次会锁定大写（`Locked`），直到再次点按 Shift 为止。与实体键盘的大写锁定一样，大写锁定只改变字母；单次 Shift 还会让数字和符号使用它们的 `ShiftText`。Shift 键开启时填充其图标，锁定时在图标下方加一条横线，因此状态不只依赖颜色表示。`ShiftState` 支持双向绑定。

QWERTY 布局有三页：字母、数字与符号（"123"），以及更多符号（"#+="）。`CurrentPage` 指明当前显示的页面（`OnScreenKeyboardLayout.LettersPage`、`SymbolsPage`、`MoreSymbolsPage`），可以设置或绑定。切换页面会关闭单次 Shift，但保留大写锁定。

### 使用 Target 代替焦点

设置 `Target` 可以把每个按键都发送到一个元素，无论它是否有焦点。当键盘专属于某一个字段，或者页面上其他地方的按压会抢走焦点时，就使用它。没有目标且没有任何元素获得焦点时，按键只会引发 `ItemInvoked`。

```xml
<arc:Input x:Name="Code" Label="Booking code" />
<arc:OnScreenKeyboard Target="{Binding #Code}" />
```

### 按键重复

按住 Backspace 或方向键时，它会在 `RepeatDelay`（500 ms）之后，每隔 `RepeatInterval`（50 ms）重复一次，直到松开为止。其他按键不重复；如需改变，可在按键上设置 `IsRepeatable`。计时使用 `TimeProvider`，因此测试可以用自己的时钟驱动重复。

### 尺寸

没有指定尺寸时，键盘的宽度与其最宽的行相同，每个键为 52 px（QWERTY 约 530 px，包括托盘），高度与行数相同，每行 46 px。给出更大的空间时，按键会随之放大：宽度按键单位平分，高度按行平分。在宽屏上请设置 `MaxWidth`（QWERTY 约 720 到 900 px），让按键保持舒适的比例。停靠在窗口底部时，设置 `CornerRadius="0"` 和 `BorderThickness="0,1,0,0"`，即可得到平整的托盘。

### 内容规范

- **标签**：按键标签要简短。一个字符保持大号；较长的标签（"Done"、"Clear"）使用小号，按键太窄时以省略号截断。
- **名称**：显示图标或符号的按键，应设置 `AutomationName`（"Minus"、"Decimal separator"、"Hide keyboard"）。
- **字段标签**：字段仍需要自己的标签和提示；键盘不会说明该输入什么。

| 不要这样写 | 改为 |
|---|---|
| 标着 "OK" 却只会关闭的按键 | `Kind="Hide"`，名称为 "Hide keyboard" |
| 按键上拼写出 "Enter" | Enter 图标，或 "Done"、"Search" 之类的动词 |
| 没有名称的符号键 | `AutomationName="Degree sign"` |

### 最佳实践

**推荐**

- 为数字和 PIN 键盘设置适合拇指的尺寸，宽约 260 到 300 px。
- 让 `Culture` 与字段的文化一致，以正确输入小数。
- 对于不能以文本形式出现的 PIN 和验证码，处理 `ItemInvoked`。
- 在宽屏上横向拉伸的键盘，请设置 `MaxWidth`。

**避免**

- 在已有系统键盘的设备上显示屏幕键盘。
- 在自定义按键模板中或键盘周围放置可获得焦点的控件，以免按压时误使其获得焦点。
- 把屏幕键盘用于列表或滑块能更快完成的选择。

### 常见问题

**为什么键盘输入不了任何内容？** 没有元素获得焦点，也没有设置 `Target`。请先轻点一个字段、设置 `Target`，或在没有焦点时处理 `ItemInvoked`（上方的预览从第一个字段开始）。

**按键会抢走我文本框的焦点吗？** 不会。键盘在 Avalonia 的焦点处理看到之前，就已接管其内部的每次按压，而且按键本身不可获得焦点。

**Backspace 和方向键在普通的 `TextBox` 中能用吗？** 能，但需要使用为 `TextBox` 提供模板的主题（Fluent 或 Simple）。Arc 自己的文本输入控件在任何应用中都能工作；没有模板的普通 `TextBox` 仍能接收输入的文字。

**能否在 `KeyDown` 中监听字母键？** 字母以文本输入的形式发送，而不是按键，因为大多数字符没有键码。请改为处理 `ItemInvoked`。

**能否让按键在松开时才生效？** 不能。按键在按下时生效，这样快速输入能保持顺序，并且按住的键可以重复。

## API 参考

<!-- api: OnScreenKeyboard -->

<!-- api: OnScreenKeyboardKey -->

## 无障碍

- 键盘是一个名为 "Keyboard" 的组；请设置 `AutomationProperties.Name`，以按用途为它命名（"PIN pad"）。
- 每个按键都是按钮，屏幕阅读器会按其 `AutomationName`、标签或类型来朗读（"Shift"、"Backspace"、"Enter"、"Space"、"Left"）；Invoke 会按下它。Shift 同时是切换按钮，会报告它是否开启。
- Shift 的变化会以礼貌方式播报（"Shift on"、"Caps lock on"、"Shift off"，来自 `ShiftOnText`、`CapsLockOnText` 和 `ShiftOffText`），切换到的页面也会播报（页面的 `Label`，例如 "Numbers and symbols"）。
- Shift 和大写锁定同时改变图标和填充；禁用的按键会变暗，并以弱化的文字颜色绘制。
- 按键在自然尺寸下高 46 px，并随所得空间增大。

### 键盘

屏幕键盘本身就是键盘：它的按键不获得焦点，它也没有自己的键盘交互。实体键盘在有焦点的字段中照常工作。

| 屏幕上的按键 | 发送给字段的内容 |
|---|---|
| 字母、数字、符号、空格 | 在光标处输入文字 |
| `Backspace` | `Back` 按键 |
| `Enter` | `Enter` 按键 |
| `Tab` / 开启 Shift 时的 `Tab` | `Tab` / `Shift+Tab` 按键 |
| 方向键 / 开启 Shift 时的方向键 | 方向键 / 带 `Shift` 的方向键（扩展选择） |

## 动效

- 按下的按键在快速弹簧上下沉至 95%，并采用按下时的填充；松开时弹回。
- 悬停（仅鼠标）和按下时的填充通过快速淡变切换。
- 标签、Shift 和页面立即切换。

| 变化 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 按键按下 | 快速弹簧缩放至 95%，填充淡变（120 ms） | 填充淡变，不缩放 | 立即 |
| 悬停（鼠标） | 120 ms 填充淡变 | 120 ms 填充淡变 | 立即 |
| Shift、大写锁定、页面 | 立即 | 立即 | 立即 |
| `Popup` 键盘打开 | 弹出层的上升与淡入 | 淡入 | 立即 |

## 响应式

- 按键平分键盘所得的宽度；在手机宽度下，QWERTY 的顶行仍保留全部十个键，按键变窄，圆角随之缩小，以保持按键的形状。
- 除非给出了高度（之后各行平分该高度），键盘的高度与其行数相同。
- 没有任何功能依赖悬停：每个按键都能通过轻点使用，悬停只为鼠标增加填充。
- `Popup` 键盘在其字段下方打开，下方没有空间时则在其上方打开。

## 相关组件

- `NumberField` 用于带步进按钮的数字；`Input`、`Textarea` 和 `PasswordField` 是它输入的字段。
- `OtpInput`（Blocks）用于一次性验证码，可由 `Digits` 键盘输入。
- `ShortcutRecorder` 用于从实体键盘录入组合键。

| 组件 | 何时选择 |
|---|---|
| `NumberField` | 有界数字集合，需要步进、滚轮或拖动调整 |
| `Slider` | 在一个范围内选取近似值 |
| `Select` | 从已知列表中选择一个选项 |
| `OtpInput` | 分格输入的一次性验证码 |
| `ShortcutRecorder` | 从实体键盘录入组合键 |
