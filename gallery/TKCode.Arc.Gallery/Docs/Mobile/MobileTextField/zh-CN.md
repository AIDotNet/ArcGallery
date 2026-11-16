---
title: 移动端文本框
summary: 44 px 高的填充文本框，可带图标和清除按钮，获得焦点时边框变为强调色。
source-hash: cee92d846535
---

<Demo name="Basic" />

## 何时使用

`TextField` 是移动端表单里的单行文本输入：姓名、邮箱、简短的回答。

- **手机上的表单**：文本框高 44 px，拇指容易点中。
- **有含义的字段**：文字前的 `Icon` 说明这个字段是做什么的。
- **常需要改正的字段**：清除按钮一次点按就能清空。

实时示例在手机外框里运行，那是桌面上的模拟；真机上的软键盘这里没有验证。

## 何时不用

- **搜索**：用 `Searchbar`，它有搜索图标、胶囊形状和搜索键。
- **很长的多行文字**：应该用文本区域，这个控件只有一行。
- **从几个值里选一个**：用开关、分段控件或带列表的面板。

## 用法

`TextField` 派生自 Avalonia 的 `TextBox`，所以 `Text`、选区、`IsReadOnly` 和绑定都和那里一样。用 `PlaceholderText` 显示提示。

```xml
<arcm:TextField xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
                xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
                PlaceholderText="姓名"
                Icon="{x:Static arc:ArcIcons.User}"
                Text="{Binding Name}" />
```

清除按钮只在文本框有焦点且有文字时显示；只读或 `ShowClearButton` 为 false 时不显示。点它会清空文本并保持焦点。文本框没有悬停状态：手指不会悬停。

### 长按菜单

长按文本框会在它上方打开 `TextEditMenu`：剪切、复制、粘贴和全选，和手机上一样。菜单只显示能用的操作：剪切和复制需要选区，所以密码框（设置了 `PasswordChar`）从不提供这两项；只读的文本框没有剪切和粘贴。全部选中后不再显示全选，点全选之后菜单会为新的选区再次出现。拖动选区手柄时，菜单出现在选区上方。菜单不获取焦点，所以软键盘保持打开；点外面、输入文字或焦点移走时菜单关闭。菜单文字跟随应用语言（en-US、zh-CN、zh-TW）。

主题把这个菜单设为文本框的 `ContextFlyout`，`Searchbar` 也有。在文本框上设置自己的 `ContextFlyout` 或 `ContextMenu` 可以替换它；把 `<arcm:TextEditMenu />` 设为其他 `TextBox` 的 `ContextFlyout`，就能让它也有同样的菜单。

在 iOS 上，添加 `TKCode.Arc.Mobile.iOS` 包，并让 App 的 delegate 派生自 `ArcAppDelegate<App>`。这样长按会打开系统编辑菜单（iOS 16 及以上），键盘上的“粘贴自…”建议能粘贴到文本框里，粘贴时也不会弹出“允许粘贴”的确认框。不加这个包时，iOS 上和其他平台一样显示 Arc 菜单。

```csharp
[Register("AppDelegate")]
public partial class AppDelegate : ArcAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseArc(arc => arc.UseArcMobile());
}
```

### Token

`TextFieldToken` 保存几何：`FieldHeight`（44）、`ContentPadding`（两侧 12）、`BorderWidth`（1）、`GlyphExtent`（18）、`IconMargin` 和 `DisabledOpacity`（0.5）。`TextEditMenuToken` 保存菜单的 `ItemHeight`（44）、`ItemPadding`（两侧 14）和 `AnchorGap`（菜单与文本框之间的 8）。颜色、圆角和字体来自 ArcDesign token。

## API 参考

<!-- api: MobileTextField -->

## 无障碍

给每个字段一个看得见的名称，放在旁边，或者用图标加占位提示；人开始输入后，占位提示就不再是唯一的标签。文本框至少 44 px 高，清除按钮是 44 px 的触摸目标。

## 动效

文本框获得或失去焦点时，边框按快速曲线变色。清除按钮从 0.6 缩放加淡入弹出（P26，通过 `PresenceHost`），离开比出现更快。减弱动效时，边框立即变色，清除按钮也立即出现和消失。

## 响应式

- `TextField` 没有断点。它占满所给的宽度，高度至少 44 px（`FieldHeight`），因此让它拉伸，不要设置固定的 `Width`。
- 文字不换行：`TextWrapping` 为 `NoWrap`，横向滚动条隐藏，过长的值会在输入框内横向滚动。
- 清除按钮（`ShowClearButton`）仅在输入框获得焦点且有文字时出现，它是 44 px 的触控目标。
- 为每个输入框在旁边放置可见的名称，因为 `PlaceholderText` 在用户开始输入后就会消失。

## 相关组件

- `MobileSearchbar`：为搜索准备的文本框。
- `MobilePage`：表单所在的屏幕。
- `MobileButton`：提交表单的常用方式。
