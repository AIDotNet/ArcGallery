using System.Globalization;
using System.Text;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Side by side or inline, hidden unchanged regions and ignored whitespace switched at run time; DiffComputed reports the size of the diff.</summary>
public partial class DiffEditorOptionsDemo : UserControl
{
    public DiffEditorOptionsDemo()
    {
        InitializeComponent();
        Diff.DiffComputed += (_, e) => Status.Text = e.IsIdentical
            ? "No changes"
            : string.Create(CultureInfo.InvariantCulture, $"{e.ChangeCount} changes: {e.InsertedLineCount} lines added, {e.RemovedLineCount} removed");
        Diff.OriginalText = Settings(theme: "light", fontSize: 13, tabSize: 4, extra: false);
        Diff.ModifiedText = Settings(theme: "dark", fontSize: 14, tabSize: 2, extra: true);
    }

    private static string Settings(string theme, int fontSize, int tabSize, bool extra)
    {
        var sb = new StringBuilder("{\n");
        sb.Append(CultureInfo.InvariantCulture, $"  \"theme\": \"{theme}\",\n");
        sb.Append(CultureInfo.InvariantCulture, $"  \"fontSize\": {fontSize},\n");
        for (var i = 1; i <= 30; i++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"  \"option{i}\": {(i % 3 == 0 ? "true" : "false")},\n");
        }

        sb.Append(CultureInfo.InvariantCulture, $"  \"tabSize\": {tabSize}");
        if (extra)
        {
            sb.Append(",\n  \"wordWrap\": \"on\"");
        }

        return sb.Append("\n}").ToString();
    }
}
