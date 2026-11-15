using Avalonia.Controls;
using TKCode.Arc.A2UI.Chat;

namespace TKCode.Arc.Gallery.Shell;

/// <summary>
/// The A2UI Chat app inside the gallery (top bar → A2UI): the same app as a2ui.tkcode.si, in its own layout. Hosts set
/// <see cref="Storage"/> to keep settings and conversations (the desktop host: a folder; otherwise this visit only).
/// </summary>
public static class GalleryA2UI
{
    /// <summary>Where the app keeps its settings and conversations.</summary>
    public static IChatStorage Storage { get; set; } = new MemoryChatStorage();

    internal static Control CreateApp(Action back)
    {
        var state = new ChatAppState(Storage);
        state.LoadConversations();
        var app = new A2UIChatApp(state) { ShowBackToGallery = true };
        app.BackRequested += (_, _) => back();
        return app;
    }
}
