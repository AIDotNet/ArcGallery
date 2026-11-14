using Avalonia;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class TocProgressPreviewDemo : UserControl
{
    private bool? _narrow;

    public TocProgressPreviewDemo()
    {
        InitializeComponent();
        Layout.SizeChanged += (_, e) => Stack(e.NewSize.Width < 600);
        Toc.LayoutUpdated += (_, _) =>
        {
            if (string.IsNullOrEmpty(Current.Text) && Toc.Sections.Count > 0)
            {
                Current.Text = Toc.Sections[Math.Max(0, Toc.ActiveIndex)].Title;
            }
        };
    }

    /// <summary>The footer names the section the marker is on.</summary>
    private void OnActiveChanged(object? sender, TocProgressActiveChangedEventArgs e) =>
        Current.Text = (e.Entry ?? Toc.Sections.FirstOrDefault())?.Title ?? string.Empty;

    /// <summary>On narrow screens the contents sit above the article in a card of their own and the footer hides.</summary>
    private void Stack(bool narrow)
    {
        if (_narrow == narrow)
        {
            return;
        }
        _narrow = narrow;
        Layout.ColumnDefinitions = narrow ? new ColumnDefinitions("*") : new ColumnDefinitions("*,224");
        Grid.SetColumn(Aside, narrow ? 0 : 1);
        Grid.SetRow(Aside, 0);
        Grid.SetRow(ArticleCard, narrow ? 1 : 0);
        Aside.Classes.Set("card", narrow);
        ArticleCard.Classes.Set("narrow", narrow);
        Footer.IsVisible = !narrow;
    }
}
