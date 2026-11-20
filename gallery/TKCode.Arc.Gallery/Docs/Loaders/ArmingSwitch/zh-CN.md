---
title: Arming switch
summary: 驾驶舱风格的带护盖开关，用于有风险的操作：掀起半透明护盖，把镀铬拨杆扳过中点直到它弹到位、指示灯亮起，再按住指示灯来执行操作。
source-hash: 120c26e42d6d
---

<Demo name="Preview" />

## 何时使用

`ArmingSwitch` 把一个危险操作变成一套简短的仪式。拉丝铝面板上有一根镀铬拨杆，罩在半透明的安全护盖下面，旁边是一盏外围带圆环的指示灯。沿铰链掀起护盖，把拨杆向上扳，直到它越过中点弹到位、指示灯闪烁着亮起，然后按住指示灯直到圆环填满。操作开始执行，指示灯变绿，护盖随后自动落下，在落下途中把拨杆推回安全位置。任何时候合上护盖都会让已待发的开关回到安全状态，和真实面板一样。

- **生产发布、数据迁移等不可逆操作**：绝不能因为一次误点就开始。
- **设置中的危险区域**：一个带保护的控件承担整个决定，而且有足够的空间放下它。
- **发布时刻或产品页面**：展示一个工具多么认真地对待破坏性操作。

## 何时不用

- **密集列表或工具栏中的快速确认**：使用 `HoldToConfirm` 或 `SlideToConfirm`。
- **需要用户证明自己清楚要删除什么时**：使用要求输入名称的对话框。
- **可以随意来回切换的普通设置**：使用 `Switch`。

## 用法

为操作命名，说明它影响什么，并提供要执行的工作。`ConfirmAction` 是一个 `Func<Task<bool>>`：任务运行期间状态为 `Running`（指示灯脉动、圆环旋转）；`true` 表示成功，`false` 或异常表示失败。没有设置工作时，按住完成即直接成功。

```xml
<arc:ArmingSwitch x:Name="Purge"
                  Title="Purge the archive"
                  Description="cold-storage, 1.2 TB in 18 buckets"
                  HoldLabel="Hold to purge"
                  RunningStatusText="Purging"
                  DoneStatusText="Purged"
                  ArmedHint="Press and hold the lamp to purge the archive."
                  DoneHint="Archive purged. This cannot be undone." />
```

```csharp
Purge.HoldDuration = TimeSpan.FromMilliseconds(1800);
Purge.ConfirmAction = async () =>
{
    var result = await storage.PurgeAsync(archive.Id);
    return result.Succeeded;
};
Purge.StatusChanged += (_, e) =>
{
    if (e.NewStatus == ArmingSwitchStatus.Failed)
    {
        log.Warn(e.Error);
    }
};
```

`ArmingSwitch` 位于 Special 包中：在 `UseArcControls()` 旁边用 `UseArcSpecial()` 注册。它占用给定的宽度；面板最宽 560 px。

### 状态

| 状态 | 显示内容 |
|---|---|
| `Safe` | 护盖合上，开关处于安全位置，指示灯熄灭，就绪灯在跳动 |
| `Open` | 护盖翻起，可以扳动开关 |
| `Armed` | 开关扳上，指示灯亮起琥珀色并缓慢呼吸；按住指示灯会填充圆环 |
| `Running` | `ConfirmAction` 的任务运行期间，圆环旋转、指示灯脉动 |
| `Done` | 指示灯变绿；启用 `IsAutoSafeEnabled` 时护盖稍后落下，把开关推回安全位置。绿灯一直亮到护盖再次掀起 |
| `Failed` | 指示灯闪红、面板抖动、红色圆环排空；开关保持待发，再次按住即可重试 |

标题旁的状态标签把图标和状态文字配在一起，面板下方的一行说明下一步该做什么；两者都可以按状态改名（`SafeStatusText` … `FailedStatusText`、`SafeHint` … `FailedHint`）。`StatusChanged` 报告每一次变化，操作抛出异常时附带该异常。`CoverOpenChanged` 和 `ArmedChanged` 跟随护盖与开关的变化，包括落下的护盖把开关推回的情况。圆环填满时、操作运行之前触发 `Confirmed`。

`OpenCover()`、`CloseCover()`、`Arm()` 和 `MakeSafe()` 可以在代码中驱动它。`IsLocked` 让每个部件仍可获得焦点但都会拒绝操作：尝试时护盖会晃动，提示行说明开关已锁定。

### 强调色与护盖

按住圆环使用主题的强调色，护盖也由它着色，并把亮度限制在一个范围内，使其在两种主题下都保持半透明。`AccentBrush` 改变圆环颜色，`CoverBrush` 改变护盖及其投下的着色阴影，例如删除操作可用安全红。

### 内容准则

- **Title**：写操作本身，而不是产品："Ship to production"、"Purge the archive"。
- **Description**：用一行说明它影响什么：服务与版本、区域、数据量。
- **HoldLabel**："Hold to" 加动词；它也是指示灯的无障碍名称。
- **状态文字与提示**：用于发布以外的操作时一起改名，让状态标签显示 "Purging" 的同时提示行解释正在发生什么。

### 最佳实践

- 它是阻力，不是授权。在 `ConfirmAction` 中到服务器检查权限，操作未完成时返回 `false`。
- 返回真实的任务，并且只在工作真正完成时才让它结束，这样 `Running` 才如实反映情况。
- 一次性操作保持 `IsAutoSafeEnabled` 开启。用户可能马上再次执行时关闭它。

## API 参考

<!-- api: ArmingSwitch -->

<!-- api: ArmingSwitchStatus -->

<!-- api: ArmingSwitchStatusChangedEventArgs -->

## 无障碍

- 控件是一个以 `Title` 命名的分组。护盖、开关和指示灯是叠放在绘制内容上的真实可聚焦元素，因此指针、触摸、键盘和屏幕阅读器都作用于同样的三个部件；面板本身对辅助技术隐藏。
- 护盖是名为 "Lift the safety cover" 或 "Close the safety cover" 的按钮。开关是以 `ArmLabel` 命名的切换控件（"Arm switch"），并报告是否已待发。指示灯以 `HoldLabel` 命名。
- 每个部件的帮助文本就是面板下方那一行，因此每个部件都会说明下一步做什么。"Lift the cover first." 这类拒绝提示会替换这一行约两秒并被朗读。
- 礼貌型实时区域在每次状态变化时朗读状态和下一步，例如 "Armed. Press and hold the lamp to ship."
- 状态从不只依赖颜色：状态标签把图标和文字配在一起，且宽度在各状态之间保持不变。
- 键盘焦点会在获得焦点的部件周围画出 2 px 的圆角焦点环。

### 键盘

| 按键 | 操作 |
|---|---|
| `Tab` | 依次在护盖、开关和指示灯之间移动 |
| 护盖上的 `Enter` / `Space` | 掀起护盖或合上护盖（合上会让已待发的开关回到安全状态） |
| 护盖上的 `↑` / `↓` | 掀起或合上护盖 |
| 开关上的 `Enter` / `Space` | 让开关待发或回到安全状态；护盖合上时拨杆会撞到盖板，提示行要求先掀起护盖 |
| 开关上的 `↑` / `↓` | 让开关待发或回到安全状态 |
| 在指示灯上按住 `Enter` 或 `Space` | 在 `HoldDuration` 内填满圆环，然后执行操作；提前松开会排空圆环 |
| `Escape` | 让一切回到安全状态：结束按住、合上护盖、开关回到安全位置（操作运行期间忽略） |

## 动效

- 一个帧循环以弹簧驱动所有部件（240 Hz），没有部件运动时就休眠。面板沿柔和的弹簧朝指针倾斜几度；光源固定在房间里，所以拉丝面板上的条纹会漂移，抬起的盖板上会滑过反光，阴影会滑动并变短。
- 护盖是铰链上的半透明 3D 盒子。拖动时盖沿始终保持在指针下方，越过打开的止点会有橡皮筋效果，并保留松手时的速度。它被弹簧压向止点：约 200 ms 弹开并轻微回弹，落下时有一次短促的跳动。悬停或聚焦时抬起 5°，首次出现时会跳动一下。
- 拨杆在越过中点前一比一跟随指针，越过后由过中点弹簧在约 65 ms 内把它甩到另一端。没过中点就松开，它会弹回原位。
- 待发时指示灯像灯丝一样亮起：两次快速闪烁、一圈光晕和洒在面板上的暖光，然后缓慢呼吸。按住时镜片被压下，圆环带着明亮的前端点逐渐填满，指示灯变得更亮。
- 圆环填满时，一圈光环从指示灯弹出，面板轻微一顿。成功时指示灯渐变为绿色，伴随更宽的光环和一道扫过面板的反光；失败时闪红、面板抖动，红色圆环排空。
- 控件不在屏幕上时帧循环停止。

| 变化 | 完整 | 减少 | 关闭 |
|---|---|---|---|
| 护盖、开关 | 弹簧、回弹、跳动 | 直接跳到新姿态 | 直接跳变 |
| 面板倾斜、漂移、就绪灯、呼吸 | 有 | 无 | 无 |
| 指示灯 | 闪烁、光晕、运行时脉动 | 淡入淡出，脉动更慢 | 常亮 |
| 按住圆环 | 稍有滞后地填充，按弹簧排空 | 填充，线性排空 | 填充，线性排空 |
| 光环、反光、抖动 | 有 | 无 | 无 |

在任何动效等级下按住都仍会填充圆环，因此操作始终保留这道保护。

## 响应式行为

- 面板按自身单位绘制，因此护盖、拨杆、指示灯、螺丝和刻字从手机到桌面卡片都保持比例，面板最宽 560 px。刻字不会小于 9 px。
- 宽度达到 720 px 时，标题、状态和提示排成一列放在面板旁边。低于此宽度时它们上下堆叠，卡片去掉外框，并用一条线隔开提示行；低于 420 px 时内边距收紧。
- 舞台在面板上方为翻起的护盖留出空间，因此它不会遮住标题。

## 相关组件

- `HoldToConfirm`：紧凑的按住确认。
- `SlideToConfirm`：在例行操作前做出明确的水平手势。
- `ConfirmMorph`：变成内嵌确认的破坏性按钮。

| 组件 | 适用场景 |
|---|---|
| `ArmingSwitch` | 代价高、难以撤销、值得两步仪式且有空间放下的单个操作 |
| `HoldToConfirm` | 在小空间里按住确认的破坏性操作 |
| `SlideToConfirm` | 触摸屏上例行但重要的操作 |
