---
title: Month calendar
summary: 以月为单位的日历应用，事件色块和横条按车道排列并显示“N more”，可停靠的日面板包含忙碌时间、可用于专注的空闲时段和事件详情，支持拖动改期或跨日规划，以及近期安排、邀请和同事日历视图。
source-hash: 19c45aece409
---

<Demo name="Preview" />

## 何时使用

`MonthCalendar` 是一个完整的日历界面：侧边栏、月视图网格和日面板。网格一眼就能看到整月的所有事件；面板详细显示所选的一天，包括已占用了多少时间、可用于专注的空闲时段，以及打开某个事件时的详情。人们可以就地安排：把事件拖到另一天，拖过空白日期来规划出差或发布周，回复邀请，并通过提示条撤销移动或删除。

- **日历或排期应用的月视图。** 加载日历、人员和事件，并通过本界面引发的事件与它们保持同步。
- **团队与内容日历。** 每个日历都有自己的颜色、图标和侧边栏开关；同事的月视图只需一次点击即可打开。
- **涵盖大量事件和多日时段的规划。** 出差、会议和发布周会以横条显示在其覆盖的各天，当天自己的事件位于横条下方。

## 何时不用

- **逐小时排期。** 请使用 `WeekCalendar`，它把事件排布在时间网格上。
- **在表单中选择一个日期。** 请使用 `Calendar` 或 `DatePicker`。
- **带依赖关系的项目阶段。** 请使用 `GanttTimeline`。

## 用法

界面会填满其容器，因此要给它一个高度（窗口、页面，或作为框架时使用 900）。添加日历、人员和事件，并指定当前登录的是谁：

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:MonthCalendar x:Name="Planner" Height="900" WorkspaceName="Tidewater" CurrentUserId="maya">
        <arc:MonthCalendar.Calendars>
            <arc:MonthCalendarSource Id="studio" Name="Studio" Tone="Teal" />
            <arc:MonthCalendarSource Id="home" Name="Home" Tone="Amber" />
        </arc:MonthCalendar.Calendars>
        <arc:MonthCalendar.People>
            <arc:MonthCalendarPerson Id="maya" Name="Maya Okafor" Email="maya@tidewater.studio" IsTeammate="False" />
            <arc:MonthCalendarPerson Id="jonah" Name="Jonah Reyes" Email="jonah@tidewater.studio" />
        </arc:MonthCalendar.People>
    </arc:MonthCalendar>
</UserControl>
```

```csharp
var day = new DateOnly(2026, 11, 12);
Planner.Events.Add(new MonthCalendarEvent("e1", "Daily sync", "studio", day, new TimeOnly(9, 15), new TimeOnly(9, 30))
{
    VideoProvider = "Video room",
    VideoUrl = "https://video.example/daily-sync",
});
Planner.Events.Add(new MonthCalendarEvent("e2", "Quarter planning", "studio", day) { EndDate = day.AddDays(1) });

Planner.EventCreated += (_, e) => api.Create(e.Event);
Planner.EventChanged += (_, e) => api.Update(e.Event);
Planner.EventDeleted += (_, e) => api.Delete(e.Event.Id);
```

### 事件、日历与人员

`MonthCalendarEvent` 包含 `Id`、`Title`、`CalendarId`、`StartDate` 以及可选的、包含端点的 `EndDate`。没有 `StartTime` 时它是全天事件；有 `StartTime` 时它是定时事件，结束于 `EndTime`（未设置时为一小时后）。全天和多日事件以带色调的横条绘制；定时事件以色块绘制，并用日历颜色显示时间。事件可以带有 `Location`、视频会议（`VideoProvider` 和 `VideoUrl`）、`Notes`、`OrganizerId` 以及 `Attendees`，每个参与者都是一个 `MonthCalendarAttendee`，包含一个人员和一个回复。

`Rsvp` 是当前登录者的回复。`Pending` 表示邀请：其色块为描边样式，列在 Invitations 下，其详情提供 Going、Maybe 和 No。已拒绝的事件标题带删除线，且不再计入忙碌时间。

`MonthCalendarSource` 有 `Name`、`Tone`（`Teal`、`Violet`、`Amber`、`Blue`、`Rose`、`Green`、`Neutral`，或带 `CustomColor` 的 `Custom`）以及可选的 `Icon`，可以是任意 24 × 24 线条图标，例如 `ArcIcons` 中的图标；默认为日历图标。`IsShown` 是侧边栏的开关。人员是 `MonthCalendarPerson`；前五个带 `IsTeammate` 的人员填充侧边栏的团队日历，`CurrentUserId` 指定当前登录的人。

### 月视图

网格显示该月跨越的周，共五或六周，平分高度。每一天把事件排入车道：先是横条，较早和较长的在上方，然后是按时间顺序排列的定时事件。能容纳多少车道取决于实际的行高；当一天的事件更多时，其最后一个车道显示 "N more"，并在面板中打开该天。延伸到下一周的横条会在下一行继续，两端为直角。

`Month`（其中任一日期）和 `SelectedDate` 是双向的，并会引发 `MonthChanged` 和 `SelectedDateChanged`。切换月份时，网格会以 12 px 的滑动方向交叉淡化。`FirstDayOfWeek`（默认为周一）和 `ShowWeekends` 也可以从账户菜单切换，而 `Today` 用于固定被视为今天的日期。

选择一天即可在面板中显示它，选择一个事件即可打开它，双击某天即可添加事件。拖动 5 px 即可拾起事件：一张卡片跟随指针，事件将覆盖的日期会高亮，松开即完成移动，并保持其时间和时长不变。拖过空白日期即可规划一段时间：表单会以这些日期作为全天事件打开。移动和删除会显示一个带 Undo 的提示，持续五秒。

### 日面板

面板显示所选的一天：标题、事件数量、已占用时间和空闲时间，以及从早上 7 点到晚上 10 点的时间条（每个事件一个色块；若是今天，还会标出当前时间）。下方是全天事件和日程安排，其中位于 `WorkDayStart` 与 `WorkDayEnd`（9 点到 6 点）之间、长度至少为 `MinimumFreeGap`（一小时）的空闲时段会被列出；点击 Block focus time 即可用一个 Focus time 事件填满其中一段。等待回复的邀请计入忙碌，但不计入已占用。

打开事件会显示其时间、时长、地点和视频链接（带复制按钮）、回复（针对邀请）、参与者及其回复、备注和日历选择器；垃圾桶按钮用于删除它。New event、N 键、面板底部的按钮以及双击都会打开表单：标题、全天、开始和结束、地点、视频链接、参与者和日历。保存时会引发 `EventCreating`，在其中设置 `ErrorMessage` 会让表单保持打开并显示该消息，之后再引发 `EventCreated`。

从 1080 像素宽起，面板停靠在月视图旁边，其左边缘可在 300 到 460 之间调整宽度（`DayPanelWidth`）；标题栏按钮可隐藏它（`IsDayPanelOpen`）。更窄时，它是一个在选择某天或某个事件时打开的 sheet。

### 筛选与视图

标题栏的筛选会淡化标题不匹配的事件，并统计匹配的数量。在侧边栏中，每个日历可切换其事件的显示，并显示其当月的数量；Only 只显示一个日历，All 则把其他日历重新显示出来。Upcoming 按天列出接下来四周的事件；Invitations 为每个邀请显示一张卡片，带有 Accept、Maybe、Decline 和 View in calendar；同事那一行则显示其月视图。`View` 和 `TeammateId` 跟随这些选择，也可以通过代码设置。

`Labels` 接受一个 `MonthCalendarLabels`，其中包含每个词语、名称和播报文字，以及日期格式模式，用于翻译或改写；`Culture` 用于格式化日期和时间。

## API 参考

<!-- api: MonthCalendar -->

<!-- api: MonthCalendarEvent -->

<!-- api: MonthCalendarSource -->

<!-- api: MonthCalendarPerson -->

<!-- api: MonthCalendarAttendee -->

<!-- api: MonthCalendarLabels -->

<!-- api: MonthCalendarEventEventArgs -->

<!-- api: MonthCalendarEventChangedEventArgs -->

<!-- api: MonthCalendarEventCreatingEventArgs -->

<!-- api: MonthCalendarRsvpChangedEventArgs -->

<!-- api: MonthCalendarDateChangedEventArgs -->

## 无障碍

- 月视图是由行和单元格组成的网格；每个日期按钮都以其日期、是否为今天以及事件数量命名，只有获得焦点的日期才是 Tab 停靠点。
- 事件色块是按钮，以标题、日期、时间和回复命名；"N more" 会说明它展开了多少个事件以及在哪一天。
- 侧边栏中的日历开关会读出其日历和数量；Only 和 All 两个胶囊按钮各有自己的名称。
- 日历不仅通过颜色区分，也通过图标和名称区分；邀请以描边显示，已拒绝的事件带删除线。
- 移动、删除、回复和新建事件会通过礼貌（polite）实时区域播报，并显示在提示中。

### 键盘

| 按键 | 操作 |
|---|---|
| Arrow keys | 在日期之间移动；离开网格时切换月份 |
| Enter or Space | 在面板中打开获得焦点的日期 |
| Page Up or Page Down | 上一月或下一月 |
| T | 跳到今天 |
| N | 在所选日期新建事件 |
| / | 聚焦事件筛选；Escape 清除它 |
| Alt + arrow keys | 将获得焦点的事件移动一天或一周 |
| Delete or Backspace | 删除获得焦点的事件，可在提示中点击 Undo 撤销 |
| Escape | 将面板从事件或表单返回到日期 |
| ? | 打开键盘快捷键 |
| Ctrl + K or Cmd + K | 搜索侧边栏 |

## 动效

| 内容 | 完整 | 减弱 | 关闭 |
|---|---|---|---|
| 切换月份 | 旧网格在 150 ms 内淡出；新网格在 180 ms 内淡入并滑动 12 px；标题滑动 6 px | 直接替换 | 直接替换 |
| 面板窗格 | 旧窗格在 150 ms 内淡出，新窗格在 200 ms 内淡入 | 直接替换 | 直接替换 |
| 停靠面板和侧边栏宽度 | 平滑弹簧（stiffness 300，damping 35）；面板在 150 ms 内淡入 | 直接跳到位 | 直接跳到位 |
| Sheet | 200 ms 内滑入，并伴随遮罩 | 200 ms 内滑入 | 立即出现 |
| 被删除和隐藏的事件 | 在原处 150 ms 内淡出 | 直接移除 | 直接移除 |
| 被移动的事件 | 在 200 ms 内于新日期淡入 | 直接出现 | 直接出现 |
| 筛选 | 不匹配的事件在 150 ms 内淡至 28% | 150 ms 内变淡 | 立即变淡 |
| 已回复的邀请卡片 | 在 135 ms 内淡入，同时其高度在平滑弹簧上收起 | 直接移除 | 直接移除 |
| 提示 | 上升 8 px 并在 200 ms 内淡入；150 ms 内离开 | 淡入淡出 | 立即出现 |

其他事件立即更换车道，与参考实现一致。

## 响应式

- 从 1080 px 起，日面板位于月视图旁边；低于此宽度时它是一个 sheet。
- 低于 900 px 时，侧边栏变为一个 sheet，从菜单按钮打开。
- 主卡片宽度为 760 px 或更小时，筛选移到标题栏下方。
- 当月份卡片的宽度小于 560 px 时，月视图显示日期数字和一个小型负载计量（每个事件一格，最多四格），所选日期的列表位于其下方，两者一起滚动。在手机上，标题栏保留月份、箭头、加号按钮和面板按钮。

## 相关

- `WeekCalendar`：在时间网格上显示一周。
- `Calendar` 和 `DatePicker`：选择日期。
- `GanttTimeline`：阶段与依赖关系。
