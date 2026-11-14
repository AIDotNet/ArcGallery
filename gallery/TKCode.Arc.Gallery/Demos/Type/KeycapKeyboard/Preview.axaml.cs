using Avalonia;
using Avalonia.Controls;
using TKCode.Arc.Primitives;

namespace TKCode.Arc.Gallery.Demos;

public partial class KeycapKeyboardPreviewDemo : UserControl
{
    public KeycapKeyboardPreviewDemo()
    {
        InitializeComponent();
        LayoutPicker.SelectionChanged += (_, _) =>
            Keys.Layout = LayoutPicker.SelectedValue as string == "Letters" ? KeycapLayout.Letters : KeycapLayout.Compact;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // A phone-sized view starts on the letters board, whose keys stay big enough to tap.
        if (ArcViewport.GetSize(this) is { Width: <= 560 })
        {
            LayoutPicker.SelectedValue = "Letters";
        }
    }
}
