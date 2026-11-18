using System.Numerics;
using Avalonia.Controls;
using TKCode.Arc.Graphics3D;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Switches a ModelViewer's environment, auto-rotation and ground shadow; the three materials show the difference.</summary>
public partial class ModelViewerLightingDemo : UserControl
{
    public ModelViewerLightingDemo()
    {
        InitializeComponent();

        // Polished metal mirrors the environment, a coated paint adds a sharp highlight, and a matte surface takes its soft light.
        var model = new Group();
        model.Add(new Mesh(new SphereGeometry(0.55f, 64, 32), new StandardMaterial { Color = ColorF.FromHex(0xf5c451), Roughness = 0.18f, Metalness = 1f })
        {
            Position = new Vector3(-1.3f, 0.55f, 0f),
        });
        model.Add(new Mesh(new RoundedBoxGeometry(0.9f, 0.9f, 0.9f, 4, 0.12f), new PhysicalMaterial
        {
            Color = ColorF.FromHex(0xb91c1c),
            Roughness = 0.45f,
            Clearcoat = 1f,
            ClearcoatRoughness = 0.05f,
        })
        {
            Position = new Vector3(0f, 0.45f, 0f),
        });
        model.Add(new Mesh(new ConeGeometry(0.5f, 1.1f, 64), new StandardMaterial { Color = ColorF.FromHex(0xf4f4f5), Roughness = 0.85f })
        {
            Position = new Vector3(1.3f, 0.55f, 0f),
        });
        Viewer.Model = model;

        Environments.SelectionChanged += (_, _) => Viewer.Environment = Environments.SelectedValue switch
        {
            "Outdoor" => ModelViewerEnvironment.Outdoor,
            "None" => ModelViewerEnvironment.None,
            _ => ModelViewerEnvironment.Studio,
        };
        // ShadowIntensity runs from 0 (no shadow) to 1.
        Shadow.ValueChanged += (_, _) => Viewer.ShadowIntensity = Shadow.Value / 100d;
    }
}
