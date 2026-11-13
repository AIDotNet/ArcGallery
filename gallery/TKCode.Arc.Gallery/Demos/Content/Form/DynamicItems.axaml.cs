using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class FormDynamicItemsDemo : UserControl
{
    private readonly FormContactsModel _model = new();

    public FormDynamicItemsDemo()
    {
        InitializeComponent();
        DataContext = _model;
        ShowCount();
        Contacts.PropertyChanged += (_, e) =>
        {
            if (e.Property == Form.ErrorCountProperty)
            {
                ShowCount();
            }
        };
    }

    private void OnRemove(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: FormContactModel contact })
        {
            _model.Remove(contact);
        }
    }

    private void ShowCount() =>
        Count.Text = Contacts.ErrorCount == 0 ? "No row has an error." : $"Form.ErrorCount: {Contacts.ErrorCount}";
}
