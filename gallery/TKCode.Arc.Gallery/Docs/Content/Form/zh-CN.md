---
title: 表单
summary: 轻量的表单范围：带有必填标记和帮助文本的带标签项，在用户操作时显示视图模型的校验错误，提交时滚动到第一个无效字段。
source-hash: 24c45d61692b
---

<Demo name="Preview" />

## 何时使用

- **注册、结账、设置和对话框**：几个字段一起提交，用户应当在发送之前看到需要修改的地方。
- **规则已经位于视图模型中**（`INotifyDataErrorInfo`，例如 CommunityToolkit.Mvvm 的 `ObservableValidator` 配合 DataAnnotations）：表单会在合适的时机、以合适的位置显示这些错误。
- **所有字段使用同一种布局**：相同的标签、必填标记、帮助文本和错误行，标签位于上方，或在左侧对齐成一列。

## 何时不用

- **单个字段**：Arc 文本框已经带有标签、描述和错误行。
- **在 XAML 中编写的规则**：表单没有规则引擎。必填、邮箱、长度、范围和正则表达式规则应放在视图模型中。
- **逐个属性地编辑一个对象**：使用 `PropertyGrid`。
- **分步骤的表单**：使用 `MultiStepForm` 区块。

## 用法

`Form` 派生自 Avalonia 的 `ContentControl`，自身不做任何布局：在其中放入一个面板即可。其中的每个 `FormItem` 进入逻辑树时都会加入最近的表单，被移除时离开，因此无需手动注册。`FormItem` 派生自 `Field`：`Label`、`Description` 和 `ErrorMessage` 的用法与任何 Arc 字段相同。

```xml
<arc:Form ValidateCommand="{Binding ValidateCommand}"
          SubmitCommand="{Binding CreateAccountCommand}"
          ResetCommand="{Binding ResetCommand}">
    <StackPanel Spacing="20">
        <arc:FormItem Label="Email" IsRequired="True">
            <arc:Input Text="{Binding Email}" Placeholder="you@example.com" />
        </arc:FormItem>
        <arc:FormItem Label="Password" IsRequired="True" Description="At least 8 characters.">
            <arc:PasswordField Text="{Binding Password}" />
        </arc:FormItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <arc:Button arc:Form.Action="Submit" IsDefault="True" Content="Create account" />
            <arc:Button arc:Form.Action="Reset" Variant="Secondary" Content="Reset" />
        </StackPanel>
    </StackPanel>
</arc:Form>
```

### 视图模型中的规则

由视图模型决定什么是有效的；由表单决定错误何时、何处显示。错误通过 Avalonia 的数据校验（`DataValidationErrors`）传递到各个项，因此表单从不读取视图模型，对 AOT 和裁剪保持安全。本页的演示通过手写代码实现 `INotifyDataErrorInfo`；使用 CommunityToolkit.Mvvm 时，同一个视图模型如下所示：

```csharp
public partial class SignUpViewModel : ObservableValidator
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter an email like name@example.com.")]
    private string _email = "";

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    private string _password = "";

    [RelayCommand]
    private void Validate() => ValidateAllProperties();        // Form.ValidateCommand

    [RelayCommand]
    private async Task CreateAccountAsync()                    // Form.SubmitCommand
    {
        ValidateAllProperties();                               // the form only sees mounted fields
        if (HasErrors)
        {
            return;
        }
        await _api.CreateAsync(Email, Password);
    }
}
```

一个项显示第一条错误，并把所有错误保存在 `Errors` 中。同一属性上的多条规则按顺序报告，因此显示的是第一条不通过的规则。绑定无法转换的值（例如绑定到 `int` 的数字中输入的 `abc`）同样会被收集，并阻止提交，尽管视图模型看不到它。

不要在 `ObservableValidator` 的同时启用 Avalonia 的 DataAnnotations 插件（`WithDataAnnotationsValidation()`）：两者会报告相同的规则，每条错误都会显示两次。裁剪和 AOT 应用最好使用手写规则，因为 .NET 的 DataAnnotations `Validator` 需要反射。

### 提交

`SubmitAsync()`（或带有 `arc:Form.Action="Submit"` 的按钮；设置 `IsDefault="True"` 时按 Enter 即提交）执行以下步骤：

1. 显示每个项的错误，并运行 `ValidateCommand`，因此用户没有触碰过的字段也会被检查。
2. 在任一项 `IsValidating` 为 true 时等待。此期间再次提交会返回同一个任务；`ResetFields()` 或表单卸载会停止等待。
3. 有错误时：引发 `SubmitFailed`，其中的无效项按文档顺序排列；把第一个无效项滚动到可见区域，连同其标签一起显示，并聚焦其字段，同时播报还有几个字段需要处理。在处理程序中设置 `SuppressFocus`，即可自行移动焦点，例如在切换标签页之后。
4. 没有错误时：引发 `Submitted`，然后以 `SubmitCommandParameter` 作为参数运行 `SubmitCommand`。

预览中的提交按钮位于折叠线以下：提交一个空表单时，视图会滚动回第一个错误。

| 方法 | 作用 |
|---|---|
| `Validate()` | 显示所有错误，运行 `ValidateCommand`，并返回表单是否有效（检查进行中时为 false） |
| `ValidateAsync()` | 与 `Validate()` 相同，但会先等待正在进行的检查 |
| `SubmitAsync()` | 即上面的提交；表单有效且命令已运行时返回 true |
| `ResetFields()` | 运行 `ResetCommand`，然后调用 `ClearValidation()`，最后引发 `FieldsReset` |
| `ClearValidation()` | 隐藏已显示的错误并忘记提交状态；视图模型的错误保留（即 issue 中的 `ClearValidate()`） |
| `FocusFirstError()` | 滚动到并聚焦第一个无效项，例如在收到服务器错误之后 |
| `GetInvalidItems()` | 按文档顺序返回无效项，便于你自行汇总 |

`HasErrors`、`ErrorCount`、`IsValidating` 和 `IsSubmitAttempted` 是只读属性，可以用于绑定。

### 何时显示错误

`ErrorTrigger` 决定项何时开始显示其已收集的错误。它本身不运行任何规则：视图模型何时检查某个值取决于绑定（`UpdateSourceTrigger=LostFocus` 只在离开字段时检查）。

| `ErrorTrigger` | 显示错误的时机 |
|---|---|
| `Change` | 错误一出现就显示，与独立字段相同 |
| `Blur`（默认） | 焦点离开该项之后（打开其下拉列表不算），或在提交时 |
| `Submit` | 仅在提交或调用 `Validate()` 时 |

一旦某个项开始显示错误，修正值会立即隐藏该错误；之后出现的新错误会在到达时显示。`ErrorTrigger` 是可继承的附加属性：可以设置在表单上、分区面板上或某一个项上。

<Demo name="ErrorTrigger" />

### 异步检查

视图模型会做防抖、调用服务器，并通过 `ErrorsChanged` 报告结果；它还保存一个标志，项将其绑定到 `IsValidating`。该标志为 true 时，项会显示“正在检查…”，而不是旧的错误，同时提交会等待检查结束。在 `finally` 块中清除该标志：表单会无限期等待，没有超时。

<Demo name="AsyncCheck" />

### 跨字段规则

比较两个字段的规则（例如密码确认）应放在视图模型中：密码改变时，重新检查确认项。错误显示在报告它的那个项上；使用 `Blur` 时，在确认项被离开之后显示。预览就是这样实现的。

### 错误显示在哪里

`ErrorPlacement`：`Inline`（默认）在字段下方打开一行显示错误；`Tooltip` 不占用空间，在鼠标悬停以及项获得键盘焦点时，于字段下方显示错误，适用于密集的表单；`None` 只显示无效状态，文本由你自行处理（`FormItem.Errors`、`GetInvalidItems()`）。`ErrorTemplate` 可以按你的方式呈现错误对象，无论是内联显示还是在提示框中。显式设置的 `FormItem.ErrorMessage`（例如服务器错误）会立即显示，并优先于绑定产生的错误。

<Demo name="ErrorPlacement" />

### 标签位置

`LabelPlacement="Left"` 把标签放在一列中。`LabelWidth` 保持 NaN 时，这一列取表单中最宽标签的宽度（最多 240 px；更长的标签会换行）；在分区上设置 `Grid.IsSharedSizeScope="True"` 可以只让该分区内部对齐，或将 `LabelWidth` 设为固定宽度。标签相对于项的 `Size` 所对应的字段居中（为小字段的项设置 `Size="Small"`）。

<Demo name="LabelPlacement" />

`RequiredIndicator` 用星号标记必填项（默认），或将其他项标记为“可选”，也可以不显示任何标记。`IsRequired` 本身不添加任何规则。

### 动态增减的项

`ItemsControl` 模板中的项会在其行被添加时加入表单，在行被移除时离开表单。每一行保留自己的规则；父视图模型在提交时检查这些行。

<Demo name="DynamicItems" />

表单只统计已挂载的项：未选中标签页上的字段、虚拟化列表尚未生成的行，以及已关闭的弹出层都不在其中。请在视图模型的提交命令中继续检查 `HasErrors`，或将 `ErrorSource` 设为视图模型：此时它的 `HasErrors` 会阻止提交（`SubmitFailed.FirstInvalidItem` 为 null），其实体级错误（`GetErrors(null)`）会出现在 `FormErrors` 中，供你自行显示提示。某些实现（例如 `ObservableValidator`）会为 `GetErrors(null)` 返回所有错误。

### 只读与关闭校验

`IsReadOnly`（可继承，因此也可用于分区）会让 Arc 文本字段、`Cascader`、`TreeSelect`、`Rating`、`TokenTextEditor` 和 `RichTextEditor` 变为只读，并隐藏必填标记。复选框、开关、选择器、日期时间选择器和滑块目前还没有只读模式，仍然可以编辑：请将它们禁用，或用 `IsEnabled="False"` 禁用整个表单。在表单或某个项上设置 `IsValidationEnabled="False"`，会隐藏其错误、停止统计，并允许提交通过；同时也应在视图模型中关闭对应的规则。已禁用、已隐藏和只读的项会被自动跳过。

<Demo name="ReadOnly" />

### 项内的字段

项内的 Arc 字段把标签和错误行交给项来显示，只有在项显示错误时才变为红色；它们自己的描述行保留。下表中的值属性会报告其绑定的校验错误，因此可以在表单中使用：

| 控件 | 已校验的属性 |
|---|---|
| `Input`、`Textarea`、`PasswordField`、`SearchField` | `Text` |
| `NumberField`、`MoneyInput`、`PhoneInput` | `Value`（以及 `Text`） |
| `DatePicker`、`TimePicker`、`DateRangePicker`、`ColorPicker` | `SelectedDate`、`SelectedTime`、`SelectedRange`、`Color` |
| `Checkbox`、`Switch` | `IsChecked` |
| `Select`、`Combobox`、`MorphSelect`、`MultiSelect`、`RadioGroup`、`RadioCards`、`SegmentedControl`、`ChipGroup`、`ToggleGroup` | `SelectedItem`、`SelectedValue`、`SelectedIndex`（`MultiSelect` 和 `ChipGroup` 上为 `SelectedItems`） |
| `Cascader`、`TreeSelect` | `SelectedItem`、`SelectedValue`（`TreeSelect` 上为 `SelectedValues`） |
| `TagInput`、`Slider`、`Rating`、`ScrubInput`、`ShortcutRecorder`、`EnumSelector` | `Value`（范围 `Slider` 上为 `LowerValue` 和 `UpperValue`），`SelectedEnum` |

一个控件只报告一组错误：当其两个已校验的属性都被绑定时，最后一次报告生效。其他库的控件保留自己的错误显示（Fluent 主题的 `DataValidationErrors`），它会显示在项的行旁边；如需关闭，请使用样式。

## API 参考

<!-- api: Form -->

<!-- api: FormItem -->

<!-- api: FormSubmitFailedEventArgs -->

<!-- api: FormLabelPlacement -->

<!-- api: FormErrorTrigger -->

<!-- api: FormErrorPlacement -->

<!-- api: FormRequiredIndicator -->

<!-- api: FormAction -->

## 无障碍

- 表单是一个表单地标；请使用 `AutomationProperties.Name` 为它命名。
- 项为其字段提供标签（`Target`，或第一个可聚焦的编辑器）：标签为 `LabeledBy`，除非你另行设置，否则标签文本即名称；`IsRequired` 对应 `IsRequiredForForm`；帮助文本包含描述，以及显示后的错误。尚未显示的错误也不会被朗读。
- 新错误以强提示方式播报；使用 `Tooltip` 或 `None` 位置时，一段隐藏的实时文本会播报相同内容。一次提交或 `Validate()` 显示多个错误时，只播报一条摘要（“2 个字段需要处理”），然后焦点移到第一个无效字段，朗读其名称和错误。
- “正在检查…”以礼貌方式播报。星号和“可选”是装饰性的；文本跟随应用语言（en-US、zh-CN、zh-TW）。

### 键盘

| 按键 | 行为 |
|---|---|
| Tab | 按内容顺序在各字段之间移动 |
| Enter | 通过带有 `arc:Form.Action="Submit"` 和 `IsDefault="True"` 的按钮提交 |

提交失败后，焦点以键盘焦点的方式移到第一个无效字段，因此非文本控件也会显示焦点环。表单不处理其他任何按键。

## 动效

- 错误、描述和“正在检查…”行以平滑的弹簧打开和关闭，并且只改写发生变化的文字，与字段的消息行相同。字段的边框以其常规过渡变为红色；提示框的移动与任何提示框相同。
- 由 `ErrorTemplate` 呈现的错误无动画地出现，滚动到第一个错误也是立即完成的。
- 减少动效时，各行以完整高度出现，文字淡入；关闭动效时一切立即完成。

## 响应式

- 表单占用所分配的宽度，项会拉伸到该宽度。
- 左侧标签共用一列，超过 240 px 后换行。表单不会在窄屏上把标签切换到顶部：在窄布局中请选择 `LabelPlacement="Top"`（例如根据 `ArcViewport.IsCompact`）。
- 提示框的位置可以防止在出现错误时，高密度表单的行被撑高。

## 相关组件

- `Field`：单个编辑器的标签、描述和错误结构。
- `Input`、`PasswordField`、`Select`、`Checkbox`：可放入项中的字段。
- `PropertyGrid`：无需编写表单即可编辑对象的属性。
- `MultiStepForm`：拆分为多个步骤的表单。
