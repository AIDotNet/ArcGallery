using Avalonia.Controls;
using TKCode.Arc.Mobile;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>A currency the picker demo offers.</summary>
public sealed record MobilePickerCurrency(string Symbol, string Name);

public partial class MobilePickerTemplatesDemo : UserControl
{
    public MobilePickerTemplatesDemo()
    {
        InitializeComponent();
        Currency.ItemsSource = new[]
        {
            new MobilePickerCurrency("$", "US dollar"),
            new MobilePickerCurrency("€", "Euro"),
            new MobilePickerCurrency("£", "Pound sterling"),
            new MobilePickerCurrency("¥", "Japanese yen"),
        };
    }

    private void OnSelectionChanged(object? sender, PickerSelectionChangedEventArgs e)
        => Log.Text = $"SelectionChanged: {(e.NewItem as MobilePickerCurrency)?.Name ?? "none"} (index {e.NewIndex})";
}
