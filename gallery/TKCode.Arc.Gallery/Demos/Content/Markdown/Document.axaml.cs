using Avalonia.Controls;
using StreamMark.Avalonia.Rendering;

namespace TKCode.Arc.Gallery.Demos;

public partial class MarkdownDocumentDemo : UserControl
{
    public MarkdownDocumentDemo()
    {
        InitializeComponent();
        Document.Text = """
            # Release notes

            Arc's `Markdown` control uses *StreamMark*. Its incremental parser reuses completed sections as text arrives.
            TextMate highlights fenced code in Arc's syntax palette, just like a [CodeBlock](https://uiarc.dev/components/code-block).

            ## What changed

            - Headings, lists, tables, quotes and links follow the ArcDesign tokens.
            - Fenced code highlights **C#**, **TypeScript**, **JSON**, **XAML**, shell and many more.
              - Unknown languages stay plain text.
            - Light, dark and the accent switch live.

            1. Add `TKCode.Arc.Markdown`.
            2. Call `UseArcMarkdown()` in `UseArc(...)`.
            3. Drop `<arc:Markdown Text="..." />` into a view.

            - [x] Static documents
            - [x] Streamed replies
            - [ ] Math and diagrams (add StreamMark's RaTeX and Mermaid plugins)

            > Color supports, but never replaces, the source text.
            > — Arc's code block notes

            | Feature | Status | Notes |
            |:--------|:------:|------:|
            | Text | Ready | `Text` |
            | Streaming | Ready | `Append` / `Complete` |
            | Links | Ready | `LinkClicked` |

            ## Code

            ```cs
            public sealed class CartView : UserControl
            {
                // Totals update when an item changes.
                public decimal Total { get; private set; }

                public void Add(decimal price, int quantity = 1) => Total += price * quantity;
            }
            ```

            ```ts
            export async function loadUser(id: number): Promise<User | null> {
              const response = await fetch(`/api/users/${id}`);
              return response.ok ? ((await response.json()) as User) : null;
            }
            ```

            ```json
            { "name": "arc-demo", "version": "1.4.0", "private": true, "tags": ["ui", "motion"] }
            ```

            ```xml
            <!-- A streamed reply in a chat bubble -->
            <arc:Markdown FontSize="14" Text="{Binding Reply}" />
            ```

            ```bash
            dotnet add package TKCode.Arc.Markdown
            dotnet run --project src/App
            ```
            """;
    }

    private void OnLinkClicked(object? sender, LinkClickedEventArgs e) => Status.Text = $"LinkClicked: {e.Url}";
}
