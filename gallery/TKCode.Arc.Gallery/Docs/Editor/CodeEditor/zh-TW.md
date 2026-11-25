---
title: 程式碼編輯器
summary: "像在 Monaco 中一樣編輯原始碼：89 種語言的語法標示、尋找取代、程式碼摺疊、多游標、minimap、來自語言伺服器的補全與暫留提示、Monaco 的快捷鍵、復原與重做。"
source-hash: 3879e8cd8bb3
---

<Demo name="Preview" />

點進編輯器即可輸入。方向鍵、Home、End 與 Page Up / Page Down 移動游標，同時按 Shift 則選取；Tab 與 Shift+Tab 增加與減少縮排；Ctrl+Z（macOS 上為 ⌘Z）復原。

## 何時使用

- 使用者在應用程式中撰寫或修改的原始碼、設定檔、查詢與指令碼。
- Tab、縮排與行號都有意義、以等寬字型顯示的文字。
- 很長的文件：文字存放在 piece tree 中，數 MB 的檔案也能快速開啟與編輯，而且只繪製可見的行。

## 何時不用

- 只供展示的程式碼：使用 `CodeBlock`，它會標示語法並附有複製按鈕。
- 表單中的一般文字：使用 `Input` 或 `Textarea`。
- 含標題、清單與連結的格式化文字：使用 `RichTextEditor`。

## 用法

`CodeEditor` 位於 `TKCode.Arc.Editor.Controls` 套件中，這個套件會帶入編輯器引擎 `TKCode.Arc.Editor`。在 `UseArc(...)` 中註冊它：

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

`Text` 保存文件，支援雙向繫結。設定它會取代整份文件並清除復原紀錄；使用者的輸入會更新它，兩種情況都會引發 `TextChanged`。

```xml
<arc:CodeEditor Height="320"
                Text="{Binding Source}"
                TabSize="2" />
```

```csharp
Editor.TextChanged += (_, _) => Save(Editor.Text);
```

引擎移植自 Monaco 編輯器（Visual Studio Code 的編輯器），因此輸入的行為與那裡相同：括號與引號會在語言允許之處自動閉合，Enter 會保留目前行的縮排，輸入一個自動插入的右括號時會直接跳過它。`LanguageId` 指定套用哪種語言的設定；新建立的編輯器是 `plaintext`。

支援輸入法：組字中的文字以底線顯示在游標處，確認後才寫入文件。

### 語言

`LanguageId` 可以是 Monaco 編輯器內建的 89 種語言中的任何一種，用 id 或名稱皆可：`csharp` 或 `C#`、`typescript`、`javascript`、`python`、`json`、`html`、`css`、`sql`、`yaml`、`markdown`、`rust`、`go`、`java`、`cpp` 等。它決定語法標示，以及該語言的括號、註解、自動閉合配對與縮排規則。顏色採用 Monaco 的淺色或深色主題，跟隨應用程式的明暗外觀，其餘文字使用編輯器本身的文字顏色。畫面上的行會立即著色，長檔案的其餘部分在應用程式閒置時完成。括號依巢狀深度著色，與 Visual Studio Code 相同；字串與註解中的括號不計，沒有對應左括號的右括號顯示為紅色。

```xml
<arc:CodeEditor LanguageId="typescript" Text="{Binding Source}" />
```

括號依巢狀深度著色，游標處括號的配對括號會加上外框。

### 語言伺服器

補全、暫留提示、問題、移至定義等語言功能來自語言伺服器。`TKCode.Arc.Editor.Lsp` 套件提供了一個 Language Server Protocol 用戶端：啟動伺服器，把它的輸出串流與輸入串流交給用戶端，再用伺服器認得該檔案的 URI 連接編輯器。

```csharp
var process = Process.Start(new ProcessStartInfo("typescript-language-server", "--stdio")
{
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
});
var client = new LanguageClient(new LanguageClientOptions { RootUri = new Uri("file:///work/") });
await client.StartAsync(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
var attachment = Editor.AttachLanguageClient(client, new Uri("file:///work/main.ts"));
```

之後，編輯器會在輸入時（或按 Ctrl+Space 時）顯示建議，暫留時顯示文件與問題，在錯誤與警告下畫波浪線，在呼叫中顯示參數提示，為程式碼動作顯示燈泡，顯示 inlay hint 與 code lens，並使用伺服器提供的語意顏色——每項都取決於伺服器是否提供。沒有伺服器時，建議來自文件中的單字。處置 `attachment` 即可中斷連接。

### 程式碼片段

`InsertSnippet` 在每個游標處插入一段 Visual Studio Code 語法的程式碼片段：`$1` 與 `${2:default}` 是定位停駐點，`${1|one,two|}` 是選項，`$TM_SELECTED_TEXT` 或 `$CLIPBOARD` 是變數。Tab 與 Shift+Tab 在定位停駐點之間移動，Escape 離開。

```csharp
Editor.InsertSnippet("for (var ${1:i} = 0; $1 < ${2:count}; $1++)\n{\n\t$0\n}");
```

### 選項

<Demo name="Options" />

- `TabSize` 是一個 Tab 的寬度（以空格計，預設 4），也是一級縮排的大小；`InsertSpaces` 讓 Tab 插入空格而不是 Tab 字元（預設開啟）。
- `WordWrap` 讓長行在編輯器寬度處換行；換出來的部分沒有行號。
- `ShowLineNumbers` 在左側顯示行號（預設開啟）。
- `ShowIndentGuides` 在每一級縮排處畫一條細線，游標所在程式碼區塊的那一條更深，對應 Monaco 的 `guides.indentation` 選項（預設開啟）。
- `SmoothScrolling` 讓滾輪捲動與捲動到游標處帶有動畫，對應 Monaco 的 `smoothScrolling` 選項（預設關閉）。
- `ShowMinimap` 在右側顯示文件的縮圖；按一下它或拖曳其滑桿即可捲動（預設開啟）。旁邊的概觀尺規標出整份文件中的游標、問題與搜尋結果。
- `Folding` 允許用邊欄中的摺疊箭頭與摺疊命令摺疊程式碼區域（預設開啟）。
- `StickyScroll` 在捲動經過類別、函式與程式碼區塊時，把它們的起始行固定在頂端，最多 `StickyScrollMaxLineCount` 行（預設開啟）。
- `ColorDecorators` 在 CSS 顏色前放一個色塊；按一下會開啟色彩選擇器，並把選取的顏色寫回（預設開啟）。
- `FormatOnType` 與 `FormatOnPaste` 在輸入或貼上時格式化程式碼，前提是語言有格式化工具（預設關閉）。`LinkedEditing` 讓連結的範圍一起變更，例如 HTML 標籤的開始與結束名稱（預設關閉）。
- `TabFocusMode` 讓 Tab 與 Shift+Tab 把焦點移出編輯器，而不是縮排；Ctrl+M 切換它（預設關閉）。
- `IsReadOnly` 讓文字不能修改；游標仍可移動，文字仍可選取與複製。

### 鍵盤

編輯器使用 Monaco 的快捷鍵；在 macOS 上 Ctrl 對應 ⌘。

| 按鍵 | 作用 |
|---|---|
| 方向鍵、Home、End、Page Up、Page Down | 移動游標 |
| 以上任一鍵加 Shift | 選取 |
| Ctrl+Home、Ctrl+End | 跳到文件開頭或結尾 |
| Ctrl+A | 全選 |
| Ctrl+Shift+Alt 加方向鍵 | 欄選取：每行一個游標 |
| Escape | 取消選取，再按一次回到單一游標 |
| Tab、Shift+Tab | 增加縮排、減少縮排 |
| Ctrl+Z，Ctrl+Y 或 Ctrl+Shift+Z | 復原、重做 |
| Ctrl+C、Ctrl+X、Ctrl+V | 複製、剪下、貼上；沒有選取範圍時，複製與剪下作用於整行 |
| Ctrl+F、Ctrl+H | 尋找、尋找並取代；F3 與 Shift+F3 移至下一個與上一個相符項目 |
| Ctrl+D、Ctrl+Shift+L | 加入選取範圍的下一個出現處、選取所有出現處 |
| Ctrl+Alt+Up、Ctrl+Alt+Down | 在上方或下方加入游標 |
| Alt+Up、Alt+Down、Shift+Alt+Up、Shift+Alt+Down | 上移或下移行，向上或向下複製行 |
| Ctrl+Shift+K、Ctrl+Enter、Ctrl+Shift+Enter | 刪除行，在下方或上方插入一行 |
| Ctrl+/、Shift+Alt+A | 切換行註解、區塊註解 |
| Ctrl+Shift+\\ | 移至相符的括號 |
| Ctrl+Shift+[、Ctrl+Shift+] | 摺疊、展開；Ctrl+K Ctrl+0 全部摺疊，Ctrl+K Ctrl+J 全部展開 |
| Shift+Alt+Right、Shift+Alt+Left | 擴大、縮小選取範圍 |
| Ctrl+Space、Ctrl+Shift+Space | 顯示建議、顯示參數提示 |
| F12、Shift+F12、Alt+F12 | 移至定義、移至參考、瞄核定義 |
| F2、Ctrl+. | 重新命名符號、顯示程式碼動作 |
| F8、Shift+F8 | 移至下一個或上一個問題 |
| Shift+Alt+F | 格式化文件 |
| Ctrl+M | 切換 Tab 是否移動焦點 |

使用滑鼠時，按一下放置游標，拖曳或 Shift+按一下選取，按兩下選取一個字，按三下選取一行；按兩下或按三下後繼續拖曳，可依字或依行延伸選取範圍。Alt+按一下新增游標（再按一次 Alt+按一下可移除），Alt+Shift+拖曳或以中鍵拖曳進行欄選取，按一下或拖曳行號選取整行。拖曳超出邊緣時會自動捲動。拖曳選取的文字可以移動它（按住 Ctrl，macOS 上按住 Option，則為複製）；Ctrl+按一下連結可開啟它，`LinkOpening` 事件讓應用程式自行開啟連結。

## API 參考

<!-- api: CodeEditor -->

## 無障礙

- 以 `AutomationProperties.Name` 為編輯器命名（如範例所示），或在旁邊放一個標籤。
- Tab 是編輯器自己的按鍵：它用來縮排，不會把焦點移出去。Ctrl+M（macOS 上為 Ctrl+Shift+M）會開啟 `TabFocusMode`，此時 Tab 移動焦點；請告訴鍵盤使用者這一點，或在編輯器只是表單中的一個欄位時設定 `TabFocusMode`。
- 螢幕閱讀器看到的是一個編輯控制項，其值就是文件。游標移動時，編輯器會讀出目前行、字元或選取範圍；Ctrl+Alt+Shift+G 讀出游標所在的行與欄。

## 動態效果

- 編輯器有焦點時游標閃爍；每次按鍵後游標會先停止閃爍一下，確保輸入時看得見它。尋找方塊開啟時會滑入；除此之外沒有其他動畫。

## 響應式行為

- 編輯器填滿給它的寬度，最小高度為 80。請自行設定高度：內容在編輯器內捲動，編輯器不會隨內容變高。
- 開啟 `WordWrap` 時，編輯器寬度改變後會重新換行。
- 在觸控螢幕上，輕觸放置游標，點兩下或長按選取一個字並顯示可拖曳的控點，手指滑動可帶慣性地捲動。
- 編輯器較窄時，關閉 `ShowMinimap` 把空間留給文字。

## 相關

- `CodeBlock`：有語法標示、只展示不編輯的程式碼。
- `Textarea`：表單中的多行純文字。
- `RichTextEditor`：格式化文字。
- `DiffEditor`：比較一段文字的兩個版本。
