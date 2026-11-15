using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using TKCode.Arc.DesignTokens;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>The developer inspector: A2UI traffic, the surfaces' data models and the errors.</summary>
internal sealed class ChatInspectorView : Border
{
    private static readonly JsonSerializerOptions s_pretty = new() { WriteIndented = true };
    private readonly SelectableTextBlock _messages = Mono();
    private readonly SelectableTextBlock _data = Mono();
    private readonly SelectableTextBlock _errors = Mono();
    private ChatConversation? _conversation;

    public ChatInspectorView()
    {
        var tabs = new Tabs();
        tabs.Items.Add(new TabItem { Header = ChatStrings.T("Messages"), Content = Scroll(_messages) });
        tabs.Items.Add(new TabItem { Header = ChatStrings.T("Data"), Content = Scroll(_data) });
        tabs.Items.Add(new TabItem { Header = ChatStrings.T("Errors"), Content = Scroll(_errors) });
        tabs.SelectedIndex = 0;
        tabs.SelectionChanged += (_, _) => Refresh();
        Child = tabs;
        Padding = new Thickness(12);
        BorderThickness = new Thickness(1, 0, 0, 0);
        ArcTokens.Bind(this, BorderBrushProperty, ArcDesignTokenKind.BorderSubtle);
        ArcTokens.Bind(this, BackgroundProperty, ArcDesignTokenKind.Background);
    }

    public ChatConversation? Conversation
    {
        get => _conversation;
        set
        {
            if (_conversation is not null)
            {
                _conversation.Log.CollectionChanged -= OnLog;
            }

            _conversation = value;
            if (value is not null)
            {
                value.Log.CollectionChanged += OnLog;
            }

            Refresh();
        }
    }

    private void OnLog(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        if (_conversation is null)
        {
            _messages.Text = _data.Text = _errors.Text = string.Empty;
            return;
        }

        var log = _conversation.Log.TakeLast(200).ToList();
        _messages.Text = string.Join("\n\n", log.Select(l => (l.Direction == "in" ? "→ " : "← ") + l.Json));
        _errors.Text = string.Join("\n\n", log.Where(l => l.Json.Contains("\"error\"", StringComparison.Ordinal)).Select(l => Pretty(l.Json)));
        var data = new JsonObject();
        foreach (var surface in _conversation.Renderer.Surfaces)
        {
            data[surface.Id] = surface.DataModel.Get("/");
        }

        _data.Text = data.ToJsonString(s_pretty);
    }

    private static string Pretty(string json)
    {
        try
        {
            return A2UIJson.Parse(json)?.ToJsonString(s_pretty) ?? json;
        }
        catch (A2UIException)
        {
            return json;
        }
    }

    private static ScrollViewer Scroll(Control content) => new() { Content = content, Margin = new Thickness(0, 8, 0, 0), AllowAutoHide = false };

    private static SelectableTextBlock Mono()
    {
        var block = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap };
        ArcTokens.Bind(block, TextBlock.FontFamilyProperty, ArcDesignTokenKind.MonoFontFamily);
        ArcTokens.Bind(block, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeXS);
        ArcTokens.Bind(block, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        return block;
    }
}
