using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class WindRoseChartSelectionDemo : UserControl
{
    public WindRoseChartSelectionDemo()
    {
        InitializeComponent();
        var year = WindRoseBinning.Bin(WindRoseDemoData.Year(), 8);
        Chart.ItemsSource = year.Rows;
        Chart.Calm = year.Calm;
        // SelectionChanged reports every change of SelectedIndex; ItemInvoked would also report choosing the same sector again.
        Chart.SelectionChanged += (_, e) =>
        {
            if (e.NewItem is not { } sector)
            {
                Details.Text = "Choose a direction to see where its wind comes from.";
                return;
            }
            var strongest = 0;
            for (var bin = 1; bin < sector.Values.Count; bin++)
            {
                if (sector.Values[bin] > sector.Values[strongest])
                {
                    strongest = bin;
                }
            }
            var bins = WindRoseBinning.DefaultBinMinimums;
            var band = strongest + 1 < bins.Count ? $"{bins[strongest]:0.#}–{bins[strongest + 1]:0.#} m/s" : $"{bins[strongest]:0.#} m/s or more";
            Details.Text = $"{sector.Label} ({sector.From:0.#}°–{sector.To:0.#}°): {sector.Share:P1} of all hours, {sector.Total:N0} hours, most often {band}.";
        };
    }
}
