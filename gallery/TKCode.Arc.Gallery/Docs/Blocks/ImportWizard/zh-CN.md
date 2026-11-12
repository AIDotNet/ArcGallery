---
title: Import wizard
summary: 单张卡片内的四步 CSV 导入：拖入文件并预览，把各列匹配到字段（每个猜测附带置信度），在计数递减的同时修正有问题的单元格，最后分批导入并查看摘要。
source-hash: 011ba077be94
---

<Demo name="Preview" />

## 何时使用

`ImportWizard` 把电子表格导出的行导入产品。用户拖入一个 CSV 文件，确认各列落到了正确的字段，修正有问题的内容，然后导入其余部分：

- **导入其他工具导出的联系人、客户、产品或用户。**
- **初始化流程**，产品需要客户已有的数据才能真正可用。
- **批量更新记录的管理工具**，已有主键的行会变成更新，而不是重复新增。

## 何时不使用

- **给记录附加文件。** 请用 `FileUpload` 或 `FileDropzone`，它们不做解析。
- **只做列映射而不检查取值。** 每列配一个 `Select` 的表单就够了。
- **与其他工具保持数据同步。** 请使用按计划同步的集成；向导只用于一次性导入。

## 用法

该卡片派生自 Avalonia 的 `TemplatedControl`，无需任何数据：未设置任何属性时，它使用联系人架构（姓名、邮箱、公司、职位、电话、国家、套餐），在 "Try a sample file" 后面提供一份示例客户列表，并模拟导入过程。

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:ImportWizard BatchSize="200" Noun="contact" PluralNoun="contacts" />
</UserControl>
```

### 四个步骤

`Step` 表示当前显示的步骤，`StepChanged` 报告每一次变化。步骤条中已完成的步骤可以跳回该步；导入进行期间，任何步骤都不能跳转。

1. **上传**：把 CSV 拖到区域上，点击（或按 Enter）选择文件，也可以加载示例。文件读取时显示一行简短的进度，随后显示文件名、大小、行数和列数，以及前五行。列数少于两列、没有数据行、引号未闭合或行数超过 `MaxRows` 的文件会被拒绝，并在区域下方显示提示。
2. **匹配字段**：文件的每一列对应一行，显示两个示例值和一个字段选择框。各列依次完成匹配，每列都带有置信度：90 及以上为绿色，80 到 89 为中性色，更低则为琥珀色。手动选择的字段显示 "Chosen"，被排除的列显示 "Ignored"，同一字段被两列选中时，两列都显示 "Duplicate"。每个必填字段都必须恰好对应一列，"Check rows" 才可用。
3. **检查**：有问题的行排在最前面（"To fix"），每个错误单元格会以底色标出，并在值下方显示说明。点击单元格（或按 Enter）即可编辑；按 Enter 或离开单元格会保存，按 Escape 取消。"Clean up n values" 会逐个单元格应用所有安全修复。行末的 x 会把该行排除在外。仍有问题的行会被排除在导入之外，但不会阻止导入。
4. **导入**：可导入的行按 `BatchSize` 分批发送，并显示进度条；随后摘要会给出新建、更新（其主键在 `ExistingKeys` 中）和被排除的数量。"Download left-out rows" 把被排除的行另存为 CSV，"Start a new import" 则重新开始。

### 连接你的服务器

`ImportBatchAction` 接收每个批次以及一个取消令牌；点击取消、重新开始或卡片离开窗口，都会取消它。出现异常时导入停止，显示异常信息和 "Try again"，并触发 `Failed`；重试会重新发送失败的批次。所有批次完成后，`Completed`（以及 `CompletedCommand`）报告总计。

```csharp
wizard.ImportBatchAction = async (records, cancellation) =>
{
    var response = await http.PostAsJsonAsync("/api/contacts/bulk", records.Select(r => r.Values), cancellation);
    response.EnsureSuccessStatusCode();
};

wizard.Completed += (_, e) => toast.Show($"{e.Created + e.Updated} contacts imported");
```

<Demo name="Handlers" />

### 自定义字段

把字段描述为 `ImportTarget`。`Headers` 是与该字段含义完全一致的名称，`Synonyms` 是常见的替代写法；表头没有提示时，`Sniff` 负责识别取值。`Validate` 为错误的值返回简短的提示；`Fix` 返回安全的清理结果（去除空格、转为小写、展开缩写）或 null，并驱动 "Clean up"。用 `IsKey` 标记用于识别已有记录的字段，并通过 `ExistingKeys` 传入已知的主键。`Noun` 和 `PluralNoun` 为文案中的记录命名，`SampleCsv` 替换示例文件（设为 null 则隐藏该按钮）。

<Demo name="CustomFields" />

匹配器的打分规则如下：表头等于字段的标签、键名或某个 `Headers` 的列得 99 分；同义词且取值看起来正确的列得 95 分；只有同义词的列得 88 分；只有取值匹配的列得 71 分；只共享某个单词的列得 64 分。每个字段都会分配给得分最高的列。

### 窄容器

卡片会填满容器，最宽 760。宽度低于 `CompactWidth`（520）时，步骤条只显示 "Step 1 of 4" 和标记，映射行把选择框堆叠到列名下方，审阅表上方的工具栏会换行。预览和审阅表在各自的框内横向滚动。

<Demo name="Narrow" />

### 文案

卡片显示或播报的每个词都是 `ImportWizardLabels` 的属性；给 `Labels` 赋一个新实例即可翻译或改写。含数字的文案是返回格式字符串的函数：`{0}` 和 `{1}` 标记滚动数字的位置，`{noun}` 取单数或复数名词。

## API 参考

<!-- api: ImportWizard -->

<!-- api: ImportTarget -->

<!-- api: ImportRecord -->

<!-- api: ImportWizardCompletedEventArgs -->

## 无障碍

- 步骤条是名为 "Import steps" 的列表；每个步骤按钮读出其名称、序号以及是否已完成（"Upload, step 1 of 4, done"），当前步骤会被标记。
- 拖放区域是名为 "Choose a CSV file" 的按钮；预览和审阅表是可聚焦的区域，可用方向键横向滚动。
- 每个字段选择框都以其所在的列命名（"Field for Email"）。置信度以数字形式读出，而不只是一条进度条；"Chosen"、"Ignored" 和 "Duplicate" 也都是文字。
- 有问题的单元格会把提示信息放进其名称中（"Email, line 3: Check the email format. Press Enter to edit"）。
- 审阅的计数和导入进度是礼貌型实时区域；错误（文件无法读取、缺少字段、批次失败）会立即播报。进度条报告当前值和总值。

### 键盘

- **Tab** 依次经过步骤条中已完成的步骤、该步骤的控件以及页脚。
- 在拖放区域上按 **Enter** 或 **Space** 打开文件选择框。
- 在审阅单元格上按 **Enter** 或 **Space** 打开编辑器；**Enter** 保存，**Escape** 取消，焦点回到该单元格。

## 动效

- **卡片**：高度随每个步骤以平滑弹簧变化。步骤按行进方向滑动 28 px 并模糊 4 px，下一步则从另一侧滑入。
- **上传**：文件卡片升起的同时拖放区域收缩消失；一条 2 px 的线在 420 毫秒内读完文件，随后五行预览依次升起，相隔 40 毫秒。
- **匹配**：每列在解析完成前显示 "Matching" 并脉动，每 117 毫秒解析一列；分数清晰地浮现，其计量条以平滑弹簧填充。
- **审阅**："Clean up" 每 90 毫秒修复一个单元格，计数逐级递减形成级联；已修复的单元格以绿色辉光显示 900 毫秒。在 "To fix" 中，这些行会一起滑出，其余行随后收拢。
- **导入**：计数和进度条随每个批次更新；摘要中的数字只计数一次，对勾自行绘制。
- 减少动效时保留淡入淡出，去掉位移、阅读线和级联，并立即显示最终数字。关闭动效时，所有变化立即生效。

## 响应式

- `ImportWizard` 按自身宽度而不是窗口宽度改变布局。宽度低于 `CompactWidth`（默认 520 px）时，它设置 `:compact`：步骤条只显示计数和标记，映射行堆叠，审阅栏换行。
- 卡片在任何尺寸下最宽为 760 px（`MaxWidth`）。
- 文件名和较长的单元格值以省略号截断；表格在各自的框内横向滚动，因此页面本身从不横向滚动。
- 触屏：没有任何操作依赖悬停。点按单元格即可打开编辑器，所有操作都是可见的按钮。

## 相关

- `FileDropzone`、`Select`、`SegmentedControl`、`Progress`、`AnimatedCounter` 和 `InlineEdit`，即卡片所遵循模式的来源控件。
- File upload，用于附加文件而非导入其中的行。
- API keys、Webhooks、Integrations、Env variables、Domain verify 和 Webhook tester，即 Developer 分组里的其他区块。
