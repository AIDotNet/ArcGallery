using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormPreviewDemo : UserControl
{
    public FormPreviewDemo()
    {
        InitializeComponent();
        DataContext = new FormSignUpModel();
    }
}
