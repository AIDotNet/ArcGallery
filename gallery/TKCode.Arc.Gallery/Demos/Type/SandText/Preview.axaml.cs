using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class SandTextPreviewDemo : UserControl
{
    public SandTextPreviewDemo()
    {
        InitializeComponent();
        Words.SelectionChanged += (_, _) =>
        {
            if (Words.SelectedItem is SegmentedOption { Value: string word })
            {
                Sand.Text = word;
            }
        };
    }
}
