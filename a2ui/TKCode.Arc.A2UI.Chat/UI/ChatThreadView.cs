using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Icons.Lucide;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The conversation column: user messages as bubbles on the right, assistant answers full width with Markdown,
/// A2UI surfaces, reasoning, tool calls and notices in the order they arrived. Follows the bottom while streaming
/// unless the user scrolled up.
/// </summary>
internal sealed class ChatThreadView : ScrollViewer
{
    private readonly StackPanel _column = new() { Spacing = 20, Margin = new Thickness(24, 24, 24, 160) };
    private ChatConversation? _conversation;
    private bool _follow = true;

    // Without this the subclass finds no ScrollViewer theme, gets a bare template and cannot scroll at all.
    protected override Type StyleKeyOverride => typeof(ScrollViewer);

    public ChatThreadView()
    {
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        // Long answers (dashboards, forms) must show that there is more below: keep the bar visible.
        AllowAutoHide = false;
        Content = new Border { Child = _column, MaxWidth = 808, HorizontalAlignment = HorizontalAlignment.Stretch };
        ScrollChanged += (_, e) =>
        {
            if (e.ExtentDelta.Y == 0 && e.ViewportDelta.Y == 0)
            {
                _follow = Offset.Y + Viewport.Height >= Extent.Height - 32;
            }
        };
    }

    public bool DeveloperMode { get; set; }

    public ChatConversation? Conversation
    {
        get => _conversation;
        set
        {
            if (_conversation is not null)
            {
                _conversation.Turns.CollectionChanged -= OnTurnsChanged;
            }

            _conversation = value;
            _column.Children.Clear();
            if (value is null)
            {
                return;
            }

            foreach (var turn in value.Turns)
            {
                _column.Children.Add(Build(turn));
            }

            value.Turns.CollectionChanged += OnTurnsChanged;
            _follow = true;
            FollowSoon();
        }
    }

    private void OnTurnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            foreach (ChatTurn turn in e.NewItems)
            {
                _column.Children.Add(Build(turn));
            }

            _follow = true;
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _column.Children.Clear();
        }

        FollowSoon();
    }

    internal void FollowSoon()
    {
        if (_follow)
        {
            Dispatcher.UIThread.Post(ScrollToEnd, DispatcherPriority.Background);
        }
    }

    private Control Build(ChatTurn turn) => turn switch
    {
        UserTurn { IsAction: true } action => ActionChip(action.Text),
        UserTurn user => UserBubble(user.Text),
        AssistantTurn assistant => new AssistantView(assistant, this),
        _ => new Panel()
    };

    private static Control UserBubble(string text)
    {
        var block = new SelectableTextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        ArcTokens.Bind(block, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeBase);
        ArcTokens.Bind(block, TextBlock.ForegroundProperty, ArcDesignTokenKind.Foreground);
        var bubble = new Border { Child = block, Padding = new Thickness(14, 10), MaxWidth = 560, HorizontalAlignment = HorizontalAlignment.Right };
        ArcTokens.Bind(bubble, Border.BackgroundProperty, ArcDesignTokenKind.SurfaceMuted);
        return bubble.BindCornerRadius(ArcDesignTokenKind.RadiusStep16);
    }

    private static Control ActionChip(string text)
    {
        var icon = new LucideIcon { Kind = LucideIconKind.MousePointerClick, Width = 14, Height = 14 };
        ArcTokens.Bind(icon, ArcGlyph.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        var label = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        ArcTokens.Bind(label, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        ArcTokens.Bind(label, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Children = { icon, label } };
        var chip = new Border { Child = row, Padding = new Thickness(10, 4), HorizontalAlignment = HorizontalAlignment.Right, BorderThickness = new Thickness(1) };
        ArcTokens.Bind(chip, Border.BorderBrushProperty, ArcDesignTokenKind.BorderSubtle);
        return chip.BindCornerRadius(ArcDesignTokenKind.RadiusPill);
    }

    /// <summary>One assistant answer.</summary>
    private sealed class AssistantView : StackPanel
    {
        private readonly AssistantTurn _turn;
        private readonly ChatThreadView _thread;
        private readonly TextBlock _working = new() { Text = ChatStrings.T("Thinking…") };

        public AssistantView(AssistantTurn turn, ChatThreadView thread)
        {
            _turn = turn;
            _thread = thread;
            Spacing = 12;
            ArcTokens.Bind(_working, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextMuted);
            foreach (var segment in turn.Segments)
            {
                Children.Add(Build(segment));
            }

            turn.Segments.CollectionChanged += (_, e) =>
            {
                if (e.NewItems is null)
                {
                    return;
                }

                foreach (ChatSegment segment in e.NewItems)
                {
                    Children.Insert(Children.Contains(_working) ? Children.Count - 1 : Children.Count, Build(segment));
                }

                UpdateWorking();
                _thread.FollowSoon();
            };
            turn.StreamingChanged += (_, _) => UpdateWorking();
            UpdateWorking();
        }

        private void UpdateWorking()
        {
            var show = _turn.IsStreaming && _turn.Segments.Count == 0;
            if (show && !Children.Contains(_working))
            {
                Children.Add(_working);
            }
            else if (!show)
            {
                Children.Remove(_working);
            }
        }

        private Control Build(ChatSegment segment) => segment switch
        {
            MarkdownSegment md => MarkdownView(md),
            SurfaceSegment surface => SurfaceView(surface),
            ReasoningSegment reasoning => ReasoningView(reasoning),
            ToolSegment tool => ToolView(tool),
            NoticeSegment notice => NoticeView(notice),
            _ => new Panel()
        };

        private Control MarkdownView(MarkdownSegment segment)
        {
            var markdown = new Markdown();
            if (segment.IsClosed)
            {
                markdown.Text = segment.Text;
            }
            else
            {
                markdown.Append(segment.Text);
                segment.Appended += text =>
                {
                    markdown.Append(text);
                    _thread.FollowSoon();
                };
                segment.Closed += (_, _) => markdown.Complete();
            }

            return markdown;
        }

        private Control SurfaceView(SurfaceSegment segment)
        {
            var conversation = _thread.Conversation!;
            return new A2UISurfaceView
            {
                Renderer = conversation.Renderer,
                SurfaceId = segment.SurfaceId,
                Registry = ChatAppState.Registry,
                ShowErrorPlaceholders = _thread.DeveloperMode,
                Margin = new Thickness(0, 4)
            };
        }

        private static Control ReasoningView(ReasoningSegment segment)
        {
            var block = new ReasoningBlock();
            void Sync()
            {
                block.Reasoning = segment.Text;
                block.Status = segment.IsDone ? ReasoningStatus.Done : ReasoningStatus.Thinking;
                block.Elapsed = segment.IsDone ? segment.Elapsed : null;
            }

            Sync();
            segment.Changed += (_, _) => Sync();
            return block;
        }

        private static Control ToolView(ToolSegment segment)
        {
            var icon = new LucideIcon { Kind = LucideIconKind.Wrench, Width = 14, Height = 14 };
            ArcTokens.Bind(icon, ArcGlyph.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
            var title = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            ArcTokens.Bind(title, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);
            ArcTokens.Bind(title, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
            var details = new SelectableTextBlock { TextWrapping = TextWrapping.Wrap, IsVisible = false, Margin = new Thickness(22, 4, 0, 0) };
            ArcTokens.Bind(details, TextBlock.FontFamilyProperty, ArcDesignTokenKind.MonoFontFamily);
            ArcTokens.Bind(details, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeXS);
            ArcTokens.Bind(details, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
            var header = new Button
            {
                Variant = ButtonVariant.Ghost,
                Size = ControlSize.Small,
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { icon, title } }
            };
            header.Click += (_, _) => details.IsVisible = !details.IsVisible;
            void Sync()
            {
                title.Text = segment.Name + (segment.IsDone ? segment.IsError ? " · failed" : " · done" : " · running…");
                details.Text = "args: " + (segment.Arguments ?? "…") + (segment.Result is { } r ? "\nresult: " + (r.Length > 2000 ? r[..2000] + "…" : r) : string.Empty);
            }

            Sync();
            segment.Changed += (_, _) => Sync();
            return new StackPanel { Children = { header, details } };
        }

        private static Control NoticeView(NoticeSegment notice)
        {
            var text = new SelectableTextBlock { Text = notice.Text + (notice.Details is { Length: > 0 } d && notice.Kind != NoticeKind.Info ? " (" + d + ")" : string.Empty), TextWrapping = TextWrapping.Wrap };
            ArcTokens.Bind(text, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);
            ArcTokens.Bind(text, TextBlock.ForegroundProperty, notice.Kind switch
            {
                NoticeKind.Error => ArcDesignTokenKind.Danger,
                NoticeKind.Warning => ArcDesignTokenKind.Warning,
                _ => ArcDesignTokenKind.TextMuted
            });
            if (notice.Kind == NoticeKind.Info)
            {
                return text;
            }

            var border = new Border { Child = text, Padding = new Thickness(12, 8), BorderThickness = new Thickness(1) };
            ArcTokens.Bind(border, Border.BorderBrushProperty, notice.Kind == NoticeKind.Error ? ArcDesignTokenKind.Danger : ArcDesignTokenKind.Warning);
            ArcTokens.Bind(border, Border.BackgroundProperty, notice.Kind == NoticeKind.Error ? ArcDesignTokenKind.DangerSubtle : ArcDesignTokenKind.WarningSubtle);
            return border.BindCornerRadius(ArcDesignTokenKind.RadiusStep10);
        }
    }
}
