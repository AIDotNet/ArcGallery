using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using TKCode.Arc.A2UI;
using TKCode.Arc.A2UI.Chat;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Plays the "Overview" answer line by line into a renderer: the surface appears and fills in as messages arrive.</summary>
public partial class A2UISurfaceViewPreviewDemo : UserControl
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(220) };
    private readonly Queue<string> _lines = new();
    private A2UIRenderer? _renderer;

    public A2UISurfaceViewPreviewDemo()
    {
        InitializeComponent();
        View.Registry = ChatAppState.Registry;
        _timer.Tick += (_, _) => Next();
        Replay();
    }

    private void OnReplay(object? sender, RoutedEventArgs e) => Replay();

    private void Replay()
    {
        _timer.Stop();
        _renderer?.Dispose();
        _renderer = new A2UIRenderer(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() });
        _renderer.Outbound += (_, e) => Status.Text = e.Kind == A2UIOutboundKind.Action ? "Action sent: " + e.ToJson() : Status.Text;
        View.Renderer = _renderer;
        _lines.Clear();
        foreach (var line in ShowcaseLibrary.All[0].Start.Split('\n'))
        {
            if (line.TrimStart().StartsWith('{'))
            {
                _lines.Enqueue(line.Trim());
            }
        }

        Status.Text = "Streaming…";
        _timer.Start();
    }

    private void Next()
    {
        if (_lines.Count == 0 || _renderer is null)
        {
            _timer.Stop();
            Status.Text = "Pick a plan and press Continue.";
            return;
        }

        _renderer.Process(_lines.Dequeue());
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }
}
