---
title: Month calendar
summary: 以月為單位的行事曆應用，事件色塊與橫條依車道排列並顯示「N more」，可停駐的日面板包含忙碌時間、可用於專注的空檔與事件詳情，支援拖曳改期或跨日規劃，以及近期安排、邀請和同事行事曆檢視。
source-hash: 19c45aece409
---

<Demo name="Preview" />

## 何時使用

`MonthCalendar` 是一個完整的行事曆畫面：側邊欄、月檢視格線和日面板。格線一眼就能看到整個月的所有事件；面板詳細顯示所選的一天，包括已排定了多少時間、可用於專注的空檔，以及開啟某個事件時的詳情。人們可以就地安排：把事件拖到另一天，拖過空白日期來規劃出差或發布週，回覆邀請，並透過提示列復原移動或刪除。

- **行事曆或排程應用的月檢視。** 載入行事曆、人員和事件，並透過本畫面引發的事件與它們保持同步。
- **團隊與內容行事曆。** 每個行事曆都有自己的顏色、圖示和側邊欄開關；同事的月檢視只需一次點選即可開啟。
- **涵蓋大量事件與多日時段的規劃。** 出差、研討會和發布週會以橫條顯示在其涵蓋的各天，當天自己的事件位於橫條下方。

## 何時不用

- **逐小時排程。** 請使用 `WeekCalendar`，它把事件排列在時間格線上。
- **在表單中選擇一個日期。** 請使用 `Calendar` 或 `DatePicker`。
- **有相依關係的專案階段。** 請使用 `GanttTimeline`。

## 用法

畫面會填滿其容器，因此要給它一個高度（視窗、頁面，或作為框架時使用 900）。新增行事曆、人員和事件，並指定目前登入的是誰：

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

### 事件、行事曆與人員

`MonthCalendarEvent` 包含 `Id`、`Title`、`CalendarId`、`StartDate` 以及可選的、包含端點的 `EndDate`。沒有 `StartTime` 時它是全天事件；有 `StartTime` 時它是定時事件，結束於 `EndTime`（未設定時為一小時後）。全天與多日事件以帶色調的橫條繪製；定時事件以色塊繪製，並用行事曆顏色顯示時間。事件可以帶有 `Location`、視訊會議（`VideoProvider` 與 `VideoUrl`）、`Notes`、`OrganizerId` 以及 `Attendees`，每位參與者都是一個 `MonthCalendarAttendee`，包含一位人員和一個回覆。

`Rsvp` 是目前登入者的回覆。`Pending` 表示邀請：其色塊為外框樣式，列在 Invitations 下，其詳情提供 Going、Maybe 和 No。已婉拒的事件標題會加上刪除線，且不再計入忙碌時間。

`MonthCalendarSource` 有 `Name`、`Tone`（`Teal`、`Violet`、`Amber`、`Blue`、`Rose`、`Green`、`Neutral`，或帶 `CustomColor` 的 `Custom`）以及可選的 `Icon`，可以是任意 24 × 24 線條圖示，例如 `ArcIcons` 中的圖示；預設為行事曆圖示。`IsShown` 是側邊欄的開關。人員是 `MonthCalendarPerson`；前五位帶有 `IsTeammate` 的人員會填入側邊欄的團隊行事曆，`CurrentUserId` 指定目前登入的人。

### 月檢視

格線顯示該月跨越的週，共五或六週，平分高度。每一天把事件排入車道：先是橫條，較早和較長的在上方，然後是依時間順序排列的定時事件。能容納多少車道取決於實際的列高；當一天的事件較多時，其最後一個車道會顯示 "N more"，並在面板中開啟該天。延伸到下一週的橫條會在下一列繼續，兩端為直角。

`Month`（其中任一日期）與 `SelectedDate` 是雙向的，並會引發 `MonthChanged` 和 `SelectedDateChanged`。切換月份時，格線會以 12 px 的滑動方向交叉淡化。`FirstDayOfWeek`（預設為週一）和 `ShowWeekends` 也可以從帳號選單切換，而 `Today` 用於固定被視為今天的日期。

選擇一天即可在面板中顯示它，選擇一個事件即可開啟它，雙擊某天即可新增事件。拖曳 5 px 即可拾起事件：一張卡片跟隨指標，事件將涵蓋的日期會亮起，放開即完成移動，並保留其時間與時長。拖過空白日期即可規劃一段期間：表單會以這些日期作為全天事件開啟。移動和刪除會顯示一則附 Undo 的提示，持續五秒。

### 日面板

面板顯示所選的一天：標題、事件數量、已排定時間與空閒時間，以及從早上 7 點到晚上 10 點的時間條（每個事件一個色塊；若是今天，還會標出目前時間）。下方是全天事件與日程安排，其中位於 `WorkDayStart` 與 `WorkDayEnd`（9 點到 6 點）之間、長度至少為 `MinimumFreeGap`（一小時）的空閒時段會被列出；點選 Block focus time 即可用一個 Focus time 事件填滿其中一段。等待回覆的邀請計入忙碌，但不計入已排定。

開啟事件會顯示其時間、時長、地點與視訊連結（附複製按鈕）、回覆（針對邀請）、參與者及其回覆、備註和行事曆選擇器；垃圾桶按鈕用於刪除它。New event、N 鍵、面板底部的按鈕以及雙擊都會開啟表單：標題、全天、開始與結束、地點、視訊連結、參與者和行事曆。儲存時會引發 `EventCreating`，在其中設定 `ErrorMessage` 會讓表單保持開啟並顯示該訊息，之後再引發 `EventCreated`。

從 1080 像素寬起，面板停駐在月檢視旁邊，其左邊緣可在 300 到 460 之間調整寬度（`DayPanelWidth`）；標題列按鈕可隱藏它（`IsDayPanelOpen`）。更窄時，它是一個在選擇某天或某個事件時開啟的 sheet。

### 篩選與檢視

標題列的篩選會淡化標題不符合的事件，並統計符合的數量。在側邊欄中，每個行事曆可切換其事件的顯示，並顯示其當月的數量；Only 只顯示一個行事曆，All 則把其他行事曆重新顯示出來。Upcoming 依日列出接下來四週的事件；Invitations 為每個邀請顯示一張卡片，附 Accept、Maybe、Decline 和 View in calendar；同事那一列則顯示其月檢視。`View` 與 `TeammateId` 會跟隨這些選擇，也可以透過程式碼設定。

`Labels` 接受一個 `MonthCalendarLabels`，其中包含每個詞語、名稱和播報文字，以及日期格式樣式，用於翻譯或改寫；`Culture` 用於格式化日期與時間。

## API 參考

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

## 協助工具

- 月檢視是由列和儲存格組成的格線；每個日期按鈕都以其日期、是否為今天以及事件數量命名，只有取得焦點的日期才是 Tab 停駐點。
- 事件色塊是按鈕，以標題、日期、時間和回覆命名；"N more" 會說明它展開了多少個事件以及在哪一天。
- 側邊欄中的行事曆開關會讀出其行事曆與數量；Only 和 All 兩個膠囊按鈕各有自己的名稱。
- 行事曆不只靠顏色區分，也靠圖示與名稱區分；邀請以外框顯示，已婉拒的事件帶刪除線。
- 移動、刪除、回覆和新增事件會透過禮貌（polite）即時區域播報，並顯示在提示中。

### 鍵盤

| 按鍵 | 操作 |
|---|---|
| Arrow keys | 在日期之間移動；離開格線時切換月份 |
| Enter or Space | 在面板中開啟取得焦點的日期 |
| Page Up or Page Down | 上一個月或下一個月 |
| T | 跳到今天 |
| N | 在所選日期新增事件 |
| / | 聚焦事件篩選；Escape 清除它 |
| Alt + arrow keys | 將取得焦點的事件移動一天或一週 |
| Delete or Backspace | 刪除取得焦點的事件，可在提示中點選 Undo 復原 |
| Escape | 將面板從事件或表單返回到日期 |
| ? | 開啟鍵盤快捷鍵 |
| Ctrl + K or Cmd + K | 搜尋側邊欄 |

## 動態效果

| 內容 | 完整 | 減弱 | 關閉 |
|---|---|---|---|
| 切換月份 | 舊格線在 150 ms 內淡出；新格線在 180 ms 內淡入並滑動 12 px；標題滑動 6 px | 直接替換 | 直接替換 |
| 面板窗格 | 舊窗格在 150 ms 內淡出，新窗格在 200 ms 內淡入 | 直接替換 | 直接替換 |
| 停駐面板與側邊欄寬度 | 平滑彈簧（stiffness 300，damping 35）；面板在 150 ms 內淡入 | 直接跳到位 | 直接跳到位 |
| Sheet | 200 ms 內滑入，並伴隨遮罩 | 200 ms 內滑入 | 立即出現 |
| 被刪除和隱藏的事件 | 在原處 150 ms 內淡出 | 直接移除 | 直接移除 |
| 被移動的事件 | 在 200 ms 內於新日期淡入 | 直接出現 | 直接出現 |
| 篩選 | 不符合的事件在 150 ms 內淡至 28% | 150 ms 內變淡 | 立即變淡 |
| 已回覆的邀請卡片 | 在 135 ms 內淡入，同時其高度在平滑彈簧上收起 | 直接移除 | 直接移除 |
| 提示 | 上升 8 px 並在 200 ms 內淡入；150 ms 內離開 | 淡入淡出 | 立即出現 |

其他事件會立即更換車道，與參考實作一致。

## 響應式

- 從 1080 px 起，日面板位於月檢視旁邊；低於此寬度時它是一個 sheet。
- 低於 900 px 時，側邊欄會變為一個 sheet，從選單按鈕開啟。
- 主卡片寬度為 760 px 或更小時，篩選移到標題列下方。
- 當月份卡片的寬度小於 560 px 時，月檢視會顯示日期數字和一個小型負載計量（每個事件一格，最多四格），所選日期的清單位於其下方，兩者一起捲動。在手機上，標題列保留月份、箭頭、加號按鈕和面板按鈕。

## 相關

- `WeekCalendar`：在時間格線上顯示一週。
- `Calendar` 與 `DatePicker`：選擇日期。
- `GanttTimeline`：階段與相依關係。
