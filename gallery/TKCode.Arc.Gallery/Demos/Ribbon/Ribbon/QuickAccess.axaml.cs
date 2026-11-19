using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class RibbonQuickAccessDemo : UserControl
{
    public RibbonQuickAccessDemo()
    {
        InitializeComponent();
        QuickAccess.Ribbon = MainRibbon;
        QuickAccess.Keys.Add("find");
        ShowKeys();
        QuickAccess.Keys.CollectionChanged += (_, _) => ShowKeys();
    }

    private void ShowKeys() => KeysText.Text = "Keys: " + (QuickAccess.Keys.Count == 0 ? "(none)" : string.Join(", ", QuickAccess.Keys));
}
