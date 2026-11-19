using System.Numerics;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class ModelViewerPreviewDemo : UserControl
{
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
        };
    }
}
