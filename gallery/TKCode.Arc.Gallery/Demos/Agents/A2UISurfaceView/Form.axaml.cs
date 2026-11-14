using System.Text.Json;
using Avalonia.Controls;
using TKCode.Arc.A2UI;
using TKCode.Arc.A2UI.Chat;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>The contact form example: two-way binding, checks and the action message.</summary>
public partial class A2UISurfaceViewFormDemo : UserControl
{
    private static readonly JsonSerializerOptions s_pretty = new() { WriteIndented = true };

    public A2UISurfaceViewFormDemo()
    {
        InitializeComponent();
        var renderer = new A2UIRenderer(new A2UIRendererOptions { Catalogs = ChatAppState.Catalogs.ToList() });
        renderer.Outbound += (_, e) =>
        {
            if (e.Kind == A2UIOutboundKind.Action)
            {
                Sent.Text = e.Action!.ToJsonString(s_pretty);
            }
        };
        View.Registry = ChatAppState.Registry;
        View.Renderer = renderer;
        var showcase = ShowcaseLibrary.All.First(s => s.Id == "contact");
        foreach (var line in showcase.Start.Split('\n').Where(l => l.TrimStart().StartsWith('{')))
        {
            renderer.Process(line.Trim());
        }

        var surface = renderer.GetSurface("contact_1")!;
        void Show(string _) => Data.Text = surface.DataModel.Get("/")?.ToJsonString(s_pretty);
        surface.DataModel.Changed += Show;
        Show("/");
        Sent.Text = "Nothing yet.";
    }
}
