using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Icons.Lucide;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The settings page (it takes the place of the conversation): a header with a close button, a section list
/// (Providers, General, Data) and the chosen section. Providers lists the configured providers beside an editor whose
/// fields are grouped in cards; Save, Test connection and Delete sit in a footer that stays put while the fields scroll.
/// Changes apply when saved.
/// </summary>
internal sealed class ChatSettingsView : DockPanel
{
    private const double NarrowWidth = 760;

    private readonly ChatAppState _state;
    private readonly StackPanel _list = new() { Spacing = 2 };
    private readonly StackPanel _editor = new() { Spacing = 16 };
    private readonly Border _footer = new() { BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(0, 12) };
    private readonly Grid _providersPage = new();
    private readonly ScrollViewer _editorScroll = new() { AllowAutoHide = false };
    private readonly Border _listHost = new();
    private readonly Border _page = new();
    private readonly Panel _nav = new StackPanel { Spacing = 2 };
    private readonly Grid _body = new();
    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.Ordinal);
    private ChatProviderConfig? _editing;
    private string _section = "providers";
    private bool? _narrow;

    // Editor fields.
    private readonly Input _name = new() { Label = ChatStrings.T("Name") };
    private readonly Select _protocol = new() { Label = ChatStrings.T("Protocol") };
    private readonly Input _baseUrl = new() { Label = ChatStrings.T("Base URL") };
    private readonly PasswordField _key = new() { Label = ChatStrings.T("API key") };
    private readonly Checkbox _remember = new()
    {
        Content = ChatStrings.T("Remember the key on this device"),
        Description = ChatStrings.T("Keys stay in memory unless you choose to remember them. A remembered key is stored unencrypted on this device.")
    };
    private readonly Textarea _models = new() { Label = ChatStrings.T("Models (one per line)"), AcceptsReturn = true, FrameMinHeight = 96 };
    private readonly Input _defaultModel = new() { Label = ChatStrings.T("Default model") };
    private readonly Input _maxTokens = new() { Label = ChatStrings.T("Max output tokens"), Placeholder = ChatProviderConfig.DefaultMaxOutputTokens.ToString(CultureInfo.InvariantCulture) };
    private readonly Select _effort = new() { Label = ChatStrings.T("Effort") };
    private readonly Checkbox _thinking = new() { Content = ChatStrings.T("Thinking (summaries)"), Description = ChatStrings.T("Shows a summary of the model's reasoning above the answer.") };
    private readonly Checkbox _fallback = new() { Content = ChatStrings.T("Server-side fallback"), Description = ChatStrings.T("Lets the provider retry on another model when a request is refused.") };
    private readonly Input _proxy = new() { Label = ChatStrings.T("Proxy URL (optional)"), Placeholder = "https://" };
    private readonly Textarea _headers = new() { Label = ChatStrings.T("Extra headers (Name: value)"), AcceptsReturn = true, FrameMinHeight = 72 };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _reasoningOptions = new();

    // General.
    private readonly Checkbox _developer = new() { Content = ChatStrings.T("Developer mode"), Description = ChatStrings.T("Shows the inspector (A2UI messages, data models, errors) and error placeholders in answers.") };
    private readonly Checkbox _autoCorrect = new() { Content = ChatStrings.T("Send interface errors back to the model"), Description = ChatStrings.T("When an answer's interface has errors, the model gets them and fixes its answer.") };
    private readonly Checkbox _fullSchema = new() { Content = ChatStrings.T("Full catalog schema in the system prompt"), Description = ChatStrings.T("More reliable interfaces, longer prompts.") };

    private static readonly string[] s_protocols = ["Anthropic Messages", "OpenAI Chat Completions", "OpenAI Responses"];
    private static readonly string[] s_efforts = ["(default)", "low", "medium", "high"];

    public ChatSettingsView(ChatAppState state)
    {
        _state = state;
        _protocol.ItemsSource = s_protocols;
        _effort.ItemsSource = s_efforts;
        _protocol.SelectionChanged += (_, _) => UpdateProtocolFields();
        ArcTokens.Bind(this, BackgroundProperty, ArcDesignTokenKind.Background);
        ArcTokens.Bind(_footer, Border.BorderBrushProperty, ArcDesignTokenKind.Border);
        ArcTokens.Bind(_footer, Border.BackgroundProperty, ArcDesignTokenKind.Background);
        ArcTokens.Bind(_status, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);
        ArcTokens.Bind(_status, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextSecondary);

        // Header: title and close.
        var title = new TextBlock { Text = ChatStrings.T("Settings"), VerticalAlignment = VerticalAlignment.Center };
        ArcTokens.Bind(title, TextBlock.FontSizeProperty, ArcDesignTokenKind.Type2XL);
        ArcTokens.Bind(title, TextBlock.FontFamilyProperty, ArcDesignTokenKind.DisplayFontFamily);
        var close = new Button { Variant = ButtonVariant.Ghost, Size = ControlSize.Small, Width = 32, Height = 32, Padding = new Thickness(0), Content = new LucideIcon { Kind = LucideIconKind.X, Width = 16, Height = 16 } };
        Avalonia.Automation.AutomationProperties.SetName(close, ChatStrings.T("Close"));
        close.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(24, 20, 16, 12) };
        header.Children.Add(title);
        Grid.SetColumn(close, 1);
        header.Children.Add(close);
        SetDock(header, Dock.Top);
        Children.Add(header);

        // Sections.
        AddNav("providers", LucideIconKind.Plug, ChatStrings.T("Providers"));
        AddNav("general", LucideIconKind.SlidersHorizontal, ChatStrings.T("General"));
        AddNav("data", LucideIconKind.Database, ChatStrings.T("Data"));
        _body.Children.Add(_nav);
        _body.Children.Add(_page);
        Children.Add(_body);

        BuildProvidersPage();
        _developer.IsChecked = _state.Preferences.DeveloperMode;
        _autoCorrect.IsChecked = _state.Preferences.Agent.AutoCorrect;
        _fullSchema.IsChecked = _state.Preferences.Agent.FullSchemaInPrompt;
        foreach (var box in new[] { _developer, _autoCorrect, _fullSchema })
        {
            box.IsCheckedChanged += (_, _) => SaveGeneral();
        }

        ApplyLayout(false);
        ShowSection("providers");
        Refresh();
    }

    public event EventHandler? SavedSettings;

    /// <summary>Raised by the close button (and Escape, handled by the host).</summary>
    public event EventHandler? CloseRequested;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyLayout(e.NewSize.Width < NarrowWidth);
    }

    /// <summary>Wide: section list on the left, provider list beside the editor. Narrow: both stack on top.</summary>
    private void ApplyLayout(bool narrow)
    {
        if (_narrow == narrow)
        {
            return;
        }

        _narrow = narrow;
        _body.ColumnDefinitions = narrow ? new ColumnDefinitions("*") : new ColumnDefinitions("200,*");
        _body.RowDefinitions = narrow ? new RowDefinitions("Auto,*") : new RowDefinitions("*");
        _body.Margin = narrow ? new Thickness(16, 0, 16, 0) : new Thickness(16, 0, 24, 0);
        _body.ColumnSpacing = 24;
        _body.RowSpacing = 12;
        if (_nav is StackPanel navStack)
        {
            navStack.Orientation = narrow ? Orientation.Horizontal : Orientation.Vertical;
        }

        Grid.SetColumn(_page, narrow ? 0 : 1);
        Grid.SetRow(_page, narrow ? 1 : 0);

        _providersPage.ColumnDefinitions = narrow ? new ColumnDefinitions("*") : new ColumnDefinitions("220,*");
        _providersPage.RowDefinitions = narrow ? new RowDefinitions("Auto,*") : new RowDefinitions("*");
        _providersPage.ColumnSpacing = 24;
        _providersPage.RowSpacing = 12;
        var editorColumn = _editorScroll.Parent as Control;
        if (editorColumn is not null)
        {
            Grid.SetColumn(editorColumn, narrow ? 0 : 1);
            Grid.SetRow(editorColumn, narrow ? 1 : 0);
        }
    }

    private void AddNav(string key, LucideIconKind icon, string text)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { new LucideIcon { Kind = icon, Width = 16, Height = 16 }, new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center } } };
        var button = new Button
        {
            Variant = ButtonVariant.Ghost,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Content = row
        };
        button.Click += (_, _) => ShowSection(key);
        _navButtons[key] = button;
        _nav.Children.Add(button);
    }

    private void ShowSection(string key)
    {
        _section = key;
        foreach (var (name, button) in _navButtons)
        {
            button.Variant = name == key ? ButtonVariant.Secondary : ButtonVariant.Ghost;
        }

        _page.Child = key switch
        {
            "general" => Scroll(Page(Section(ChatStrings.T("Agent"), null, _autoCorrect, _fullSchema), Section(ChatStrings.T("Interface"), null, _developer))),
            "data" => Scroll(DataPage()),
            _ => _providersPage
        };
    }

    private static ScrollViewer Scroll(Control content) => new() { Content = content, AllowAutoHide = false };

    private static StackPanel Page(params Control[] sections)
    {
        var page = new StackPanel { Spacing = 16, MaxWidth = 720, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 12, 24) };
        foreach (var section in sections)
        {
            page.Children.Add(section);
        }

        return page;
    }

    /// <summary>A titled card holding a group of fields.</summary>
    private static Border Section(string title, string? description, params Control[] children)
    {
        var stack = new StackPanel { Spacing = 14 };
        var heading = new StackPanel { Spacing = 2 };
        var titleBlock = new TextBlock { Text = title };
        ArcTokens.Bind(titleBlock, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeBase);
        ArcTokens.Bind(titleBlock, TextBlock.FontWeightProperty, ArcDesignTokenKind.WeightMedium);
        heading.Children.Add(titleBlock);
        if (description is not null)
        {
            heading.Children.Add(Muted(description));
        }

        stack.Children.Add(heading);
        foreach (var child in children)
        {
            if (child.Parent is Panel old)
            {
                old.Children.Remove(child);
            }
            else if (child.Parent is Decorator decorator)
            {
                decorator.Child = null;
            }

            stack.Children.Add(child);
        }

        var card = new Border { Child = stack, Padding = new Thickness(20), BorderThickness = new Thickness(1) };
        ArcTokens.Bind(card, Border.BackgroundProperty, ArcDesignTokenKind.SurfaceRaised);
        ArcTokens.Bind(card, Border.BorderBrushProperty, ArcDesignTokenKind.Border);
        card.BindCornerRadius(ArcDesignTokenKind.RadiusStep20);
        return card;
    }

    /// <summary>Two short fields side by side (stacked when narrow).</summary>
    private Grid Pair(Control left, Control right)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 12 };
        // Select and Input differ in height: line the boxes up, not the labels.
        left.VerticalAlignment = VerticalAlignment.Bottom;
        right.VerticalAlignment = VerticalAlignment.Bottom;
        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    private static TextBlock Muted(string text)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        ArcTokens.Bind(block, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextMuted);
        ArcTokens.Bind(block, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);
        return block;
    }

    private void BuildProvidersPage()
    {
        // Provider list and "Add a provider".
        var add = new DropdownMenu { Label = "+ " + ChatStrings.T("Add a provider"), HorizontalAlignment = HorizontalAlignment.Stretch };
        add.Items.Add(MenuItem("Anthropic", () => Add(ChatProviderConfig.AnthropicPreset())));
        add.Items.Add(MenuItem("OpenAI (Responses)", () => Add(ChatProviderConfig.OpenAIResponsesPreset())));
        add.Items.Add(MenuItem("OpenAI (Chat Completions)", () => Add(ChatProviderConfig.OpenAIChatPreset())));
        foreach (var (name, url) in ChatProviderConfig.CompatibleTemplates)
        {
            add.Items.Add(MenuItem(name, () => Add(new ChatProviderConfig { Name = name, Protocol = ChatProtocol.OpenAIChatCompletions, BaseUrl = url })));
        }

        _listHost.Child = new StackPanel { Spacing = 10, Children = { _list, add } };
        _providersPage.Children.Add(_listHost);

        // Editor: scrolling fields over a fixed footer.
        _editorScroll.Content = _editor;
        _editor.Margin = new Thickness(0, 0, 12, 16);
        _editor.MaxWidth = 720;
        _editor.HorizontalAlignment = HorizontalAlignment.Left;
        var test = new Button { Variant = ButtonVariant.Secondary, Content = ChatStrings.T("Test connection") };
        test.Click += async (_, _) => await TestAsync();
        var save = new Button { Variant = ButtonVariant.Primary, Content = ChatStrings.T("Save") };
        save.Click += (_, _) => Save();
        var delete = new Button { Variant = ButtonVariant.Ghost, Content = ChatStrings.T("Delete") };
        delete.Click += (_, _) => DeleteEditing();
        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8 };
        footer.Children.Add(delete);
        Grid.SetColumn(_status, 1);
        footer.Children.Add(_status);
        var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { test, save } };
        Grid.SetColumn(right, 2);
        footer.Children.Add(right);
        _footer.Child = footer;
        var editorColumn = new DockPanel();
        SetDock(_footer, Dock.Bottom);
        editorColumn.Children.Add(_footer);
        editorColumn.Children.Add(_editorScroll);
        _providersPage.Children.Add(editorColumn);
    }

    private Control DataPage()
    {
        var export = new Button { Variant = ButtonVariant.Secondary, Content = ChatStrings.T("Copy all data (without keys)"), HorizontalAlignment = HorizontalAlignment.Left };
        var copied = Muted(string.Empty);
        export.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(_state.Export());
                copied.Text = ChatStrings.T("Copied") + " ✓";
            }
        };
        var clear = new Button { Variant = ButtonVariant.Danger, Content = ChatStrings.T("Clear all data"), HorizontalAlignment = HorizontalAlignment.Left };
        clear.Click += (_, _) =>
        {
            _state.ClearAll();
            _editing = null;
            Refresh();
            SavedSettings?.Invoke(this, EventArgs.Empty);
        };
        return Page(
            Section(ChatStrings.T("Export"), ChatStrings.T("Providers and conversations as JSON. API keys are never included."), export, copied),
            Section(ChatStrings.T("Clear all data"), ChatStrings.T("Deletes every provider, remembered key and conversation on this device."), clear));
    }

    private static MenuEntry MenuItem(string header, Action action) => new() { Header = header, Command = new RelayCommand(action) };

    private void Refresh()
    {
        _list.Children.Clear();
        foreach (var provider in _state.Providers)
        {
            var name = new TextBlock { Text = provider.Name, TextTrimming = TextTrimming.CharacterEllipsis };
            var detail = Muted(provider.Protocol switch
            {
                ChatProtocol.AnthropicMessages => "Anthropic Messages",
                ChatProtocol.OpenAIResponses => "OpenAI Responses",
                _ => "OpenAI Chat"
            } + (provider.Id == _state.ActiveProvider?.Id ? " · " + ChatStrings.T("In use") : string.Empty));
            detail.TextWrapping = TextWrapping.NoWrap;
            detail.TextTrimming = TextTrimming.CharacterEllipsis;
            var button = new Button
            {
                Variant = ReferenceEquals(provider, _editing) ? ButtonVariant.Secondary : ButtonVariant.Ghost,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Height = double.NaN,
                Padding = new Thickness(12, 8),
                Content = new StackPanel { Spacing = 2, Children = { name, detail } }
            };
            button.Click += (_, _) =>
            {
                Edit(provider);
                Refresh();
            };
            _list.Children.Add(button);
        }

        if (_editing is null && _state.Providers.Count > 0)
        {
            Edit(_state.Providers[0]);
            Refresh();
        }
        else if (_editing is null)
        {
            _editor.Children.Clear();
            _editor.Children.Add(Section(ChatStrings.T("No provider"), ChatStrings.T("Requests go from this device straight to the provider. Endpoints that do not allow browser requests need a proxy.")));
            _footer.IsVisible = false;
        }
    }

    private void Add(ChatProviderConfig config)
    {
        _state.Providers.Add(config);
        _state.ActiveProviderId ??= config.Id;
        _editing = null;
        Edit(config);
        Refresh();
    }

    private void DeleteEditing()
    {
        if (_editing is not { } provider)
        {
            return;
        }

        _state.Providers.Remove(provider);
        if (_state.ActiveProviderId == provider.Id)
        {
            _state.ActiveProviderId = _state.Providers.FirstOrDefault()?.Id;
        }

        _editing = null;
        _state.Save();
        Refresh();
        SavedSettings?.Invoke(this, EventArgs.Empty);
    }

    private void Edit(ChatProviderConfig provider)
    {
        _editing = provider;
        _name.Text = provider.Name;
        _protocol.SelectedIndex = (int)provider.Protocol;
        _baseUrl.Text = provider.BaseUrl;
        _key.Text = provider.ApiKey;
        _remember.IsChecked = provider.RememberKey;
        _models.Text = string.Join("\n", provider.Models);
        _defaultModel.Text = provider.DefaultModel ?? string.Empty;
        _maxTokens.Text = provider.MaxOutputTokens.ToString(CultureInfo.InvariantCulture);
        _effort.SelectedIndex = Math.Max(0, Array.IndexOf(s_efforts, provider.Effort));
        _thinking.IsChecked = provider.Thinking;
        _fallback.IsChecked = provider.ServerFallback;
        _proxy.Text = provider.ProxyUrl ?? string.Empty;
        _headers.Text = string.Join("\n", provider.Headers.Select(h => h.Key + ": " + h.Value));
        _status.Text = string.Empty;

        var fetch = new Button { Variant = ButtonVariant.Secondary, Size = ControlSize.Small, Content = ChatStrings.T("Fetch models"), HorizontalAlignment = HorizontalAlignment.Left };
        fetch.Click += async (_, _) => await FetchModelsAsync();
        _reasoningOptions.Child = null;
        var anthropicOptions = new StackPanel { Spacing = 14 };
        _reasoningOptions.Child = anthropicOptions;
        DetachAll(_thinking, _fallback);
        anthropicOptions.Children.Add(_thinking);
        anthropicOptions.Children.Add(_fallback);
        DetachAll(_name, _protocol, _defaultModel, _maxTokens);

        _editor.Children.Clear();
        _editor.Children.Add(Section(ChatStrings.T("Connection"), null, Pair(_name, _protocol), _baseUrl, _key, _remember));
        _editor.Children.Add(Section(ChatStrings.T("Models"), null, _models, fetch, Pair(_defaultModel, _maxTokens)));
        _editor.Children.Add(Section(ChatStrings.T("Reasoning"), null, _effort, _reasoningOptions));
        _editor.Children.Add(Section(ChatStrings.T("Network"), ChatStrings.T("Requests go from this device straight to the provider. Endpoints that do not allow browser requests need a proxy."), _proxy, _headers));
        _footer.IsVisible = true;
        UpdateProtocolFields();
    }

    private static void DetachAll(params Control[] controls)
    {
        foreach (var control in controls)
        {
            if (control.Parent is Panel panel)
            {
                panel.Children.Remove(control);
            }
        }
    }

    private void UpdateProtocolFields()
    {
        _reasoningOptions.IsVisible = _protocol.SelectedIndex == (int)ChatProtocol.AnthropicMessages;
    }

    private ChatProviderConfig Collect(ChatProviderConfig target)
    {
        target.Name = string.IsNullOrWhiteSpace(_name.Text) ? "Provider" : _name.Text!.Trim();
        target.Protocol = (ChatProtocol)Math.Max(0, _protocol.SelectedIndex);
        target.BaseUrl = (_baseUrl.Text ?? string.Empty).Trim();
        target.ApiKey = (_key.Text ?? string.Empty).Trim();
        target.RememberKey = _remember.IsChecked == true;
        target.Models = (_models.Text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        target.DefaultModel = string.IsNullOrWhiteSpace(_defaultModel.Text) ? target.Models.FirstOrDefault() : _defaultModel.Text!.Trim();
        target.MaxOutputTokens = int.TryParse((_maxTokens.Text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxTokens) && maxTokens >= 1
            ? maxTokens
            : ChatProviderConfig.DefaultMaxOutputTokens;
        target.Effort = _effort.SelectedIndex > 0 ? s_efforts[_effort.SelectedIndex] : string.Empty;
        target.Thinking = _thinking.IsChecked == true;
        target.ServerFallback = _fallback.IsChecked == true;
        target.ProxyUrl = string.IsNullOrWhiteSpace(_proxy.Text) ? null : _proxy.Text!.Trim();
        target.Headers.Clear();
        foreach (var line in (_headers.Text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var colon = line.IndexOf(':');
            if (colon > 0)
            {
                target.Headers[line[..colon].Trim()] = line[(colon + 1)..].Trim();
            }
        }

        return target;
    }

    /// <summary>General preferences apply at once (they are switches, not a form).</summary>
    private void SaveGeneral()
    {
        _state.Preferences.DeveloperMode = _developer.IsChecked == true;
        _state.Preferences.Agent.AutoCorrect = _autoCorrect.IsChecked == true;
        _state.Preferences.Agent.FullSchemaInPrompt = _fullSchema.IsChecked == true;
        _state.Save();
        SavedSettings?.Invoke(this, EventArgs.Empty);
    }

    private void Save()
    {
        if (_editing is not null)
        {
            Collect(_editing);
            if (OperatingSystem.IsBrowser() && Uri.TryCreate(_editing.BaseUrl, UriKind.Absolute, out var uri) && uri.Scheme == "http"
                && uri.Host is not ("localhost" or "127.0.0.1"))
            {
                _status.Text = "A browser page served over https cannot call an http endpoint other than localhost. Use https or a proxy.";
                return;
            }
        }

        _state.Save();
        _status.Text = ChatStrings.T("Saved") + " ✓";
        Refresh();
        SavedSettings?.Invoke(this, EventArgs.Empty);
    }

    private async Task FetchModelsAsync()
    {
        if (_editing is null)
        {
            return;
        }

        var config = Collect(_editing.Clone());
        _status.Text = "…";
        try
        {
            var models = await HttpChatProvider.Create(config).ListModelsAsync(CancellationToken.None);
            _models.Text = string.Join("\n", models.Select(m => m.Id));
            _status.Text = $"{models.Count} models";
        }
        catch (ChatProviderException ex)
        {
            _status.Text = ex.Message;
        }
    }

    private async Task TestAsync()
    {
        if (_editing is null)
        {
            return;
        }

        var config = Collect(_editing.Clone());
        config.MaxOutputTokens = 16;
        config.Thinking = false;
        config.Effort = string.Empty;
        var model = config.DefaultModel ?? config.Models.FirstOrDefault();
        if (string.IsNullOrEmpty(model))
        {
            _status.Text = "Add a model first.";
            return;
        }

        _status.Text = "…";
        var started = DateTime.UtcNow;
        try
        {
            var request = new ChatRequest { Model = model, System = "Reply with OK.", Messages = [ChatMessage.User("ping")] };
            await foreach (var evt in HttpChatProvider.Create(config).StreamAsync(request, CancellationToken.None))
            {
                if (evt is ChatCompleted)
                {
                    break;
                }
            }

            _status.Text = $"{ChatStrings.T("Connected")} · {(DateTime.UtcNow - started).TotalMilliseconds:0} ms";
        }
        catch (ChatProviderException ex)
        {
            _status.Text = ex.Message;
        }
    }
}
