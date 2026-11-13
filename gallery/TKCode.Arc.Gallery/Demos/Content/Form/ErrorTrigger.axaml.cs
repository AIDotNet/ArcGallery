using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormErrorTriggerDemo : UserControl
{
    public FormErrorTriggerDemo()
    {
        InitializeComponent();
        // One view model per form: the rules are the same, only when the errors show differs.
        ChangeForm.DataContext = new FormProfileModel();
        BlurForm.DataContext = new FormProfileModel();
        SubmitForm.DataContext = new FormProfileModel();
    }
}
