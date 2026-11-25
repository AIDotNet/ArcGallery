using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Gallery.Localization;

namespace TKCode.Arc.Gallery.Shell;

/// <summary>
/// Keeps the local calendar date (<c>yyyy-MM-dd</c>) on which the templates popup was last shown: local storage in the
/// browser, a small file on desktop and Android. A host sets <see cref="GalleryTemplatesPromo.Store"/>; without one
/// (tests and screenshot tools) the shell does not open the popup automatically.
/// </summary>
public interface IGalleryTemplatesPromoStore
{
    /// <summary>The date (<c>yyyy-MM-dd</c>) when the popup was last shown, or null when it has not been shown.</summary>
    string? ReadLastShownDate();

    /// <summary>Remembers <paramref name="date"/> (<c>yyyy-MM-dd</c>) so the popup does not appear again today.</summary>
    void WriteLastShownDate(string date);
}

/// <summary>
/// A once-per-day dialog pointing visitors to <see cref="SiteUrl"/> for more free TKCode.Arc starter templates.
/// </summary>
public static class GalleryTemplatesPromo
{
    /// <summary>Where the free TKCode.Arc rapid development templates live.</summary>
    public static Uri SiteUrl { get; } = new("https://tkcode.si/");

    /// <summary>Where the last shown date is kept; set by each host before creating <see cref="GalleryShell"/>.</summary>
    public static IGalleryTemplatesPromoStore? Store { get; set; }

    /// <summary>Current local time; replaced in tests to advance days.</summary>
    internal static Func<DateTimeOffset> Clock { get; set; } = static () => DateTimeOffset.Now;

    /// <summary>How the external link is opened; replaced in tests to capture the target URL.</summary>
    internal static Action<Control, Uri> Opener { get; set; } = GallerySite.Open;

    private static string? s_lastShownDateInSession;

    /// <summary>Today's local calendar date key (<c>yyyy-MM-dd</c>).</summary>
    public static string TodayKey() => Clock().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Whether the popup should appear now: true only when a host configured <see cref="Store"/> and the popup has not
    /// been shown yet today (neither in this session nor in persistent storage).
    /// </summary>
    public static bool ShouldShowToday()
    {
        if (Store is null)
        {
            return false;
        }
        var today = TodayKey();
        if (string.Equals(s_lastShownDateInSession, today, StringComparison.Ordinal))
        {
            return false;
        }
        return !string.Equals(ReadStoredDate(), today, StringComparison.Ordinal);
    }

    /// <summary>Records today as the last shown date in memory and in <see cref="Store"/>.</summary>
    public static void MarkShownToday()
    {
        var today = TodayKey();
        s_lastShownDateInSession = today;
        try
        {
            Store?.WriteLastShownDate(today);
        }
        catch (Exception)
        {
            // Blocked browser storage or a read-only profile still keeps the in-session guard for today.
        }
    }

    /// <summary>Forgets the in-session guard (tests).</summary>
    internal static void ResetSession() => s_lastShownDateInSession = null;

    /// <summary>Builds the dialog that introduces <see cref="SiteUrl"/> and opens it on confirmation.</summary>
    public static Dialog CreateDialog(Control owner)
    {
        var dialog = new Dialog { Size = ControlSize.Medium };
        GalleryText.Bind(dialog, ModalOverlay.TitleProperty, GalleryShellLangResourceKind.TemplatesPromoTitle);
        GalleryText.Bind(dialog, ModalOverlay.DescriptionProperty, GalleryShellLangResourceKind.TemplatesPromoDescription);

        var iconTile = new Border
        {
            Width = 38,
            Height = 38,
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = GalleryUi.Glyph(ArcIcons.LayoutTemplate, 18, ArcDesignTokenKind.Foreground)
        };
        ArcTokens.Bind(iconTile, Border.BackgroundProperty, ArcDesignTokenKind.Surface);
        ArcTokens.Bind(iconTile, Border.BorderBrushProperty, ArcDesignTokenKind.BorderSubtle);
        ArcTokens.Bind(iconTile, Border.CornerRadiusProperty, ArcDesignTokenKind.RadiusControl);

        var siteName = GalleryUi.Text("tkcode.si", ArcDesignTokenKind.Foreground, 14, FontWeight.SemiBold);
        siteName.VerticalAlignment = VerticalAlignment.Center;

        var badge = new Badge
        {
            Tone = BadgeTone.Info,
            Icon = new ArcGlyph { Data = ArcIcons.Sparkles, Size = 12 },
            VerticalAlignment = VerticalAlignment.Center
        };
        GalleryText.Bind(badge, ContentControl.ContentProperty, GalleryShellLangResourceKind.TemplatesPromoBadge);

        var titleRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };
        titleRow.Children.Add(siteName);
        titleRow.Children.Add(badge);

        var address = GalleryUi.Mono(SiteUrl.AbsoluteUri, ArcDesignTokenKind.TextSecondary, 12.5);
        address.TextTrimming = TextTrimming.CharacterEllipsis;

        var info = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        info.Children.Add(titleRow);
        info.Children.Add(address);

        var external = GalleryUi.Glyph(ArcIcons.ArrowUpRight, 16, ArcDesignTokenKind.TextMuted);
        external.VerticalAlignment = VerticalAlignment.Center;

        var cardRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 12
        };
        Grid.SetColumn(info, 1);
        Grid.SetColumn(external, 2);
        cardRow.Children.Add(iconTile);
        cardRow.Children.Add(info);
        cardRow.Children.Add(external);

        var card = GallerySite.Panel(cardRow, ArcDesignTokenKind.SurfaceMuted, 14);
        card.Cursor = new Cursor(StandardCursorType.Hand);
        card.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Opener(owner, SiteUrl);
                dialog.Close(true);
            }
        };

        var note = GallerySite.Text(GalleryShellLangResourceKind.TemplatesPromoNote, ArcDesignTokenKind.TextMuted, 12);

        var body = new StackPanel { Spacing = 12 };
        body.Children.Add(card);
        body.Children.Add(note);
        dialog.Content = body;

        var later = new Button
        {
            Variant = ButtonVariant.Secondary,
            Content = GallerySite.Labelled(GalleryShellLangResourceKind.TemplatesPromoLater, null)
        };
        Dialog.SetResult(later, false);

        var visit = new Button
        {
            Variant = ButtonVariant.Primary,
            Content = GallerySite.Labelled(GalleryShellLangResourceKind.TemplatesPromoAction, ArcIcons.ArrowUpRight)
        };
        visit.Click += (_, _) => Opener(owner, SiteUrl);
        Dialog.SetResult(visit, true);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        actions.Children.Add(later);
        actions.Children.Add(visit);
        dialog.Actions = actions;

        return dialog;
    }

    private static string? ReadStoredDate()
    {
        try
        {
            return Store?.ReadLastShownDate()?.Trim();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
