using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TKCode.Arc.Gallery.Demos;

public partial class RadialMenuMarkingMenuDemo : UserControl
{
    private readonly List<Point> _trail = new();
    private int _count;

    public RadialMenuMarkingMenuDemo()
    {
        InitializeComponent();
        // The press that opens the menu starts the trail; the menu captures the pointer, so its moves arrive on the menu.
        Board.AddHandler(PointerPressedEvent, OnBoardPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        Edits.AddHandler(PointerMovedEvent, OnMenuPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        Edits.ItemInvoked += (_, e) => Status.Text = $"{e.Container.Header} ({++_count} so far)";
    }

    private void OnBoardPressed(object? sender, PointerPressedEventArgs e)
    {
        _trail.Clear();
        _trail.Add(e.GetPosition(Surface));
        Trail.Points = _trail.ToList();
    }

    private void OnMenuPointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(Surface).Properties;
        if (_trail.Count == 0 || !(properties.IsLeftButtonPressed || properties.IsRightButtonPressed))
        {
            return;
        }
        _trail.Add(e.GetPosition(Surface));
        Trail.Points = _trail.ToList();
    }
}
