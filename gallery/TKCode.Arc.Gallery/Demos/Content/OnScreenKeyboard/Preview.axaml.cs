using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class OnScreenKeyboardPreviewDemo : UserControl
{
    public OnScreenKeyboardPreviewDemo()
    {
        InitializeComponent();

        // The keyboard types into whichever field has focus. Until one has, the first key starts in the name field.
        Keyboard.ItemInvoked += (_, e) =>
        {
            if (e.Target is null && NameField.Focus())
            {
                e.Handled = true;
                Keyboard.Invoke(e.Item);
            }
        };
    }
}
