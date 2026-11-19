using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using TKCode.Arc.Motion;

namespace TKCode.Arc.Gallery.Demos;

public partial class ModelViewerPreviewDemo : UserControl
{
    private Rect? _viewport;
    private int _playing = -1;

    public ModelViewerPreviewDemo()
    {
        InitializeComponent();
        Viewer.ModelLoaded += (_, _) =>
        {
            var camera = Viewer.Camera;
            var controls = Viewer.CameraControls;
            var target = controls.Target;
            var distance = Vector3.Distance(camera.Position, target);
            camera.Position = target + Vector3.Normalize(new Vector3(0.18f, 0.06f, 1f)) * distance;
            camera.LookAt(target);
            controls.SaveState();
            UpdatePlayback();
        };
        Clips.SelectionChanged += (_, _) => UpdatePlayback();
        Viewer.EffectiveViewportChanged += (_, e) =>
        {
            _viewport = e.EffectiveViewport;
            UpdatePlayback();
        };
        Viewer.DetachedFromVisualTree += (_, _) =>
        {
            _viewport = null;
            UpdatePlayback();
        };
        Viewer.PropertyChanged += (_, e) =>
        {
            if (e.Property == ArcMotion.LevelProperty || e.Property == Visual.BoundsProperty || e.Property == Visual.IsVisibleProperty)
            {
                UpdatePlayback();
            }
        };
        Viewer.Classes.CollectionChanged += (_, _) => UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        var animations = Viewer.Animations;
        Clips.IsEnabled = animations.Count > 0 && ArcMotion.IsFull(Viewer);

        var next = -1;
        if (Clips.IsEnabled && Viewer.IsEffectivelyVisible && _viewport is { } viewport
            && viewport.Intersects(new Rect(Viewer.Bounds.Size)) && Clips.SelectedValue is string selected)
        {
            for (var i = 0; i < animations.Count; i++)
            {
                if (animations[i].Name == selected)
                {
                    next = i;
                    break;
                }
            }
        }

        if (next == _playing)
        {
            return;
        }

        _playing = next;
        if (next < 0)
        {
            Viewer.StopAnimation();
        }
        else
        {
            Viewer.PlayAnimation(next);
        }
    }
}
