using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class TabsBrowserTabsDemo : UserControl
{
    private readonly ObservableCollection<string> _files = new() { "README.md", "Program.cs", "App.axaml", "Tabs.cs", "CHANGELOG.md" };
    private int _untitled;

    public TabsBrowserTabsDemo()
    {
        InitializeComponent();
        Files.ItemsSource = _files;
        UpdateLog();
    }

    // The tabs only ask: the app removes the item, and the selection moves to the next tab (or the previous one at the end).
    private void OnTabCloseRequested(object? sender, TabCloseRequestedEventArgs e)
    {
        if (e.Item is string file)
        {
            _files.Remove(file);
            UpdateLog();
        }
    }

    private void OnTabAddRequested(object? sender, RoutedEventArgs e)
    {
        _untitled++;
        var file = $"Untitled-{_untitled}";
        _files.Add(file);
        Files.SelectedItem = file;
        UpdateLog();
    }

    private void UpdateLog() => Log.Text = _files.Count == 0 ? "All files closed." : $"{_files.Count} open files";
}
