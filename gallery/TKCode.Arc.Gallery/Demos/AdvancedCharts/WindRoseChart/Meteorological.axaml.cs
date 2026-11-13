using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindRoseChartMeteorologicalDemo : UserControl
{
    public WindRoseChartMeteorologicalDemo()
    {
        InitializeComponent();
        // A year of synthetic hourly readings, sorted into 16 directions and the default speed bins (m/s).
        // Readings below 0.5 m/s are calm: they have no direction and show in the centre.
        var readings = WindRoseDemoData.Year();
        var year = WindRoseBinning.Bin(readings, 16);
        Chart.ItemsSource = year.Rows;
        Chart.Calm = year.Calm;
        Caption.Text = $"{year.Total:N0} hourly readings (synthetic data); petals show the share of all hours, calm included.";
    }
}
