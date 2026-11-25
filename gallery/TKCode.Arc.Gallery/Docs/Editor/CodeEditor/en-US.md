---
title: Code editor
summary: "Edit source code like in Monaco: syntax highlighting for 89 languages, find and replace, folding, multiple cursors, a minimap, completion and hovers from a language server, Monaco's keybindings, undo and redo."
---

<Demo name="Preview" />

Click into the editor and type. The arrow keys, Home, End and Page Up / Page Down move the cursor, Shift with them selects, Tab and Shift+Tab indent and outdent, and Ctrl+Z (⌘Z on macOS) undoes.

## When to use

- Source code, configuration files, queries and scripts that people write or change in the app.
- Text where tabs, indentation and line numbers matter, in a monospaced font.
- Long documents: the text lives in a piece tree, so a file of many megabytes opens and edits quickly, and only the visible lines are drawn.

## When not to use

- Code that is only shown: use `CodeBlock`, which highlights it and has a copy button.
- Ordinary text in a form: use `Input` or `Textarea`.
- Formatted text with headings, lists and links: use `RichTextEditor`.

## Usage

`CodeEditor` is in the `TKCode.Arc.Editor.Controls` package, which brings the editor engine `TKCode.Arc.Editor` with it. Register the package inside `UseArc(...)`:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcCodeEditor());
```

`Text` holds the document and binds both ways. Setting it replaces the document and clears the undo history; what the user types updates it, and `TextChanged` is raised for both.

```xml
<arc:CodeEditor Height="320"
                Text="{Binding Source}"
                TabSize="2" />
```

```csharp
Editor.TextChanged += (_, _) => Save(Editor.Text);
```

The engine is a port of the Monaco editor (the editor of Visual Studio Code), so typing behaves as it does there: brackets and quotes close themselves where the language says so, Enter keeps the indentation of the line, and typing over an auto-inserted closing bracket steps over it. `LanguageId` names the language whose configuration applies; a new editor is `plaintext`.

Input methods work: the text being composed shows at the cursor, underlined, and goes into the document when it is committed.

### Languages

`LanguageId` takes any of the 89 languages the Monaco editor comes with, by id or by name: `csharp` or `C#`, `typescript`, `javascript`, `python`, `json`, `html`, `css`, `sql`, `yaml`, `markdown`, `rust`, `go`, `java`, `cpp` and the rest. It sets the syntax highlighting and the language's brackets, comments, auto-closing pairs and indentation rules. The colors are Monaco's light or dark theme, following the app's appearance, with the editor's own text color for everything else. The lines on screen are colored at once and the rest of a long file while the app is idle. Brackets take a color by how deeply they are nested, as in Visual Studio Code; brackets in strings and comments do not count, and a closing bracket without an opening one is red.

```xml
<arc:CodeEditor LanguageId="typescript" Text="{Binding Source}" />
```

Brackets take a color by how deeply they are nested, and the bracket matching the one at the cursor is outlined.

### Language servers

Completion, hovers, problems, go to definition and the other language features come from a language server. The `TKCode.Arc.Editor.Lsp` package has a client for the Language Server Protocol: start the server, give the client its output and input streams, and attach the editor with the URI the server knows the file by.

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

The editor then shows suggestions while you type (or on Ctrl+Space), hovers with documentation and problems, squiggles under errors and warnings, parameter hints in calls, a lightbulb for code actions, inlay hints and code lenses, and the server's semantic colors, each when the server offers it. Without a server, suggestions come from the words of the document. Dispose the attachment to detach.

### Snippets

`InsertSnippet` inserts a snippet in Visual Studio Code's syntax at every cursor: `$1` and `${2:default}` are tab stops, `${1|one,two|}` a choice and `$TM_SELECTED_TEXT` or `$CLIPBOARD` variables. Tab and Shift+Tab move between the tab stops and Escape leaves them.

```csharp
Editor.InsertSnippet("for (var ${1:i} = 0; $1 < ${2:count}; $1++)\n{\n\t$0\n}");
```

### Options

<Demo name="Options" />

- `TabSize` is the width of a tab in spaces (4) and the size of one indentation level, and `InsertSpaces` makes Tab insert spaces instead of a tab character (on).
- `WordWrap` wraps long lines at the width of the editor; wrapped parts have no line number.
- `ShowLineNumbers` shows the line numbers on the left (on).
- `ShowIndentGuides` draws a thin line at each indentation level, stronger for the block the cursor is in, as Monaco's `guides.indentation` option (on).
- `SmoothScrolling` animates scrolling with the wheel and to the cursor, as Monaco's `smoothScrolling` option (off).
- `ShowMinimap` shows a zoomed-out picture of the document at the right edge; click it or drag its slider to scroll (on). Beside it, the overview ruler marks the cursor, problems and search results of the whole document.
- `Folding` lets code regions fold with the chevrons in the gutter and the folding commands (on).
- `StickyScroll` keeps the lines that open the enclosing classes, functions and blocks pinned at the top while you scroll through them, up to `StickyScrollMaxLineCount` lines (on).
- `ColorDecorators` puts a swatch before CSS colors; clicking it opens a color picker that writes the color back (on).
- `FormatOnType` and `FormatOnPaste` format as you type or paste, when the language has a formatter (off). `LinkedEditing` changes linked ranges together, such as an HTML tag's start and end names (off).
- `TabFocusMode` makes Tab and Shift+Tab move the focus out of the editor instead of indenting; Ctrl+M toggles it (off).
- `IsReadOnly` keeps the text from changing; the cursor still moves and text can still be selected and copied.

### Keyboard

The editor uses Monaco's keybindings; Ctrl stands for ⌘ on macOS.

| Keys | Action |
|---|---|
| Arrows, Home, End, Page Up, Page Down | Move the cursor |
| Shift with any of them | Select |
| Ctrl+Home, Ctrl+End | Go to the start or end of the document |
| Ctrl+A | Select all |
| Ctrl+Shift+Alt with an arrow | Column selection: one cursor per line |
| Escape | Cancel the selection, then return to one cursor |
| Tab, Shift+Tab | Indent, outdent |
| Ctrl+Z, Ctrl+Y or Ctrl+Shift+Z | Undo, redo |
| Ctrl+C, Ctrl+X, Ctrl+V | Copy, cut and paste; without a selection, copy and cut take the whole line |
| Ctrl+F, Ctrl+H | Find, find and replace; F3 and Shift+F3 go to the next and previous match |
| Ctrl+D, Ctrl+Shift+L | Add the next occurrence of the selection, select all occurrences |
| Ctrl+Alt+Up, Ctrl+Alt+Down | Add a cursor above or below |
| Alt+Up, Alt+Down, Shift+Alt+Up, Shift+Alt+Down | Move or copy the lines up or down |
| Ctrl+Shift+K, Ctrl+Enter, Ctrl+Shift+Enter | Delete the line, insert a line below or above |
| Ctrl+/, Shift+Alt+A | Toggle a line comment, a block comment |
| Ctrl+Shift+\\ | Go to the matching bracket |
| Ctrl+Shift+[, Ctrl+Shift+] | Fold, unfold; Ctrl+K Ctrl+0 folds all and Ctrl+K Ctrl+J unfolds all |
| Shift+Alt+Right, Shift+Alt+Left | Expand, shrink the selection |
| Ctrl+Space, Ctrl+Shift+Space | Show suggestions, show parameter hints |
| F12, Shift+F12, Alt+F12 | Go to definition, go to references, peek definition |
| F2, Ctrl+. | Rename the symbol, show code actions |
| F8, Shift+F8 | Go to the next or previous problem |
| Shift+Alt+F | Format the document |
| Ctrl+M | Toggle whether Tab moves the focus |

With the mouse, click to place the cursor, drag or Shift+click to select, double-click to select a word and triple-click to select a line; keep dragging after a double or triple click to extend by words or lines. Alt+click adds a cursor (Alt+click it again to remove it), Alt+Shift+drag or a drag with the middle button selects a column, and a click or drag on the line numbers selects whole lines. Dragging past an edge scrolls. Drag selected text to move it (hold Ctrl, or Option on macOS, to copy it), and Ctrl+click a link to open it; the `LinkOpening` event lets the app open links itself.

## API reference

<!-- api: CodeEditor -->

## Accessibility

- Name the editor with `AutomationProperties.Name`, as the demos do, or put a label next to it.
- Tab is a key of the editor: it indents and does not move the focus out. Ctrl+M (Ctrl+Shift+M on macOS) switches on `TabFocusMode`, in which Tab moves the focus; tell keyboard users about it, or set `TabFocusMode` where the editor is one field of a form.
- Screen readers see an edit control whose value is the document. As the cursor moves, the editor reads out the line, the characters or the selection, and Ctrl+Alt+Shift+G reads the cursor's line and column.

## Motion

- The cursor blinks while the editor has focus and stops blinking for a moment after every key, so it is visible while you type. The find widget slides in when it opens; nothing else moves.

## Responsive behavior

- The editor fills the width it is given and has a minimum height of 80. Give it a height of your own: it scrolls its content and does not grow with it.
- With `WordWrap` on, the lines wrap again when the editor changes width.
- On touch screens, a tap places the cursor, a double tap or a long press selects a word and shows handles to drag, and a finger scrolls with momentum.
- On a narrow editor, turn `ShowMinimap` off to give the text the room.

## Related

- `CodeBlock`: code that is shown with highlighting, not edited.
- `Textarea`: plain multi-line text in a form.
- `RichTextEditor`: formatted text.
- `DiffEditor`: compare two versions of a text.
