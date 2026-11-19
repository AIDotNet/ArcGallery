using System.Numerics;
using Avalonia.Controls;
using TKCode.Arc.Graphics3D;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>A model built from the engine's geometries and shown by ModelViewer, which frames, lights and shadows it.</summary>
public partial class ModelViewerFromCodeDemo : UserControl
{
    public ModelViewerFromCodeDemo()
    {
        InitializeComponent();
        var model = new Group();
        model.Add(new Mesh(new TorusKnotGeometry(0.6f, 0.2f, 160, 24), new PhysicalMaterial
        {
            Color = ColorF.FromHex(0xc2410c),
            Roughness = 0.35f,
            Clearcoat = 1f,
            ClearcoatRoughness = 0.08f,
        })
        {
            Position = new Vector3(0f, 1.1f, 0f),
        });
        model.Add(new Mesh(new CylinderGeometry(0.9f, 1f, 0.18f, 64), new StandardMaterial { Color = ColorF.FromHex(0xd9d9de), Roughness = 0.25f, Metalness = 1f })
        {
            Position = new Vector3(0f, 0.09f, 0f),
        });

        // The viewer does not dispose an object it was given: it stays yours.
        Viewer.Model = model;
    }
}
