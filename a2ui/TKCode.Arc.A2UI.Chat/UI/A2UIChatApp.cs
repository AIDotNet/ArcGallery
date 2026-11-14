using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Icons.Lucide;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The A2UI Chat app as one control: a collapsible sidebar (new chat, examples, recent chats, settings), a top bar with
/// the conversation title and the model, the conversation column with the message box, and an optional inspector.
/// Below 768 px the sidebar becomes a drawer.
/// </summary>
public sealed class A2UIChatApp : UserControl
{
    private const double NarrowWidth = 768;
    private readonly ChatAppState _state;
    private readonly Grid _root = new() { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
    private readonly Border _sidebarHost = new();
    private readonly Grid _main = new() { RowDefinitions = new RowDefinitions("52,*") };
    private readonly Panel _content = new();
    private readonly ChatThreadView _thread = new();
    private readonly ChatComposerView _composer = new();
    private readonly ChatInspectorView _inspector = new() { Width = 380, IsVisible = false };
    private readonly TextBlock _title = new() { VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly Select _model = new() { Size = ControlSize.Small, MinWidth = 180, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button _menu = IconButton(LucideIconKind.Menu, "Menu");
    private readonly Button _inspectorToggle = IconButton(LucideIconKind.PanelRight, ChatStrings.T("Inspector"));
    private readonly Drawer _drawer = new() { Side = DrawerSide.Left, ShowHeader = false, PanelSize = 300 };
    // Settings take the place of the conversation (and the inspector) until closed.
    private readonly Border _settings = new() { IsVisible = false };
    private readonly StackPanel _recents = new() { Spacing = 2 };
    private ChatConversation? _current;
    private bool _narrow;
    private bool _syncingModel;

    public A2UIChatApp(ChatAppState state)
    {
        _state = state;
        ArcTokens.Bind(this, BackgroundProperty, ArcDesignTokenKind.Background);

        // Top bar.
        ArcTokens.Bind(_title, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeBase);
        _title.FontWeight = FontWeight.Medium;
        var topBar = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"), Margin = new Thickness(12, 0), ColumnSpacing = 8 };
        topBar.Children.Add(_menu);
        Grid.SetColumn(_title, 1);
        topBar.Children.Add(_title);
        Grid.SetColumn(_model, 2);
        topBar.Children.Add(_model);
        Grid.SetColumn(_inspectorToggle, 3);
        topBar.Children.Add(_inspectorToggle);
        _menu.Click += (_, _) => _drawer.IsOpen = true;
        _inspectorToggle.Click += (_, _) => _inspector.IsVisible = !_inspector.IsVisible;
        _model.SelectionChanged += (_, _) => OnModelChosen();
        _main.Children.Add(topBar);

        // Conversation and message box.
        _composer.VerticalAlignment = VerticalAlignment.Bottom;
        _composer.HorizontalAlignment = HorizontalAlignment.Stretch;
        _composer.MaxWidth = 760;
        _composer.Margin = new Thickness(16, 0, 16, 16);
        _composer.Submitted += async (_, text) => await SendAsync(text);
        _composer.StopRequested += (_, _) => _current?.Stop();
        Grid.SetRow(_content, 1);
        _main.Children.Add(_content);

        Grid.SetColumn(_main, 1);
        Grid.SetColumn(_inspector, 2);
        _root.Children.Add(_sidebarHost);
        _root.Children.Add(_main);
        _root.Children.Add(_inspector);
        _root.Children.Add(_drawer);
        Grid.SetColumn(_settings, 1);
        Grid.SetColumnSpan(_settings, 2);
        _root.Children.Add(_settings);
        Content = _root;

        _state.Conversations.CollectionChanged += (_, _) => RebuildRecents();
        _state.Changed += (_, _) => SyncModels();
        BuildSidebar();
        SyncModels();
        Show(_state.Conversations.FirstOrDefault(c => c.ShowcaseId is null));
        AddHandler(KeyDownEvent, OnShortcut, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        // Escape closes settings unless a field or popup inside used it first.
        AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Escape && !e.Handled && _settings.IsVisible)
            {
                e.Handled = true;
                CloseSettings();
            }
        }, Avalonia.Interactivity.RoutingStrategies.Bubble);
    }

    /// <summary>Shows a "Back to Arc Gallery" item in the sidebar.</summary>
    public bool ShowBackToGallery
    {
        get => _showBack;
        set
        {
            _showBack = value;
            BuildSidebar();
        }
    }

    private bool _showBack;

    /// <summary>Raised by "Back to Arc Gallery".</summary>
    public event EventHandler? BackRequested;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        var narrow = e.NewSize.Width < NarrowWidth;
        if (narrow != _narrow)
        {
            _narrow = narrow;
            BuildSidebar();
        }
    }

    private static Button IconButton(LucideIconKind kind, string name)
    {
        var icon = new LucideIcon { Kind = kind, Width = 18, Height = 18 };
        ArcTokens.Bind(icon, ArcGlyph.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        var button = new Button { Variant = ButtonVariant.Ghost, Size = ControlSize.Small, Content = icon, Width = 34, Height = 34, Padding = new Thickness(0) };
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        ToolTip.SetTip(button, name);
        return button;
    }

    private static Button RowButton(LucideIconKind kind, string text, Action onClick)
    {
        var icon = new LucideIcon { Kind = kind, Width = 16, Height = 16 };
        ArcTokens.Bind(icon, ArcGlyph.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        var label = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        var button = new Button
        {
            Variant = ButtonVariant.Ghost,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { icon, label } }
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private static TextBlock SectionHeader(string text)
    {
        var block = new TextBlock { Text = text, Margin = new Thickness(12, 16, 12, 4) };
        ArcTokens.Bind(block, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeXS);
        ArcTokens.Bind(block, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextMuted);
        return block;
    }

    private void BuildSidebar()
    {
        var collapsed = _state.Preferences.SidebarCollapsed && !_narrow;
        _menu.IsVisible = _narrow;
        var sidebar = collapsed ? BuildRail() : BuildFull();
        _sidebarHost.Child = null;
        _drawer.Content = null;
        if (_narrow)
        {
            _drawer.Content = sidebar;
            _sidebarHost.IsVisible = false;
        }
        else
        {
            _sidebarHost.Child = sidebar;
            _sidebarHost.IsVisible = true;
            _drawer.IsOpen = false;
        }
    }

    private Control BuildRail()
    {
        var expand = IconButton(LucideIconKind.PanelLeft, "Expand");
        expand.Click += (_, _) => SetCollapsed(false);
        var add = IconButton(LucideIconKind.SquarePen, ChatStrings.T("New chat"));
        add.Click += (_, _) => NewChat();
        var settings = IconButton(LucideIconKind.Settings, ChatStrings.T("Settings"));
        settings.Click += (_, _) => OpenSettings();
        var top = new StackPanel { Spacing = 6, Children = { expand, add } };
        var dock = new DockPanel { Width = 56, Margin = new Thickness(0, 10) };
        DockPanel.SetDock(settings, Dock.Bottom);
        dock.Children.Add(settings);
        dock.Children.Add(top);
        top.HorizontalAlignment = HorizontalAlignment.Center;
        settings.HorizontalAlignment = HorizontalAlignment.Center;
        return Frame(dock);
    }

    private Control BuildFull()
    {
        var brand = new TextBlock { Text = "A2UI", FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0) };
        ArcTokens.Bind(brand, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeLG);
        var collapse = IconButton(LucideIconKind.PanelLeft, "Collapse");
        collapse.Click += (_, _) =>
        {
            if (_narrow)
            {
                _drawer.IsOpen = false;
            }
            else
            {
                SetCollapsed(true);
            }
        };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Height = 52, Margin = new Thickness(0, 0, 8, 0) };
        header.Children.Add(brand);
        Grid.SetColumn(collapse, 1);
        header.Children.Add(collapse);

        var list = new StackPanel { Spacing = 2, Margin = new Thickness(8, 0) };
        list.Children.Add(RowButton(LucideIconKind.SquarePen, ChatStrings.T("New chat"), NewChat));
        list.Children.Add(SectionHeader(ChatStrings.T("Examples")));
        foreach (var showcase in ShowcaseLibrary.All)
        {
            var button = RowButton(LucideIconKind.Sparkles, showcase.Title, () => OpenShowcase(showcase));
            ToolTip.SetTip(button, showcase.Description);
            list.Children.Add(button);
        }

        list.Children.Add(SectionHeader(ChatStrings.T("Recents")));
        if (_recents.Parent is Panel previous)
        {
            previous.Children.Remove(_recents);
        }

        list.Children.Add(_recents);
        RebuildRecents();

        var footer = new StackPanel { Spacing = 2, Margin = new Thickness(8, 8) };
        if (ShowBackToGallery)
        {
            footer.Children.Add(RowButton(LucideIconKind.ArrowLeft, ChatStrings.T("Back to Arc Gallery"), () => BackRequested?.Invoke(this, EventArgs.Empty)));
        }

        footer.Children.Add(RowButton(LucideIconKind.Settings, ChatStrings.T("Settings"), OpenSettings));
        var provider = new TextBlock { Margin = new Thickness(12, 2), TextTrimming = TextTrimming.CharacterEllipsis };
        ArcTokens.Bind(provider, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeXS);
        ArcTokens.Bind(provider, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextMuted);
        provider.Text = _state.ActiveProvider is { } p ? "● " + p.Name : ChatStrings.T("No provider");
        footer.Children.Add(provider);

        var dock = new DockPanel { Width = 272 };
        DockPanel.SetDock(header, Dock.Top);
        DockPanel.SetDock(footer, Dock.Bottom);
        dock.Children.Add(header);
        dock.Children.Add(footer);
        dock.Children.Add(new ScrollViewer { Content = list, AllowAutoHide = false });
        return Frame(dock);
    }

    private Border Frame(Control content)
    {
        var frame = new Border { Child = content, BorderThickness = new Thickness(0, 0, 1, 0) };
        ArcTokens.Bind(frame, Border.BackgroundProperty, ArcDesignTokenKind.SurfaceMuted);
        ArcTokens.Bind(frame, Border.BorderBrushProperty, ArcDesignTokenKind.BorderSubtle);
        return frame;
    }

    private void SetCollapsed(bool collapsed)
    {
        _state.Preferences.SidebarCollapsed = collapsed;
        _state.Save();
        BuildSidebar();
    }

    private void RebuildRecents()
    {
        _recents.Children.Clear();
        // Examples have their own section; Recents lists the user's own conversations.
        foreach (var conversation in _state.Conversations.Where(c => c.ShowcaseId is null))
        {
            // The title is the button's only content (left aligned); the delete button sits on the row beside it and
            // shows while the pointer is over the row, so the title never moves or disappears.
            var label = new TextBlock
            {
                Text = conversation.Title,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            var button = new Button
            {
                Variant = ReferenceEquals(conversation, _current) ? ButtonVariant.Secondary : ButtonVariant.Ghost,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(12, 0, 36, 0),
                Content = label
            };
            button.Click += (_, _) => Show(conversation);
            var delete = IconButton(LucideIconKind.Trash2, ChatStrings.T("Delete"));
            delete.Width = delete.Height = 26;
            delete.Margin = new Thickness(0, 0, 6, 0);
            delete.HorizontalAlignment = HorizontalAlignment.Right;
            delete.VerticalAlignment = VerticalAlignment.Center;
            delete.Opacity = 0;
            delete.Click += (_, _) =>
            {
                var wasCurrent = ReferenceEquals(conversation, _current);
                _state.Delete(conversation);
                if (wasCurrent)
                {
                    Show(_state.Conversations.FirstOrDefault());
                }
            };
            var row = new Panel { Children = { button, delete } };
            row.PointerEntered += (_, _) => delete.Opacity = 1;
            row.PointerExited += (_, _) => delete.Opacity = 0;
            _recents.Children.Add(row);
        }
    }

    private void NewChat()
    {
        Show(null);
        _drawer.IsOpen = false;
        _composer.FocusInput();
    }

    private void OpenShowcase(Showcase showcase)
    {
        _drawer.IsOpen = false;
        Show(_state.OpenShowcase(showcase));
    }

    private void OpenSettings()
    {
        var view = new ChatSettingsView(_state);
        view.SavedSettings += (_, _) =>
        {
            SyncModels();
            BuildSidebar();
            _inspectorToggle.IsVisible = _state.Preferences.DeveloperMode;
            _thread.DeveloperMode = _state.Preferences.DeveloperMode;
        };
        view.CloseRequested += (_, _) => CloseSettings();
        _settings.Child = view;
        _settings.IsVisible = true;
        _drawer.IsOpen = false;
    }

    private void CloseSettings()
    {
        _settings.IsVisible = false;
        _settings.Child = null;
        Show(_current ?? _state.Conversations.FirstOrDefault(c => c.ShowcaseId is null));
    }

    private async Task SendAsync(string text)
    {
        if (_current is null || _current.ShowcaseId is not null && _state.ActiveProvider is not null)
        {
            Show(_state.NewConversation());
        }

        await _current!.SendAsync(text);
        RebuildRecents();
    }

    private void Show(ChatConversation? conversation)
    {
        if (_current is not null)
        {
            _current.RunningChanged -= OnRunningChanged;
        }

        _current = conversation;
        // Opening a conversation (sidebar, new chat, example) leaves the settings page.
        _settings.IsVisible = false;
        _settings.Child = null;
        _content.Children.Clear();
        _thread.DeveloperMode = _state.Preferences.DeveloperMode;
        _inspectorToggle.IsVisible = _state.Preferences.DeveloperMode;
        _inspector.Conversation = conversation;
        if (_composer.Parent is Panel old)
        {
            old.Children.Remove(_composer);
        }

        if (conversation is null)
        {
            _title.Text = string.Empty;
            _content.Children.Add(EmptyState());
        }
        else
        {
            _title.Text = conversation.Title + (conversation.ShowcaseId is not null ? " · " + ChatStrings.T("Example") : string.Empty);
            conversation.RunningChanged += OnRunningChanged;
            _thread.Conversation = conversation;
            _content.Children.Add(_thread);
            _composer.VerticalAlignment = VerticalAlignment.Bottom;
            _content.Children.Add(_composer);
        }

        _composer.IsRunning = conversation?.IsRunning == true;
        SyncModels();
        RebuildRecents();
    }

    private Control EmptyState()
    {
        var greeting = new TextBlock { Text = ChatStrings.T("What can I show you?"), HorizontalAlignment = HorizontalAlignment.Center };
        ArcTokens.Bind(greeting, TextBlock.FontSizeProperty, ArcDesignTokenKind.Type3XL);
        ArcTokens.Bind(greeting, TextBlock.FontFamilyProperty, ArcDesignTokenKind.DisplayFontFamily);
        var description = new TextBlock
        {
            Text = ChatStrings.T("Answers can include live interfaces: forms, choices, lists and charts drawn with Arc."),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        ArcTokens.Bind(description, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);
        _composer.Margin = new Thickness(0);
        _composer.VerticalAlignment = VerticalAlignment.Top;
        _composer.Placeholder = ChatStrings.T("Ask anything…");
        var chips = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
        // The rich examples only; the sidebar lists the small ones as well.
        foreach (var showcase in ShowcaseLibrary.Rich)
        {
            var chip = new Button { Variant = ButtonVariant.Secondary, Size = ControlSize.Small, Content = showcase.Prompt, Margin = new Thickness(4) };
            chip.Click += (_, _) => OpenShowcase(showcase);
            chips.Children.Add(chip);
        }

        var stack = new StackPanel
        {
            Spacing = 20,
            MaxWidth = 760,
            Margin = new Thickness(16),
            VerticalAlignment = VerticalAlignment.Center,
            Children = { greeting, description, _composer, chips }
        };
        if (_state.ActiveProvider is null)
        {
            var add = new Button { Variant = ButtonVariant.Primary, Content = ChatStrings.T("Add a provider"), HorizontalAlignment = HorizontalAlignment.Center };
            add.Click += (_, _) => OpenSettings();
            stack.Children.Insert(2, add);
        }

        return new ScrollViewer { Content = stack, AllowAutoHide = false };
    }

    private void OnRunningChanged(object? sender, EventArgs e)
    {
        _composer.IsRunning = _current?.IsRunning == true;
        if (_current is not null)
        {
            _title.Text = _current.Title + (_current.ShowcaseId is not null ? " · " + ChatStrings.T("Example") : string.Empty);
        }

        _composer.Placeholder = ChatStrings.T("Reply…");
        _composer.Margin = new Thickness(16, 0, 16, 16);
        RebuildRecents();
    }

    private void SyncModels()
    {
        _syncingModel = true;
        var provider = _state.ActiveProvider;
        var items = new List<string>();
        foreach (var p in _state.Providers)
        {
            foreach (var model in p.Models.DefaultIfEmpty(p.DefaultModel ?? string.Empty).Where(m => m.Length > 0))
            {
                items.Add(p.Name + " · " + model);
            }
        }

        _model.ItemsSource = items;
        var currentModel = _state.ActiveModel ?? provider?.DefaultModel ?? provider?.Models.FirstOrDefault();
        _model.SelectedItem = provider is null ? null : items.FirstOrDefault(i => i == provider.Name + " · " + currentModel);
        _model.IsVisible = items.Count > 0;
        _composer.ModelText = _current?.ShowcaseId is not null
            ? ChatStrings.T("Example") + " · replay"
            : provider is null ? ChatStrings.T("No provider") : currentModel ?? string.Empty;
        _syncingModel = false;
    }

    private void OnModelChosen()
    {
        if (_syncingModel || _model.SelectedItem is not string chosen)
        {
            return;
        }

        foreach (var p in _state.Providers)
        {
            var prefix = p.Name + " · ";
            if (chosen.StartsWith(prefix, StringComparison.Ordinal))
            {
                _state.ActiveProviderId = p.Id;
                _state.ActiveModel = chosen[prefix.Length..];
                _composer.ModelText = _state.ActiveModel;
                _state.Save();
                return;
            }
        }
    }

    private void OnShortcut(object? sender, KeyEventArgs e)
    {
        var command = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (command && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.Key == Key.O)
        {
            NewChat();
            e.Handled = true;
        }
        else if (command && e.Key == Key.B && !_narrow)
        {
            SetCollapsed(!_state.Preferences.SidebarCollapsed);
            e.Handled = true;
        }
    }
}
