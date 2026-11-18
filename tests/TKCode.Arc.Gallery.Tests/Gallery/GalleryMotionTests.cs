using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using TKCode.Arc.Gallery.Shell;
using TKCode.Arc.Motion;

namespace TKCode.Arc.Tests.Gallery;

public class GalleryMotionTests
{
    [AvaloniaFact]
    public void Appearance_Motion_Caps_The_Level_Set_By_A_Demo()
    {
        var appearance = new GalleryTheme();
        var demo = new Border();
        ArcMotion.SetLevel(demo, ArcMotionLevel.Reduced);
        var shell = new Border { Child = demo };
        var window = TestHelpers.Show(shell, ArcMotionLevel.Full);
        try
        {
            appearance.SetMotion(shell, ArcMotionLevel.Reduced);
            ArcMotion.GetEffectiveLevel(demo).ShouldBe(ArcMotionLevel.Reduced);

            appearance.SetMotion(shell, ArcMotionLevel.Off);
            ArcMotion.GetEffectiveLevel(demo).ShouldBe(ArcMotionLevel.Off);

            appearance.SetMotion(shell, ArcMotionLevel.Full);
            ArcMotion.GetEffectiveLevel(demo).ShouldBe(ArcMotionLevel.Reduced);
        }
        finally
        {
            window.Close();
        }
    }
}
