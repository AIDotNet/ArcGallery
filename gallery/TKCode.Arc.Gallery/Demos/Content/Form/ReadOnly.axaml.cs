using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormReadOnlyDemo : UserControl
{
    public FormReadOnlyDemo()
    {
        InitializeComponent();
        var model = new FormProfileModel { Name = "Ada Lovelace", Email = "ada@" };
        DataContext = model;
    }
}
