using System.Globalization;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Word wrap, line numbers, indent guides and read-only switched at run time; TextChanged keeps a line and character count.</summary>
public partial class CodeEditorOptionsDemo : UserControl
{
    public CodeEditorOptionsDemo()
    {
        InitializeComponent();
        Editor.TextChanged += (_, _) => UpdateStatus();
        Editor.Text = "A long line wraps at the width of the editor when word wrap is on; turn it off and the line runs on to the right, and the wheel or Shift+wheel scrolls it into view.\n\nIndented lines keep their indentation:\n    one\n        two\n";
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var text = Editor.Text;
        var lines = text.Split('\n').Length;
        Status.Text = string.Create(CultureInfo.InvariantCulture, $"{lines} lines, {text.Length} characters");
    }
}
