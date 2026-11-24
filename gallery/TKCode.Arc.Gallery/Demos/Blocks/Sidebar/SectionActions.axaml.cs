using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Your own entity for a sidebar section: a titled group of pages.</summary>
public sealed class SidebarProjectGroup
{
    public string Key { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public bool IsCollapsible { get; init; }

    public string AddText => $"Add to {Title}";

    public ObservableCollection<SidebarMenuNode> Items { get; } = new();
}

public partial class SidebarSectionActionsDemo : UserControl
{
    private readonly ObservableCollection<SidebarProjectGroup> _groups = new();

    public SidebarSectionActionsDemo()
    {
        InitializeComponent();
        var projects = new SidebarProjectGroup { Key = "projects", Title = "Projects", IsCollapsible = true };
        projects.Items.Add(new SidebarMenuNode { Key = "projects/atlas", Title = "Atlas" });
        projects.Items.Add(new SidebarMenuNode { Key = "projects/beacon", Title = "Beacon" });
        var teams = new SidebarProjectGroup { Key = "teams", Title = "Teams", IsCollapsible = true };
        teams.Items.Add(new SidebarMenuNode { Key = "teams/design", Title = "Design" });
        teams.Items.Add(new SidebarMenuNode { Key = "teams/platform", Title = "Platform" });
        _groups.Add(projects);
        _groups.Add(teams);
        Nav.SectionsSource = _groups;
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: SidebarProjectGroup group })
        {
            var index = group.Items.Count + 1;
            group.Items.Add(new SidebarMenuNode { Key = $"{group.Key}/new-{index}", Title = $"New page {index}" });
            Log.Text = $"Added a page to {group.Title}.";
        }
    }

    private void OnRemove(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: SidebarMenuNode node })
        {
            return;
        }
        foreach (var group in _groups)
        {
            if (group.Items.Remove(node))
            {
                Log.Text = $"Removed {node.Title} from {group.Title}.";
                return;
            }
        }
    }
}
