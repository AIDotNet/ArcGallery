---
title: 移动端列表
summary: 分组的行，用于设置和菜单：图标、标题、副标题、行尾的值或控件，以及箭头。
source-hash: 5f0bbe05de52
---

<Demo name="Basic" />

## 何时使用

`ListGroup` 和 `ListItem` 排出一屏的行：设置、菜单和账户页，每一行都是可以点按的标题。

- **设置**：把相关的行放在一个简短的 `Header` 下，用 `Footer` 作说明。
- **目的地菜单**：带 `Icon` 和箭头、会打开其他页面的行。
- **行尾的值或开关**：放进 `Trailing`。
- **行首的人或账户**：把头像放进 `Leading`。
- **不是操作的内容**：二维码、一段文字或自带的按钮放进 `ListCell`，这种行不响应按压。

实时示例在手机外框里运行，那是桌面上的模拟；真机上的触摸反馈这里没有验证。

## 何时不用

- **很长或无限的数据**：应该用虚拟化列表，这个还没有做。
- **带图片和多个操作的卡片**：自己排版。
- **桌面菜单**：用桌面的菜单控件；这里的行没有悬停状态。

## 用法

把 `ListItem` 放进 `ListGroup`，再把组放进可滚动的 `Page`。项的内容就是它的标题。`ListItem` 是一个按钮，给它 `Command` 或处理 `Click`；手指按在行上时，行会变暗。

```xml
<arcm:Page xmlns="https://github.com/avaloniaui"
           xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
           xmlns:arc="https://github.com/AIDotNet/TKCode.Arc"
           xmlns:arcm="https://github.com/AIDotNet/TKCode.Arc.Mobile"
           IsScrollable="True">
    <arcm:ListGroup Header="账户">
        <arcm:ListItem Icon="{x:Static arc:ArcIcons.User}" Content="个人资料" Subtitle="Ada Lovelace" ShowChevron="True" />
        <arcm:ListItem Content="每周摘要" Trailing="周日" ShowChevron="True" />
    </arcm:ListGroup>
</arcm:Page>
```

一组的最后一行下面没有分隔线。

### 内容单元格

`ListCell` 是放任意内容、不响应按压的行：没有按压变暗，没有悬停，本身也不获得焦点（里面的内容可以）。它的外框和 `ListItem` 相同：同样 48 px 的最小高度、两侧内边距和分隔线，所以单元格和行项可以混在同一组里，组里最后一行（不论是单元格还是行项）下面没有分隔线。用它放二维码、一段文字、自带的按钮，或者在行之间放一个 `MobileRadioCards` 网格。

<Demo name="Cells" />

```xml
<arcm:ListGroup Header="邀请码">
    <arcm:ListCell HorizontalContentAlignment="Center">
        <Image Source="{Binding QrCode}" Width="{arc:ArcDesignTokenResource MediaSizeMD}" />
    </arcm:ListCell>
    <arcm:ListCell>
        <arcm:Button Content="复制邀请码" HorizontalAlignment="Left" />
    </arcm:ListCell>
    <arcm:ListItem Content="通过邮件邀请" ShowChevron="True" />
</arcm:ListGroup>
```

直接写在组里、或作为 `ItemTemplate` 根元素返回的 `ListCell` 就是这一行本身，不会再被包进 `ListItem`。

### 前置内容

`Leading` 接受标题前的任意内容，例如头像。它和 `Icon` 共用第一列（两者都设置时取代图标），所以标题和 `Subtitle` 一起留在它旁边。前置内容保持自己的尺寸，行会长高以容纳它。

<Demo name="Leading" />

```xml
<arcm:ListItem Content="Ada Lovelace" Subtitle="账户、设备与登录" ShowChevron="True">
    <arcm:ListItem.Leading>
        <Border Width="{arcm:ListItemTokenResource AvatarSizeXL}"
                Height="{arcm:ListItemTokenResource AvatarSizeXL}"
                CornerRadius="{arc:ArcDesignTokenResource RadiusPill}"
                Background="{arc:ArcDesignTokenResource AccentSubtle}" />
    </arcm:ListItem.Leading>
</arcm:ListItem>
```

头像尺寸是 `ListItemToken` 的 token，既用于前置插槽，也用于页面上其他位置的个人资料行和页头：

| Token | 尺寸 | 用途 |
|---|---|---|
| `AvatarSizeSM` | 32 | 紧凑的行、评论 |
| `AvatarSizeMD` | 40 | 联系人或账户行 |
| `AvatarSizeLG` | 48 | 以人为主的行 |
| `AvatarSizeXL` | 64 | 设置页顶部的个人资料行 |
| `AvatarSize2XL` | 88 | 个人资料页头 |

### Token

`ListItemToken` 保存行的几何：`RowHeight`（48）、`ContentPadding`（16 乘 10）、`IconExtent`（22）、`IconGap`（12）、`TrailingMargin`、`ChevronExtent`（16）、`SeparatorInset`（16）、`LeadingMargin`（图标或前置内容之后的 12）以及上面的头像尺寸。`ListCellToken` 保存单元格的几何：`RowHeight`（48）、`ContentPadding`（16 乘 12）和 `SeparatorInset`（16）。`ListGroupToken` 保存组的几何：`GroupPadding`（16 乘 8）、`CardBorderThickness`（1）、`HeaderMargin` 和 `FooterMargin`。颜色、圆角和字体来自 ArcDesign token。

## API 参考

<!-- api: MobileListGroup -->

<!-- api: MobileListItem -->

<!-- api: MobileListCell -->

## 无障碍

一行是一个按钮，名称来自标题和副标题。每一行都要有标题，图标或头像不能是它唯一的名称。每一行至少 48 px 高，高于 44 px 的触摸目标。`ListCell` 不是按钮，也不获得焦点；它里面的控件（按钮、单选卡片）各自获得焦点、各自命名。

## 动效

手指按在行上时，背景按快速曲线变暗；减弱动效时，立即变暗。`ListCell` 没有自己的动效：它不响应按压。

## 响应式

- `ListGroup` 没有断点。它是一张占满宽度的卡片，周围有 `GroupPadding`（16 x 8 px），`Header` 与 `Footer` 换行（`TextWrapping="Wrap"`），而不会被截断。
- 每个 `ListItem` 行至少 48 px 高，副标题以省略号（`CharacterEllipsis`）截成一行。
- 行末的内容占用标题之外剩下的空间，所以过长的行末值会把标题挤短。请保持简短。
- 前置内容保持自己的尺寸，行会长高以容纳它；40 px 的头像让行变成 60 px。`ListCell` 随内容长高，所以其中的长文字应换行，而不是一直延伸。
- 手指按下时以淡色底面标示该行，没有任何行依赖悬停。点按该行即可对它执行操作。

## 相关组件

- `MobilePage`：可滚动的页面承载这些组。
- `MobileSwitch`：设置项常用的 `Trailing` 控件。
- `MobileNavbar`：顶栏，根设置页可用大标题。
- `MobilePicker`：显示当前值、在操作表里打开选项的行。
- `MobileRadioCards`：可以放进 `ListCell` 的卡片网格。
