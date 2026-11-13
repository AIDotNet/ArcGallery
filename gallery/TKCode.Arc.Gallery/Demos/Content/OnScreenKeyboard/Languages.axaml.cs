using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class OnScreenKeyboardLanguagesDemo : UserControl
{
    public OnScreenKeyboardLanguagesDemo()
    {
        InitializeComponent();

        // A German QWERTZ keyboard: the built-in QWERTY with its letter rows replaced. The symbols pages are kept.
        var layout = OnScreenKeyboardLayout.CreateQwerty();
        layout.Name = "German";
        var letters = layout.Pages[0].Rows;
        letters[0] = OnScreenKeyboardRow.FromCharacters("qwertzuiopü");
        letters[1] = OnScreenKeyboardRow.FromCharacters("asdfghjklöä");
        var third = OnScreenKeyboardRow.FromCharacters("yxcvbnm");
        third.Keys.Insert(0, new OnScreenKeyboardKey(OnScreenKeyboardKeyKind.Shift, 1.5));
        third.Keys.Add(new OnScreenKeyboardKey("ß") { ShiftText = "ẞ" });
        third.Keys.Add(new OnScreenKeyboardKey(OnScreenKeyboardKeyKind.Backspace, 1.5));
        letters[2] = third;
        Keyboard.Layout = layout;
    }
}
