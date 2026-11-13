using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;

namespace TKCode.Arc.Gallery.Demos;

public partial class RadialMenuMvvmDemo : UserControl
{
    public RadialMenuMvvmDemo()
    {
        InitializeComponent();
        DataContext = new RadialMenuMvvmViewModel();
    }
}

/// <summary>One tool of the demo: what the radial menu shows and the command it runs.</summary>
public sealed class RadialMenuToolItem(string name, Geometry icon, ICommand command)
{
    public string Name { get; } = name;

    public Geometry Icon { get; } = icon;

    public ICommand Command { get; } = command;
}

public sealed class RadialMenuMvvmViewModel : INotifyPropertyChanged
{
    private static readonly (string Name, Geometry Icon)[] s_extraTools =
    [
        ("Comment", ArcIcons.MessageSquare), ("Image", ArcIcons.Image), ("Link", ArcIcons.Link), ("Eraser", ArcIcons.Eraser)
    ];

    private readonly List<string> _recent = new();
    private readonly DemoCommand _deleteCommand;
    private readonly DemoCommand _removeToolCommand;
    private string _currentTool = "Select";
    private bool _isLocked;

    public RadialMenuMvvmViewModel()
    {
        _deleteCommand = new DemoCommand(_ => CurrentTool = "Deleted the selection", _ => !IsLocked);
        Tools =
        [
            Tool("Select", ArcIcons.Move),
            Tool("Pen", ArcIcons.Pencil),
            Tool("Text", ArcIcons.Type),
            Tool("Shape", ArcIcons.Square),
            new RadialMenuToolItem("Delete", ArcIcons.Trash2, _deleteCommand)
        ];
        var tools = Tools;
        _removeToolCommand = new DemoCommand(_ => tools.RemoveAt(tools.Count - 2), _ => tools.Count > 3);
        Tools.CollectionChanged += (_, _) => _removeToolCommand.Refresh();
        AddToolCommand = new DemoCommand(_ => AddTool(), _ => tools.Count < 8);
        Tools.CollectionChanged += (_, _) => ((DemoCommand)AddToolCommand).Refresh();
        RememberCommand = new DemoCommand(item => Remember((RadialMenuToolItem)item!));
    }

    public ObservableCollection<RadialMenuToolItem> Tools { get; }

    /// <summary>Runs for every choice (the menu's ItemInvokedCommand): remembers the tool.</summary>
    public ICommand RememberCommand { get; }

    public ICommand AddToolCommand { get; }

    public ICommand RemoveToolCommand => _removeToolCommand;

    public string CurrentTool
    {
        get => _currentTool;
        private set
        {
            _currentTool = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTool)));
        }
    }

    /// <summary>While locked, the Delete tool's command cannot run, so its item is disabled.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLocked)));
            _deleteCommand.Refresh();
        }
    }

    public string Recent => _recent.Count == 0 ? "No tools used yet." : "Recently used: " + string.Join(", ", _recent);

    public event PropertyChangedEventHandler? PropertyChanged;

    private RadialMenuToolItem Tool(string name, Geometry icon) => new(name, icon, new DemoCommand(_ => CurrentTool = name));

    private void AddTool()
    {
        var next = s_extraTools.FirstOrDefault(t => Tools.All(tool => tool.Name != t.Name));
        if (next.Name is not null)
        {
            Tools.Insert(Tools.Count - 1, Tool(next.Name, next.Icon));
        }
    }

    private void Remember(RadialMenuToolItem tool)
    {
        _recent.Remove(tool.Name);
        _recent.Insert(0, tool.Name);
        if (_recent.Count > 3)
        {
            _recent.RemoveAt(3);
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Recent)));
    }

    private sealed class DemoCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => execute(parameter);

        public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
