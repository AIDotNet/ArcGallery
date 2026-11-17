using Avalonia.Headless.XUnit;
using TKCode.Arc.Gallery.Docs;

namespace TKCode.Arc.Tests.Gallery;

/// <summary>
/// Every API table says how the control can be restyled (AIDotNet/TKCode.Arc#154): a control that draws in Render says a new
/// template does not change it; a templated one says its template can be replaced.
/// </summary>
public class GalleryCustomizationTests
{
    [AvaloniaFact]
    public void A_Drawn_Control_Says_It_Is_Restyled_Through_Tokens_And_Brushes()
    {
        GalleryDocs.ApiReference("Gauge", "en-US").ShouldContain("Customization: drawn in code (`Render`).");
        GalleryDocs.ApiReference("Gauge", "zh-CN").ShouldContain("定制方式：代码绘制（`Render`）。");
        GalleryDocs.ApiReference("Gauge", "zh-TW").ShouldContain("自訂方式：程式碼繪製（`Render`）。");
    }

    [AvaloniaFact]
    public void A_Templated_Control_Says_Its_Template_Can_Be_Replaced()
    {
        var accordion = GalleryDocs.ApiReference("Accordion", "en-US");
        accordion.ShouldContain("Customization: templated.");
        accordion.ShouldNotContain("drawn in code");
    }
}
