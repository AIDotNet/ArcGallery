using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindRoseChartSectorCountsDemo : UserControl
{
    private static readonly WindObservation[] s_readings = WindRoseDemoData.Year();

    public WindRoseChartSectorCountsDemo()
    {
        InitializeComponent();
        Show(16);
        // The same readings sorted again: each new petal grows from the old petal that covered its direction.
        Picker.SelectionChanged += (_, _) =>
        {
            if (Picker.SelectedValue is string value && int.TryParse(value, out var count))
            {
                Show(count);
            }
        };
    }

    private void Show(int count)
    {
        var data = WindRoseBinning.Bin(s_readings, count);
        Chart.SectorCount = count;
        Chart.ItemsSource = data.Rows;
        Chart.Calm = data.Calm;
    }
}
