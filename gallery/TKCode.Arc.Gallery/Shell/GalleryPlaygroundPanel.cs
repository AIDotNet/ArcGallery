using TKCode.Arc.DesignTokens;
using TKCode.Arc.Gallery.Docs;
using TKCode.Arc.Gallery.Localization;
using TKCode.Arc.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace TKCode.Arc.Gallery.Shell;

/// <summary>
/// The panel next to a playground demo's Preview (design/engineering/gallery-playground.md): one editor for each property
/// of the demo's <c>x:Name="Target"</c> element the demo's <c>.knobs</c> file lists, and the XAML for the element as it is now.
/// Editors write through <see cref="AvaloniaObject.SetValue(AvaloniaProperty, object?, Avalonia.Data.BindingPriority)"/> on the
/// property the generator named, and the panel follows the target when the demo changes a property itself (a click that
/// toggles <c>IsChecked</c>, say). Nothing is found by name or type at run time.
/// </summary>
public sealed class GalleryPlaygroundPanel : Border
{
    private readonly GalleryPlayground _playground;
    private readonly Control _target;
    private readonly List<KnobRow> _rows = new();
    private readonly TextBlock _snippet = GalleryUi.Mono(string.Empty, ArcDesignTokenKind.TextSecondary, 12);
    private readonly TextBlock _changed;
    private readonly Button _reset;
    private readonly Grid _body = new() { RowDefinitions = new RowDefinitions("*,Auto"), RowSpacing = 10 };
    private readonly Control _header;
    private readonly Control _chevron;
    private bool _drawer;
    private bool _open = true;
    private bool _attached;

    public GalleryPlaygroundPanel(GalleryPlayground playground, Control target)
    {
        _playground = playground;
        _target = target;
        CornerRadius = new CornerRadius(20);
        BorderThickness = new Thickness(1);
        Padding = new Thickness(16, 12);
        ArcTokens.Bind(this, BackgroundProperty, ArcDesignTokenKind.Surface);
        ArcTokens.Bind(this, BorderBrushProperty, ArcDesignTokenKind.BorderSubtle);

        var title = GalleryUi.Text(string.Empty, ArcDesignTokenKind.Foreground, 14, FontWeight.SemiBold);
        GalleryText.Bind(title, TextBlock.TextProperty, GalleryShellLangResourceKind.PlaygroundTitle);
        title.VerticalAlignment = VerticalAlignment.Center;
        _changed = GalleryUi.Text(string.Empty, ArcDesignTokenKind.TextMuted, 12);
        _changed.VerticalAlignment = VerticalAlignment.Center;
        _changed.Margin = new Thickness(8, 0, 0, 0);
        _reset = GalleryPreviewTools.IconButton(ArcIcons.RotateCcw, GalleryShellLangResourceKind.PlaygroundReset);
        _reset.Click += (_, _) => ResetAll();
        _chevron = GalleryUi.Glyph(ArcIcons.ChevronDown, 16);
        _chevron.IsVisible = false;

        var heading = new StackPanel { Orientation = Orientation.Horizontal, Children = { title, _changed } };
        var bar = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"), MinHeight = 30 };
        bar.Children.Add(heading);
        Grid.SetColumn(_reset, 1);
        bar.Children.Add(_reset);
        Grid.SetColumn(_chevron, 2);
        bar.Children.Add(_chevron);
        bar.Background = Brushes.Transparent;
        bar.PointerPressed += (_, e) =>
        {
            if (_drawer && !IsInside(e.Source, _reset))
            {
                SetOpen(!_open);
            }
        };
        _header = bar;

        // Room on the right for the scroll bar, which would cover the editors' edge.
        var rows = new StackPanel { Spacing = 14, Margin = new Thickness(0, 0, 14, 0) };
        string? group = null;
        foreach (var knob in playground.Knobs)
        {
            if (knob.Group is { Length: > 0 } heading2 && heading2 != group)
            {
                group = heading2;
                var label = GalleryUi.Text(heading2, ArcDesignTokenKind.TextMuted, 12, FontWeight.SemiBold);
                label.Margin = new Thickness(0, rows.Children.Count == 0 ? 0 : 4, 0, -4);
                rows.Children.Add(label);
            }
            var row = new KnobRow(this, knob, target);
            _rows.Add(row);
            rows.Children.Add(row.View);
        }
        var scroll = new ScrollViewer
        {
            Content = rows,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        scroll.SetValue(ScrollViewer.AllowAutoHideProperty, false);

        // The XAML as plain wrapped text with a copy button: a code block's header and horizontal scroll are too wide for
        // a 280 px column.
        _snippet.TextWrapping = TextWrapping.NoWrap;
        var copy = GalleryPreviewTools.IconButton(ArcIcons.Copy, GalleryShellLangResourceKind.CopyCode);
        copy.Width = 26;
        copy.Height = 26;
        copy.MinHeight = 26;
        copy.VerticalAlignment = VerticalAlignment.Top;
        copy.Click += async (_, _) =>
        {
            await ArcClipboard.CopyTextAsync(this, Xaml);
        };
        var xamlLabel = GalleryUi.Text(string.Empty, ArcDesignTokenKind.TextMuted, 12, FontWeight.SemiBold);
        GalleryText.Bind(xamlLabel, TextBlock.TextProperty, GalleryShellLangResourceKind.PlaygroundXaml);
        var snippetBar = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        snippetBar.Children.Add(xamlLabel);
        xamlLabel.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(copy, 1);
        snippetBar.Children.Add(copy);
        var snippetBox = new Border
        {
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(12),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    snippetBar,
                    // Long lines scroll instead of wrapping (a wrapped indent would break away from its text); a long element
                    // scrolls too, so the XAML never pushes the editors out of the panel.
                    new ScrollViewer
                    {
                        Content = _snippet,
                        MaxHeight = 120,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                }
            }
        };
        ArcTokens.Bind(snippetBox, BackgroundProperty, ArcDesignTokenKind.SurfaceMuted);

        _body.Children.Add(scroll);
        Grid.SetRow(snippetBox, 1);
        _body.Children.Add(snippetBox);
        var content = new Grid { RowDefinitions = new RowDefinitions("Auto,*"), RowSpacing = 10 };
        content.Children.Add(_header);
        Grid.SetRow(_body, 1);
        content.Children.Add(_body);
        Child = content;
        Refresh();
    }

    /// <summary>The playground this panel edits.</summary>
    public GalleryPlayground Playground => _playground;

    /// <summary>The knob editors, in order (for tests).</summary>
    internal IReadOnlyList<KnobRow> Rows => _rows;

    /// <summary>The XAML text now shown.</summary>
    public string Xaml { get; private set; } = string.Empty;

    /// <summary>Whether the panel is a collapsible drawer (narrow pages) rather than a column beside the stage.</summary>
    public bool IsDrawer
    {
        get => _drawer;
        set
        {
            if (_drawer == value)
            {
                return;
            }
            _drawer = value;
            _chevron.IsVisible = value;
            // A drawer starts closed; leaving drawer mode shows everything again.
            SetOpen(!value);
        }
    }

    /// <summary>Whether the drawer is open (always true outside drawer mode).</summary>
    public bool IsOpen => !_drawer || _open;

    public void SetOpen(bool open)
    {
        _open = open;
        _body.IsVisible = open || !_drawer;
        _chevron.RenderTransform = open && _drawer ? new RotateTransform(180) : null;
    }

    /// <summary>Puts every property back to the value the demo starts with.</summary>
    public void ResetAll()
    {
        foreach (var row in _rows)
        {
            row.Reset();
        }
        Refresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_attached)
        {
            _attached = true;
            _target.PropertyChanged += OnTargetPropertyChanged;
            // The demo may have changed a property while the panel was away.
            foreach (var row in _rows)
            {
                row.Follow();
            }
            Refresh();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_attached)
        {
            _attached = false;
            _target.PropertyChanged -= OnTargetPropertyChanged;
        }
    }

    private void OnTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        foreach (var row in _rows)
        {
            if (row.Property == e.Property)
            {
                row.Follow();
                Refresh();
                return;
            }
        }
    }

    /// <summary>Updates the XAML, the count of changed properties and the reset button after a change.</summary>
    internal void Refresh()
    {
        var changed = _rows.Count(r => r.IsChanged);
        _changed.Text = changed == 0 ? string.Empty : "· " + changed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _reset.IsVisible = changed > 0;
        var text = GalleryPlaygroundSnippet.Build(_playground,
            knob => _rows.First(r => r.Knob == knob).Current,
            knob => _rows.First(r => r.Knob == knob).Default);
        Xaml = text;
        _snippet.Text = text;
    }

    private static bool IsInside(object? source, Control ancestor)
    {
        for (var visual = source as Avalonia.Visual; visual is not null; visual = visual.GetVisualParent())
        {
            if (ReferenceEquals(visual, ancestor))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>One property: its label, its editor and the way back to the demo's starting value.</summary>
    internal sealed class KnobRow
    {
        private readonly GalleryPlaygroundPanel _panel;
        private readonly Control _target;
        private readonly Button _undo;
        private readonly object? _initial;
        private Action<object?> _show = static _ => { };
        private bool _writing;

        public KnobRow(GalleryPlaygroundPanel panel, GalleryKnob knob, Control target)
        {
            _panel = panel;
            Knob = knob;
            _target = target;
            Property = knob.Property();
            _initial = target.GetValue(Property);
            Default = Property.GetMetadata(target.GetType()) switch
            {
                IStyledPropertyMetadata styled => styled.DefaultValue,
                // A direct property keeps no default; a value it is "unset" to is the closest thing.
                IDirectPropertyMetadata direct when !ReferenceEquals(direct.UnsetValue, AvaloniaProperty.UnsetValue) => direct.UnsetValue,
                _ => null
            };

            var name = GalleryUi.Mono(knob.Name, ArcDesignTokenKind.TextSecondary, 12);
            name.VerticalAlignment = VerticalAlignment.Center;
            _undo = GalleryPreviewTools.IconButton(ArcIcons.RotateCcw, GalleryShellLangResourceKind.PlaygroundReset);
            _undo.Width = 22;
            _undo.Height = 22;
            _undo.MinHeight = 22;
            _undo.IsVisible = false;
            _undo.Click += (_, _) =>
            {
                Reset();
                _panel.Refresh();
            };
            var line = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            line.Children.Add(name);
            Grid.SetColumn(_undo, 1);
            line.Children.Add(_undo);

            var editor = CreateEditor();
            View = new StackPanel { Spacing = 6, Children = { line, editor } };
            Follow();
        }

        public GalleryKnob Knob { get; }

        public AvaloniaProperty Property { get; }

        public object? Default { get; }

        public Control View { get; }

        public object? Current => _target.GetValue(Property);

        public bool IsChanged => !Equals(Current, _initial);

        /// <summary>Writes <paramref name="value"/> to the target (what an editor does when the visitor changes it).</summary>
        public void Write(object? value)
        {
            if (_writing)
            {
                return;
            }
            _writing = true;
            try
            {
                _target.SetValue(Property, value);
            }
            finally
            {
                _writing = false;
            }
            _undo.IsVisible = IsChanged;
            _panel.Refresh();
        }

        public void Reset()
        {
            Write(_initial);
            Follow();
        }

        /// <summary>Shows the target's value in the editor, after the demo changed it or the panel was attached.</summary>
        public void Follow()
        {
            if (_writing)
            {
                return;
            }
            _writing = true;
            try
            {
                _show(Current);
            }
            finally
            {
                _writing = false;
            }
            _undo.IsVisible = IsChanged;
        }

        private Control CreateEditor()
        {
            switch (Knob.Kind)
            {
                case GalleryKnobKind.Switch:
                    return SwitchEditor();
                case GalleryKnobKind.Choice:
                    return ChoiceEditor();
                case GalleryKnobKind.Number:
                    return NumberEditor();
                default:
                    return TextEditor();
            }
        }

        private Control SwitchEditor()
        {
            var toggle = new Switch { HorizontalAlignment = HorizontalAlignment.Left };
            toggle.PropertyChanged += (_, e) =>
            {
                if (e.Property == ToggleButton.IsCheckedProperty && !_writing)
                {
                    Write(toggle.IsChecked == true);
                }
            };
            _show = value => toggle.IsChecked = value is true;
            return toggle;
        }

        private Control ChoiceEditor()
        {
            var choices = Knob.Choices;
            void Pick(int index)
            {
                if (index >= 0 && index < choices.Length && !_writing)
                {
                    Write(choices[index].Value);
                }
            }
            int IndexOf(object? value)
            {
                for (var i = 0; i < choices.Length; i++)
                {
                    if (Equals(choices[i].Value, value))
                    {
                        return i;
                    }
                }
                return -1;
            }
            if (choices.Length <= 4)
            {
                var segmented = new GallerySegmented(choices.Select(c => c.Name).ToArray());
                segmented.SelectionChanged += Pick;
                _show = value => segmented.SelectedIndex = IndexOf(value);
                return segmented;
            }
            var select = new Select
            {
                ItemsSource = choices.Select(c => new SelectOption(c.Value, c.Name)).ToList(),
                Size = ControlSize.Small
            };
            select.SelectionChanged += (_, _) => Pick(select.SelectedIndex);
            _show = value => select.SelectedIndex = IndexOf(value);
            return select;
        }

        private Control NumberEditor()
        {
            var slider = new Slider
            {
                // The readout beside it shows the value.
                IsValueVisible = false,
                Minimum = Knob.Min,
                Maximum = Knob.Max,
                SmallChange = Knob.Step,
                LargeChange = Knob.Step * 10
            };
            var readout = GalleryUi.Mono(string.Empty, ArcDesignTokenKind.TextMuted, 12);
            readout.VerticalAlignment = VerticalAlignment.Center;
            readout.MinWidth = 36;
            readout.TextAlignment = TextAlignment.End;
            slider.PropertyChanged += (_, e) =>
            {
                if (e.Property != RangeBase.ValueProperty || _writing)
                {
                    return;
                }
                var raw = slider.Value;
                var snapped = Knob.Step > 0 ? Math.Round((raw - Knob.Min) / Knob.Step) * Knob.Step + Knob.Min : raw;
                snapped = Math.Clamp(snapped, Knob.Min, Knob.Max);
                readout.Text = snapped.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
                Write(Knob.IsInteger ? (object)(int)Math.Round(snapped) : snapped);
            };
            _show = value =>
            {
                var number = value is null ? Knob.Min : Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                slider.Value = number;
                readout.Text = number.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            };
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
            row.Children.Add(slider);
            Grid.SetColumn(readout, 1);
            row.Children.Add(readout);
            return row;
        }

        private Control TextEditor()
        {
            var input = new Input { Size = ControlSize.Small };
            input.PropertyChanged += (_, e) =>
            {
                if (e.Property == TextBox.TextProperty && !_writing)
                {
                    Write(input.Text ?? string.Empty);
                }
            };
            _show = value => input.Text = value?.ToString() ?? string.Empty;
            if (Knob.Presets.Length == 0)
            {
                return input;
            }
            var presets = new WrapPanel { ItemSpacing = 4, LineSpacing = 4 };
            foreach (var preset in Knob.Presets)
            {
                // A long suggestion is cut with an ellipsis (its full text is the tooltip) so it stays inside the panel.
                var chip = new Button
                {
                    Content = new TextBlock { Text = preset, TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 200 },
                    Variant = ButtonVariant.Secondary,
                    Size = ControlSize.Small,
                    FontSize = 12
                };
                ToolTip.SetTip(chip, preset);
                chip.Click += (_, _) =>
                {
                    input.Text = preset;
                };
                presets.Children.Add(chip);
            }
            return new StackPanel { Spacing = 6, Children = { input, presets } };
        }
    }
}
