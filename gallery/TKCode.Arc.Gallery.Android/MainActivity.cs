using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using TKCode.Arc.Fonts;
using TKCode.Arc.Gallery.Localization;
using TKCode.Arc.Gallery.Shell;
using TKCode.Arc.Gallery.Shots;
using TKCode.Arc.Gallery.Content;
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
        return base.CustomizeAppBuilder(builder)
                   .WithInterFont()
                   .WithArcFonts()
                   .WithArcPixelFonts()
                   .WithGalleryShots()
                   .WithGalleryContent()
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
