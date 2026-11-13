using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class OnScreenKeyboardPinPadDemo : UserControl
{
    private string _pin = string.Empty;

    public OnScreenKeyboardPinPadDemo()
    {
        InitializeComponent();

        // No text box here, so the PIN never shows as text. Every key is handled: the keypad itself types nothing.
        Keypad.ItemInvoked += (_, e) =>
        {
            e.Handled = true;
            switch (e.Item.Kind)
            {
                case OnScreenKeyboardKeyKind.Text when _pin.Length < Dots.Children.Count:
                    _pin += e.Text;
                    Prompt.Text = "Enter your 4-digit PIN";
                    break;
                case OnScreenKeyboardKeyKind.Backspace when _pin.Length > 0:
                    _pin = _pin[..^1];
                    break;
                case OnScreenKeyboardKeyKind.Enter:
                    Prompt.Text = _pin == "2468" ? "Unlocked" : _pin.Length < Dots.Children.Count ? "The PIN has 4 digits" : "Wrong PIN, try again";
                    _pin = string.Empty;
                    break;
            }
            for (var i = 0; i < Dots.Children.Count; i++)
            {
                Dots.Children[i].Classes.Set("filled", i < _pin.Length);
            }
        };
    }
}
