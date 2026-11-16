---
title: 移動端文字方塊
summary: 44 px 高的填充文字方塊，可帶圖示和清除按鈕，取得焦點時邊框變為強調色。
source-hash: cee92d846535
---

<Demo name="Basic" />

## 何時使用

`TextField` 是移動端表單裡的單行文字輸入：姓名、信箱、簡短的回答。

- **手機上的表單**：文字方塊高 44 px，拇指容易點中。
- **有含義的欄位**：文字前的 `Icon` 說明這個欄位是做什麼的。
- **常需要修正的欄位**：清除按鈕一次點按就能清空。

即時範例在手機外框裡執行，那是桌面上的模擬；真機上的軟鍵盤這裡沒有驗證。

## 何時不用

- **搜尋**：用 `Searchbar`，它有搜尋圖示、膠囊形狀和搜尋鍵。
- **很長的多行文字**：應該用文字區域，這個控制項只有一行。
- **從幾個值裡選一個**：用開關、分段控制項或帶清單的面板。

## 用法

`TextField` 衍生自 Avalonia 的 `TextBox`，所以 `Text`、選取、`IsReadOnly` 和繫結都和那裡一樣。用 `PlaceholderText` 顯示提示。

```xml
<arcm:TextField xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                PlaceholderText="姓名"
                Icon="{x:Static arc:ArcIcons.User}"
                Text="{Binding Name}" />
```

清除按鈕只在文字方塊有焦點且有文字時顯示；唯讀或 `ShowClearButton` 為 false 時不顯示。點它會清空文字並保持焦點。文字方塊沒有懸停狀態：手指不會懸停。

### 長按選單

長按文字方塊會在它上方開啟 `TextEditMenu`：剪下、拷貝、貼上和全選，和手機上一樣。選單只顯示能用的操作：剪下和拷貝需要選取範圍，所以密碼欄位（設定了 `PasswordChar`）從不提供這兩項；唯讀的文字方塊沒有剪下和貼上。全部選取後不再顯示全選，點全選之後選單會為新的選取範圍再次出現。拖曳選取控點時，選單出現在選取範圍上方。選單不取得焦點，所以螢幕鍵盤保持開啟；點外面、輸入文字或焦點移走時選單關閉。選單文字跟隨應用程式語言（en-US、zh-CN、zh-TW）。

主題把這個選單設為文字方塊的 `ContextFlyout`，`Searchbar` 也有。在文字方塊上設定自己的 `ContextFlyout` 或 `ContextMenu` 可以取代它；把 `<arcm:TextEditMenu />` 設為其他 `TextBox` 的 `ContextFlyout`，就能讓它也有同樣的選單。

在 iOS 上，加入 `TKCode.Arc.Mobile.iOS` 套件，並讓 App 的 delegate 衍生自 `ArcAppDelegate<App>`。這樣長按會開啟系統編輯選單（iOS 16 以上），鍵盤上的「從…貼上」建議能貼到文字方塊裡，貼上時也不會跳出「允許貼上」的確認框。不加這個套件時，iOS 上和其他平台一樣顯示 Arc 選單。

```csharp
[Register("AppDelegate")]
public partial class AppDelegate : ArcAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseArc(arc => arc.UseArcMobile());
}
```

### Token

`TextFieldToken` 保存幾何：`FieldHeight`（44）、`ContentPadding`（兩側 12）、`BorderWidth`（1）、`GlyphExtent`（18）、`IconMargin` 和 `DisabledOpacity`（0.5）。`TextEditMenuToken` 保存選單的 `ItemHeight`（44）、`ItemPadding`（兩側 14）和 `AnchorGap`（選單與文字方塊之間的 8）。顏色、圓角和字型來自 ArcDesign token。

## API 參考

<!-- api: MobileTextField -->

## 無障礙

給每個欄位一個看得見的名稱，放在旁邊，或者用圖示加預留位置提示；人開始輸入後，預留位置提示就不再是唯一的標籤。文字方塊至少 44 px 高，清除按鈕是 44 px 的觸控目標。

## 動效

文字方塊取得或失去焦點時，邊框按快速曲線變色。清除按鈕從 0.6 縮放加淡入彈出（P26，透過 `PresenceHost`），離開比出現更快。減弱動效時，邊框立即變色，清除按鈕也立即出現和消失。

## 響應式

- `TextField` 沒有斷點。它佔滿所給的寬度，高度至少 44 px（`FieldHeight`），因此讓它延展，不要設定固定的 `Width`。
- 文字不換行：`TextWrapping` 為 `NoWrap`，水平捲軸隱藏，過長的值會在輸入框內水平捲動。
- 清除按鈕（`ShowClearButton`）只在輸入框有焦點且有文字時出現，它是 44 px 的觸控目標。
- 為每個輸入框在旁邊放置可見的名稱，因為 `PlaceholderText` 在使用者開始輸入後就會消失。

## 相關元件

- `MobileSearchbar`：為搜尋準備的文字方塊。
- `MobilePage`：表單所在的螢幕。
- `MobileButton`：提交表單的常用方式。
