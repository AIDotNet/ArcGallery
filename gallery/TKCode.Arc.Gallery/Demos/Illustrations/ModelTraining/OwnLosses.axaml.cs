using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class ModelTrainingOwnLossesDemo : UserControl
{
    private readonly List<string> _losses = new();

    public ModelTrainingOwnLossesDemo()
    {
        InitializeComponent();
        Card.ClassNames = ["Spam", "Inbox"];
        Card.Labels = new ModelTrainingLabels { Unit = "emails" };
        // The measured losses, spread over the run: a fast fall that flattens out early.
        Card.LossValues = [1.12, 0.71, 0.52, 0.44, 0.4, 0.38, 0.37];
    }

    private void OnIterationCompleted(object? sender, ModelTrainingIterationEventArgs e)
    {
        if (e.Iteration == 1)
        {
            _losses.Clear();
        }
        _losses.Add($"{e.Iteration}: {e.Loss:0.00}");
        Log.Text = string.Join("  ·  ", _losses);
    }
}
