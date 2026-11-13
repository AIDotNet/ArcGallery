using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindRoseChartCountAxisDemo : UserControl
{
    public WindRoseChartCountAxisDemo()
    {
        InitializeComponent();
        var january = WindRoseBinning.Bin(WindRoseDemoData.Month(0), 16);
        Chart.ItemsSource = january.Rows;
        Chart.Calm = january.Calm;
        // The amounts stay the same; only the axis changes, and the rings and petals spring to the new scale.
        Picker.SelectionChanged += (_, _) =>
            Chart.RadialAxis = Picker.SelectedValue as string == "count" ? WindRoseRadialAxis.Count : WindRoseRadialAxis.Percent;
    }
}
