using System.Numerics;
using Avalonia.Controls;
using TKCode.Arc.Graphics3D;
using TKCode.Arc.Graphics3D.Shading;
using TKCode.Arc.Motion;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Node materials on a Viewport3D: the surface's color, roughness, opacity and vertex positions as graphs of Sl functions.</summary>
public partial class Viewport3DNodeMaterialsDemo : UserControl
{
    public Viewport3DNodeMaterialsDemo()
    {
        InitializeComponent();
        var scene = new Scene { Background = ColorF.FromHex(0x14161a) };

        // Marble: fractal noise picks between two colors and drives roughness.
        var veins = Sl.Fbm(Sl.PositionLocal * 3f + Sl.Vec3(Sl.Fbm(Sl.PositionLocal * 2f) * 2f));
        var marble = new NodeMaterial
        {
            ColorNode = Sl.Mix(Sl.Color(0xf2efe9), Sl.Color(0x39424e), Sl.SmoothStep(0.45f, 0.7f, veins)),
            RoughnessNode = Sl.Mix(0.15f, 0.6f, veins),
        };
        scene.Add(new Mesh(new SphereGeometry(1f, 64, 32), marble) { Position = new Vector3(-2.4f, 0f, 0f) });

        // A wobbling blob: vertices move along their normals, color from a rim term, animated by time.
        var wave = Sl.Sin(Sl.PositionLocal.Y * 8f + Sl.Time * 3f) * 0.08f;
        var blob = new NodeMaterial
        {
            PositionNode = Sl.PositionLocal + Sl.NormalLocal * wave,
            ColorNode = Sl.Mix(Sl.Color(0x3e63dd), Sl.Color(0xff5fd2), Sl.Fresnel(2f)),
            RoughnessNode = 0.3f,
            FlatShading = true,
        };
        scene.Add(new Mesh(new SphereGeometry(1f, 96, 48), blob));

        // Unlit hologram stripes with a uniform the application can change.
        var speed = Sl.Uniform(4f);
        var hologram = new NodeMaterial
        {
            Lit = false,
            Transparent = true,
            Side = Side.Double,
            ColorNode = Sl.Color(0x4cc9f0) * (0.6f + Sl.Fresnel(1.5f) * 2f),
            OpacityNode = Sl.Saturate(Sl.Step(0.5f, Sl.Fract(Sl.PositionWorld.Y * 10f - Sl.Time * speed)) * 0.6f + Sl.Fresnel(2f)),
        };
        scene.Add(new Mesh(new SphereGeometry(1f, 48, 24), hologram) { Position = new Vector3(2.4f, 0f, 0f) });
        scene.Add(new DirectionalLight(2f) { Position = new Vector3(3f, 4f, 5f) },
            new HemisphereLight(ColorF.FromHex(0xbfd4ff), ColorF.FromHex(0x30281c), 0.8f));

        var camera = new PerspectiveCamera(45f, 2f, 0.1f, 200f) { Position = new Vector3(0f, 0.6f, 7f) };
        camera.LookAt(Vector3.Zero);
        Viewport.Scene = scene;
        Viewport.Camera = camera;
        Viewport.CameraControls = new OrbitControls(camera) { EnableDamping = true, EnablePan = false, MinDistance = 4f, MaxDistance = 12f };

        // Sl.Time moves only when frames are drawn: keep them coming at full motion. With reduced motion the picture
        // stands still and is redrawn when the camera moves.
        Viewport.FrameRendering += (_, e) =>
        {
            if (ArcMotion.IsFull(this))
            {
                e.RequestNextFrame = true;
            }
        };

        // The viewport does not observe materials: ask for a frame after changing one.
        Speed.ValueChanged += (_, _) =>
        {
            speed.Float = (float)Speed.Value;
            Viewport.RequestRender();
        };
    }
}
