using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RibbonPreviewDemo : UserControl
{
    public RibbonPreviewDemo()
    {
        InitializeComponent();
        QuickAccess.Ribbon = MainRibbon;
        QuickAccess.Keys.Add("run");
        MainRibbon.ItemInvoked += (_, e) =>
        {
            var command = (e.Item as RibbonButton)?.Label ?? "Command";
            Status.Text = e.Entry?.Header is { } row ? $"{command} › {row}" : command;
        };
    }
}
