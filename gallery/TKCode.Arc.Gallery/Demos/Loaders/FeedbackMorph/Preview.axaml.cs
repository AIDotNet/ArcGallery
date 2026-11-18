using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;

namespace TKCode.Arc.Gallery.Demos;

public partial class FeedbackMorphPreviewDemo : UserControl
{
    public FeedbackMorphPreviewDemo()
    {
        InitializeComponent();
        // A made-up send: it takes 1.1 s, then succeeds or fails as the second switch says.
        Morph.SubmitHandler = async (_, cancellation) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1100), cancellation);
            if (Result.SelectedValue as string == "fail")
            {
                throw new InvalidOperationException("The sample send failed on purpose.");
            }
        };
    }

    // "Opens up" moves the button to the bottom of the card, where the panel grows upward.
    private void OnDirectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Morph is null)
        {
            return;
        }
        var up = Direction.SelectedValue as string == "up";
        Morph.Close();
        Morph.Side = up ? FeedbackMorphSide.Top : FeedbackMorphSide.Bottom;
        Morph.VerticalAlignment = up ? VerticalAlignment.Bottom : VerticalAlignment.Top;
    }
}
