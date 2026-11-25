using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TKCode.Arc.Gallery.Localization;
using TKCode.Arc.Gallery.Shell;
using TKCode.Arc.Motion;

namespace TKCode.Arc.Tests.Gallery;

/// <summary>The once-per-day templates popup linking to https://tkcode.si/.</summary>
public class GalleryTemplatesPromoTests
{
    private sealed class MemoryPromoStore(string? date = null) : IGalleryTemplatesPromoStore
    {
        public string? LastShownDate { get; private set; } = date;

        public string? ReadLastShownDate() => LastShownDate;

        public void WriteLastShownDate(string date) => LastShownDate = date;
    }

    private sealed class BrokenPromoStore : IGalleryTemplatesPromoStore
    {
        public string ReadLastShownDate() => throw new IOException("blocked");

        public void WriteLastShownDate(string date) => throw new IOException("blocked");
    }

    private static void WithPromo(IGalleryTemplatesPromoStore? store, DateTimeOffset now, Action<List<Uri>> test)
    {
        var oldStore = GalleryTemplatesPromo.Store;
        var oldClock = GalleryTemplatesPromo.Clock;
        var oldOpener = GalleryTemplatesPromo.Opener;
        var opened = new List<Uri>();
        GalleryTemplatesPromo.Store = store;
        GalleryTemplatesPromo.Clock = () => now;
        GalleryTemplatesPromo.Opener = (_, uri) => opened.Add(uri);
        GalleryTemplatesPromo.ResetSession();
        try
        {
            test(opened);
        }
        finally
        {
            GalleryTemplatesPromo.Store = oldStore;
            GalleryTemplatesPromo.Clock = oldClock;
            GalleryTemplatesPromo.Opener = oldOpener;
            GalleryTemplatesPromo.ResetSession();
        }
    }

    private static (GalleryShell Shell, Window Window) ShowShell()
    {
        var shell = new GalleryShell();
        var window = TestHelpers.Show(shell);
        ArcMotion.SetLevel(window, ArcMotionLevel.Off);
        for (var i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
        return (shell, window);
    }

    [AvaloniaFact]
    public void Without_A_Host_Store_The_Shell_Does_Not_Open_The_Popup()
    {
        WithPromo(null, new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero), _ =>
        {
            var (shell, window) = ShowShell();
            try
            {
                shell.TemplatesPromoDialog.ShouldBeNull();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact]
    public void The_Popup_Shows_Once_Per_Day_And_Again_The_Next_Day()
    {
        var store = new MemoryPromoStore();
        var day1 = new DateTimeOffset(2026, 11, 25, 9, 0, 0, TimeSpan.FromHours(8));
        var day1Later = new DateTimeOffset(2026, 11, 25, 22, 30, 0, TimeSpan.FromHours(8));
        var day2 = new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.FromHours(8));

        WithPromo(store, day1, _ =>
        {
            var (shell1, window1) = ShowShell();
            try
            {
                shell1.TemplatesPromoDialog.ShouldNotBeNull();
                shell1.TemplatesPromoDialog!.IsOpen.ShouldBeTrue();
                store.LastShownDate.ShouldBe("2026-11-25");
            }
            finally
            {
                window1.Close();
            }

            // Reopening later the same day (even in a fresh session) does not show the popup again.
            GalleryTemplatesPromo.Clock = () => day1Later;
            GalleryTemplatesPromo.ResetSession();
            var (shell2, window2) = ShowShell();
            try
            {
                shell2.TemplatesPromoDialog.ShouldBeNull();
            }
            finally
            {
                window2.Close();
            }

            // On the next calendar day, the popup opens once again and records the new date.
            GalleryTemplatesPromo.Clock = () => day2;
            var (shell3, window3) = ShowShell();
            try
            {
                shell3.TemplatesPromoDialog.ShouldNotBeNull();
                shell3.TemplatesPromoDialog!.IsOpen.ShouldBeTrue();
                store.LastShownDate.ShouldBe("2026-11-26");
            }
            finally
            {
                window3.Close();
            }
        });
    }

    [AvaloniaFact]
    public void Blocked_Storage_Still_Limits_The_Popup_To_Once_In_The_Session()
    {
        WithPromo(new BrokenPromoStore(), new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero), _ =>
        {
            var (shell1, window1) = ShowShell();
            try
            {
                shell1.TemplatesPromoDialog.ShouldNotBeNull();
                shell1.TemplatesPromoDialog!.IsOpen.ShouldBeTrue();
            }
            finally
            {
                window1.Close();
            }

            var (shell2, window2) = ShowShell();
            try
            {
                shell2.TemplatesPromoDialog.ShouldBeNull();
            }
            finally
            {
                window2.Close();
            }
        });
    }

    [AvaloniaFact]
    public void Primary_Action_Opens_Tkcode_Si_And_Closes_The_Dialog_While_Later_Just_Closes()
    {
        var day = new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);

        WithPromo(new MemoryPromoStore(), day, opened =>
        {
            var (shell, window) = ShowShell();
            try
            {
                var dialog = shell.TemplatesPromoDialog.ShouldNotBeNull();
                var actions = (StackPanel)dialog.Actions!;
                var visit = actions.Children.OfType<Button>().Single(b => b.Variant == ButtonVariant.Primary);
                visit.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();

                opened.ShouldHaveSingleItem().ShouldBe(new Uri("https://tkcode.si/"));
                dialog.IsOpen.ShouldBeFalse();
            }
            finally
            {
                window.Close();
            }
        });

        WithPromo(new MemoryPromoStore(), day, opened =>
        {
            var (shell, window) = ShowShell();
            try
            {
                var dialog = shell.TemplatesPromoDialog.ShouldNotBeNull();
                var actions = (StackPanel)dialog.Actions!;
                var later = actions.Children.OfType<Button>().Single(b => b.Variant == ButtonVariant.Secondary);
                later.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();

                opened.ShouldBeEmpty();
                dialog.IsOpen.ShouldBeFalse();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact]
    public void Dialog_Title_And_Description_Follow_The_Current_Language()
    {
        var day = new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);
        WithPromo(new MemoryPromoStore(), day, _ =>
        {
            var (shell, window) = ShowShell();
            try
            {
                var dialog = shell.TemplatesPromoDialog.ShouldNotBeNull();
                dialog.Title.ShouldBe("More free TKCode.Arc starter templates");

                GalleryText.SetLanguage("zh-CN");
                Dispatcher.UIThread.RunJobs();
                dialog.Title.ShouldBe("更多免费的 TKCode.Arc 快速开发模板");
                dialog.Description!.ShouldContain("tkcode.si");

                GalleryText.SetLanguage("zh-TW");
                Dispatcher.UIThread.RunJobs();
                dialog.Title.ShouldBe("更多免費的 TKCode.Arc 快速開發範本");
            }
            finally
            {
                GalleryText.SetLanguage("en-US");
                Dispatcher.UIThread.RunJobs();
                window.Close();
            }
        });
    }
}
