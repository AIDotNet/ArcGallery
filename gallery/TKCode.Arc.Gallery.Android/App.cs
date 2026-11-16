using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using TKCode.Arc.Gallery.Shell;

namespace TKCode.Arc.Gallery.Android;

public partial class App : Avalonia.Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        // A phone has one view and no windows, so the shell is the page itself, as in the browser. The installed app has no
        // update check or settings of the desktop app yet: it is the site's layout with the files it carries (GalleryHostKind.Mobile
        // would follow once the shell has mobile-specific chrome).
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            GalleryHost.Kind = GalleryHostKind.Desktop;
            GalleryHost.ClientOs = GalleryClientOs.Android;
            var shell = new GalleryShell();
            // Android draws the app under the status and navigation bars (the system forces it from API 35); the safe area is
            // what is left, so the shell sits inside it instead of under the clock.
            var frame = new Border { Child = shell };
            frame.AttachedToVisualTree += (_, _) =>
            {
                if (TopLevel.GetTopLevel(frame) is { InsetsManager: { } insets } top)
                {
                    // The insets arrive in device pixels; layout is in device-independent units.
                    void Apply(Thickness pixels) => frame.Padding = new Thickness(
                        pixels.Left / top.RenderScaling, pixels.Top / top.RenderScaling,
                        pixels.Right / top.RenderScaling, pixels.Bottom / top.RenderScaling);
                    Apply(insets.SafeAreaPadding);
                    insets.SafeAreaChanged += (_, e) => Apply(e.SafeAreaPadding);
                }
            };
            singleView.MainView = frame;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
