using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using TKCode.Arc.Fonts;
using TKCode.Arc.Gallery.Localization;
using TKCode.Arc.Gallery.Shell;
using TKCode.Arc.Gallery.Demos;
using TKCode.Arc.Gallery.Docs;
using TKCode.Arc.Localization;

namespace TKCode.Arc.Gallery.Android;

[Application]
public sealed class AndroidApp : AvaloniaAndroidApplication<App>
{
    public AndroidApp(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        GalleryLogos.Source = new AssetLogoSource();
        // The screenshots and the pages' text are not in the APK (35 MB): they come from the site and are kept in the cache folder.
        var files = new SiteFiles();
        GalleryShots.Source = new SiteShotSource(files);
        GalleryTexts.Source = new SiteTextSource(files);
        ChoroplethExplorerDemoData.ReadMap = file => files.Get("content/Demos/Blocks/ChoroplethExplorer/Maps/" + file, refresh: false) is { } bytes
            ? System.Text.Encoding.UTF8.GetString(bytes)
            : null;
        // Chinese and English only: the Traditional Chinese pages would be one more language to fetch and to keep.
        GalleryLanguages.Restrict("en-US", "zh-CN");
        return base.CustomizeAppBuilder(builder)
                   .WithInterFont()
                   .WithArcFonts()
                   .WithArcPixelFonts()
                   .WithCjkFont()
                   .UseArc(arc => arc
                       .UseLanguages(GalleryLanguages.Initial([CultureInfo.CurrentUICulture.Name]), GalleryLanguages.Tags)
                       .UseArcControls()
                       .UseArcGallery());
    }
}

[Activity(Label = "TKCode Gallery", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light.NoActionBar",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.Density)]
public sealed class MainActivity : AvaloniaMainActivity
{
}
