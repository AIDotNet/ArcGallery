namespace TKCode.Arc.A2UI.Chat;

public static class ChatBuilderExtensions
{
    /// <summary>Registers what the A2UI Chat app uses: the Arc controls, the A2UI view and the Special components.</summary>
    public static ArcBuilder UseArcA2UIChat(this ArcBuilder builder)
        => builder.UseArcControls().UseArcA2UI().UseArcSpecial();
}
