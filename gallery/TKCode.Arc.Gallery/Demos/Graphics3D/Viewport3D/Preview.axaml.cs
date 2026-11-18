using System.Numerics;
using Avalonia.Controls;
using TKCode.Arc.Graphics3D;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>A lit scene with shadows on a Viewport3D: orbit controls, and a switch that spins the box in FrameRendering.</summary>
public partial class Viewport3DPreviewDemo : UserControl
{
    private readonly Mesh _box;

    public Viewport3DPreviewDemo()
    {
        InitializeComponent();

        // Units, light intensities and the camera follow three.js: meters, Y up, the field of view in degrees.
        var scene = new Scene();
        var floor = new Mesh(new PlaneGeometry(12f, 12f), new StandardMaterial { Color = ColorF.FromHex(0x8a8f98), Roughness = 0.9f })
        {
            ReceiveShadow = true,
        };
        floor.RotateX(-MathF.PI / 2f);
        _box = new Mesh(new BoxGeometry(1f, 1f, 1f), new StandardMaterial { Color = ColorF.FromHex(0xe5484d), Roughness = 0.4f })
        {
            Position = new Vector3(-1.2f, 0.5f, 0f),
            CastShadow = true,
            ReceiveShadow = true,
        };
        var sphere = new Mesh(new SphereGeometry(0.6f, 48, 24), new StandardMaterial { Color = ColorF.FromHex(0x3e63dd), Roughness = 0.25f, Metalness = 0.1f })
        {
            Position = new Vector3(0.9f, 0.6f, 0.3f),
            CastShadow = true,
            ReceiveShadow = true,
        };
        var sun = new DirectionalLight(3f) { Position = new Vector3(3f, 5f, 2f), CastShadow = true };
        scene.Add(floor, _box, sphere, sun, new AmbientLight(0.4f),
            new HemisphereLight(ColorF.FromHex(0xbfd4ff), ColorF.FromHex(0x30281c), 0.6f));

        var target = new Vector3(0f, 0.4f, 0f);
        var camera = new PerspectiveCamera(45f, 2f, 0.1f, 100f) { Position = new Vector3(3.2f, 2.6f, 4.2f) };
        camera.LookAt(target);

        Viewport.Scene = scene;
        Viewport.Camera = camera;
        // The controls take the viewport's pointer, wheel and arrow keys; the limits keep the camera above the floor.
        Viewport.CameraControls = new OrbitControls(camera)
        {
            Target = target,
            EnableDamping = true,
            MinDistance = 2f,
            MaxDistance = 12f,
            MaxPolarAngle = (MathF.PI / 2f) - 0.05f,
        };

        // The viewport draws on demand. While the switch is on, each frame turns the box and asks for the next one;
        // turning the switch on asks for the first.
        Viewport.FrameRendering += OnFrameRendering;
        Spin.IsCheckedChanged += (_, _) => Viewport.RequestRender();
    }

    private void OnFrameRendering(object? sender, Viewport3DFrameEventArgs e)
    {
        if (Spin.IsChecked == true)
        {
            _box.RotateY((float)e.Elapsed.TotalSeconds * 1.2f);
            e.RequestNextFrame = true;
        }
    }
}
