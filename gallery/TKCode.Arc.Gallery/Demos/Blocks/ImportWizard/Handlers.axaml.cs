using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class ImportWizardHandlersDemo : UserControl
{
    private bool _failedOnce;

    public ImportWizardHandlersDemo()
    {
        InitializeComponent();

        // Stands in for a POST to your bulk endpoint. The third batch fails the first time, so "Try again" shows.
        Wizard.ImportBatchAction = async (records, cancellation) =>
        {
            await Task.Delay(300, cancellation);
            if (!_failedOnce && records.Count > 0 && records[0].RowNumber > 10)
            {
                _failedOnce = true;
                throw new InvalidOperationException("The server timed out.");
            }
        };

        Wizard.FileLoaded += (_, e) => Log.Text = $"{e.FileName}: {e.RowCount} rows, columns {string.Join(", ", e.Headers)}.";
        Wizard.BatchImported += (_, e) => Log.Text = $"Batch {e.Batch} of {e.BatchCount} saved, {e.Imported} of {e.Total} rows.";
        Wizard.Failed += (_, e) => Log.Text = $"Batch {e.Batch} failed: {e.Exception.Message}";
        Wizard.Completed += (_, e) => Log.Text = $"Done: {e.Created} new, {e.Updated} updated, {e.Skipped} left out.";
    }
}
