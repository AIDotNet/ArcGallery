using System.Runtime.InteropServices.JavaScript;
using TKCode.Arc.Gallery.Shell;

namespace TKCode.Arc.Gallery.Browser;

/// <summary>Keeps the date (<c>yyyy-MM-dd</c>) the templates popup was last shown in local storage (the <c>arc-promo</c> module of main.js).</summary>
internal sealed partial class BrowserTemplatesPromoStore : IGalleryTemplatesPromoStore
{
    public string? ReadLastShownDate() => Load() is { Length: > 0 } date ? date : null;

    public void WriteLastShownDate(string date) => Save(date);

    [JSImport("read", "arc-promo")]
    private static partial string Load();

    [JSImport("write", "arc-promo")]
    private static partial void Save(string date);
}
