using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormAsyncCheckDemo : UserControl
{
    public FormAsyncCheckDemo()
    {
        InitializeComponent();
        DataContext = new FormSignUpModel();
        ShowState();
        Check.PropertyChanged += (_, e) =>
        {
            if (e.Property == Form.IsValidatingProperty || e.Property == Form.HasErrorsProperty)
            {
                ShowState();
            }
        };
    }

    private void ShowState() =>
        State.Text = Check.IsValidating ? "Form.IsValidating: true (a submit would wait)" : $"Form.HasErrors: {Check.HasErrors}";
}
