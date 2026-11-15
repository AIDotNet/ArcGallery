using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using TKCode.Arc.DesignTokens;
using TKCode.Arc.Icons.Lucide;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The message box: a rounded card with a growing text area, the model name and a send / stop button. Enter sends,
/// Shift+Enter starts a new line.
/// </summary>
internal sealed class ChatComposerView : Border
{
    private readonly Textarea _input = new() { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MaxHeight = 240, BorderThickness = new Thickness(0) };
    private readonly Button _send = new() { Variant = ButtonVariant.Primary, Size = ControlSize.Small, Width = 32, Height = 32, Padding = new Thickness(0) };
    private readonly TextBlock _model = new() { VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly LucideIcon _sendIcon = new() { Kind = LucideIconKind.ArrowUp, Width = 16, Height = 16 };
    private bool _running;

    public ChatComposerView()
    {
        _input.Placeholder = ChatStrings.T("Reply…");
        _input.Background = Brushes.Transparent;
        _input.IsResizable = false;
        _input.FrameMinHeight = 44;
        // The card is the frame: the text area's own border and fill are turned off.
        _input.TemplateApplied += (_, e) =>
        {
            if (e.NameScope.Find<FieldFrame>("PART_Frame") is { } frame)
            {
                frame.BorderThickness = new Thickness(0);
                frame.Background = Brushes.Transparent;
            }
        };
        _input.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        _input.TextChanged += (_, _) => UpdateSend();
        _send.Content = _sendIcon;
        _send.Click += (_, _) =>
        {
            if (_running)
            {
                StopRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Submit();
            }
        };
        ArcTokens.Bind(_model, TextBlock.ForegroundProperty, ArcDesignTokenKind.TextMuted);
        ArcTokens.Bind(_model, TextBlock.FontSizeProperty, ArcDesignTokenKind.TypeSM);

        var bottom = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(12, 0, 8, 8) };
        bottom.Children.Add(_model);
        Grid.SetColumn(_send, 1);
        bottom.Children.Add(_send);
        Child = new StackPanel { Children = { _input, bottom } };
        Padding = new Thickness(4, 4, 4, 0);
        BorderThickness = new Thickness(1);
        ArcTokens.Bind(this, BackgroundProperty, ArcDesignTokenKind.SurfaceRaised);
        ArcTokens.Bind(this, BorderBrushProperty, ArcDesignTokenKind.Border);
        ArcTokens.Bind(this, BoxShadowProperty, ArcDesignTokenKind.ShadowRaised);
        this.BindCornerRadius(ArcDesignTokenKind.RadiusStep20);
        UpdateSend();
    }

    /// <summary>Raised with the text when the user sends.</summary>
    public event EventHandler<string>? Submitted;

    public event EventHandler? StopRequested;

    public string Placeholder
    {
        get => _input.Placeholder ?? string.Empty;
        set => _input.Placeholder = value;
    }

    public string ModelText
    {
        get => _model.Text ?? string.Empty;
        set => _model.Text = value;
    }

    public bool IsRunning
    {
        get => _running;
        set
        {
            _running = value;
            _sendIcon.Kind = value ? LucideIconKind.Square : LucideIconKind.ArrowUp;
            Avalonia.Automation.AutomationProperties.SetName(_send, ChatStrings.T(value ? "Stop" : "Send"));
            UpdateSend();
        }
    }

    public string Text
    {
        get => _input.Text ?? string.Empty;
        set => _input.Text = value;
    }

    public void FocusInput() => _input.Focus();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            if (!_running)
            {
                Submit();
            }
        }
        else if (e.Key == Key.Escape && _running)
        {
            e.Handled = true;
            StopRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Submit()
    {
        var text = (_input.Text ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return;
        }

        _input.Text = string.Empty;
        Submitted?.Invoke(this, text);
    }

    private void UpdateSend() => _send.IsEnabled = _running || !string.IsNullOrWhiteSpace(_input.Text);
}
