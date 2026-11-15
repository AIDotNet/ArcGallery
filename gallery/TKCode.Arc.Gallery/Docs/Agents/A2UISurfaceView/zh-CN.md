---
title: A2UI 界面
summary: "用 Arc 控件渲染 A2UI 1.0（智能体到用户界面）消息：智能体用 JSON 描述一个界面，界面视图把它画出来，保存用户输入的数据，并把用户的操作发回智能体。"
source-hash: d4162f1ba2cb
---

<Demo name="Preview" />

## 何时使用

- AI 智能体需要用界面而不是文字回答：要填写的表单、要做的选择、要比较的列表或图表。
- 界面在运行时由模型或服务端生成，又必须留在你的设计系统里：智能体只能使用你登记的组件，不能写代码或样式。
- 多个前端共用一个协议：A2UI 在网页、移动端和这里都一样。

## 何时不用

- 自己用 XAML 或 C# 搭的界面：直接用控件。
- 富文本和长回答：用 `Markdown`。

## 用法

添加包并在 `UseArc` 中注册：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcA2UI());
```

创建 `A2UIRenderer`，把智能体的消息（每行一个 JSON 对象）交给它，再用 `A2UISurfaceView` 显示一个界面。视图会等到界面被创建，随组件到达逐步填充，界面被删除时清空。

```csharp
var renderer = new A2UIRenderer();
renderer.Outbound += (_, e) => SendToAgent(e.ToJson());   // 动作、函数调用、错误
var view = new A2UISurfaceView { Renderer = renderer, SurfaceId = "booking_1" };

await renderer.ProcessAsync(responseStream);                // JSON Lines，在 UI 线程上应用
```

### 双向数据

输入组件（`TextField`、`CheckBox`、`ChoicePicker`、`Slider`、`DateTimeInput`）会把用户输入写进界面数据模型中 `value` 绑定的路径，绑定同一路径的其他组件立即更新。在动作发生之前不会向智能体发送任何内容；设置了 `sendDataModel` 时，数据随动作一起发出（`GetRendererDataModel`）。

<Demo name="Form" />

### 校验与动作

`@checks` 规则用目录里的函数（`required`、`email`、`regex`、`length`、`numeric`、`and`、`or`、`not`）校验输入和按钮；校验不通过的按钮会被禁用。按钮的 `action.event` 会变成一条解析好 `context` 的 `action` 消息；`action.functionCall` 运行本地函数，例如 `openUrl`，它会先触发 `UrlOpening`。

### 目录

渲染器默认支持两个目录：

- A2UI 基础目录（`A2UICatalogs.Basic`，18 个组件、14 个函数）：文本、图片、图标、行与列、列表、卡片、标签页、弹窗、按钮和各种表单输入；
- Arc 目录（`A2UICatalogs.Arc`，18 个用 Arc 控件绘制的组件）：`BarChart`、`LineChart`、`DonutChart`、`Sparkline`、`Metric`、`Table`、`Timeline`、`Stepper`、`Progress`、`Alert`、`Badge`、`Avatar`、`Markdown`、`CodeBlock`、`Accordion`、`Switch`、`SegmentedControl` 和 `Rating`。组件写上 `"@catalogId": "https://github.com/AIDotNet/TKCode.Arc/a2ui/catalogs/arc/v1"` 即可使用；其中的输入组件和基础目录一样双向绑定，Markdown 里的链接会先经过 `UrlOpening`。

可以用 JSON Schema 文档添加自己的目录，并为每个组件登记一个视图：

```csharp
var charts = A2UICatalog.FromJson(chartsCatalogJson);
var options = new A2UIRendererOptions { Catalogs = [A2UICatalogs.Basic, A2UICatalogs.Arc, charts] };
var registry = A2UIComponentRegistry.CreateDefault()
    .Register(charts.Id, "BarChart", () => new MyBarChartView());
```

组件用 `@catalogId` 选择目录；由智能体实现的函数会通过 `callAgentFunction` 发给智能体。

### 校验模式

默认（`A2UIValidationMode.Lenient`）每个组件都按目录 Schema 校验，还没到达的子组件显示为占位，适合模型流式输出的回答。`Strict` 还会拒绝不完整的批次（缺少 root、悬空引用、环）。被拒绝的消息通过 `ErrorReported` 报告，同时作为 `error` 消息发给智能体，模型可以据此自行修正。

## API 参考

<!-- api: A2UISurfaceView -->

## 无障碍

`@accessibility` 映射到平台：`label` 对应自动化名称，`description` 对应帮助文本，`live` 对应实时播报设置，`hidden` 对应原始自动化视图。Arc 控件保留各自的键盘和读屏行为。

## 动效

组件沿用绘制它们的 Arc 控件的动效。回答流式到达时，新组件占据自己的位置，不推动相邻组件；视图本身没有额外动画。

## 响应式

行、列和列表使用给定的宽度；横向列表可以滚动。智能体可能发送很长的界面时，把视图放进可滚动的容器。

## 相关组件

- `Markdown`：同一智能体的富文本回答。
- `ChatThread`：消息里可以放界面的会话。
- `Form` 与 `FormItem`：用 XAML 编写的表单。
