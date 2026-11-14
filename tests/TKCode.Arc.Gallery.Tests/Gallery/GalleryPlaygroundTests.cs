using TKCode.Arc.Gallery.Docs;
using TKCode.Arc.Gallery.Shell;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace TKCode.Arc.Tests.Gallery;

/// <summary>The playground panel beside a hero demo (design/engineering/gallery-playground.md).</summary>
public class GalleryPlaygroundTests
{
    private static (GalleryDemoView View, GalleryPlaygroundPanel Panel, Window Window) Open(string id, double width = 1200)
    {
        var view = new GalleryDemoView(id, hero: true);
        var window = TestHelpers.Show(view);
        window.Width = width;
        window.Height = 900;
        Dispatcher.UIThread.RunJobs();
        var panel = view.Playground.ShouldNotBeNull();
        return (view, panel, window);
    }

    /// <summary>The snippet on one line (it wraps one attribute to a line once it is long).</summary>
    private static string Flat(string xaml) => xaml.Replace("\n    ", " ");

    private static Control Target(GalleryDemoView view) => view.View.ShouldNotBeNull().FindControl<Control>("Target").ShouldNotBeNull();

    [AvaloniaFact]
    public void Every_Playground_Has_A_Target_And_Knobs_That_Can_Be_Set()
    {
        GalleryPlaygrounds.All.ShouldNotBeEmpty();
        foreach (var playground in GalleryPlaygrounds.All)
        {
            var demo = GalleryDemos.Get(playground.DemoId).ShouldNotBeNull(playground.DemoId);
            var root = demo.Create();
            var target = root.FindControl<Control>("Target").ShouldNotBeNull(playground.DemoId);
            playground.Knobs.ShouldNotBeEmpty(playground.DemoId);
            foreach (var knob in playground.Knobs)
            {
                var property = knob.Property();
                property.IsAttached.ShouldBeFalse(knob.Name);
                property.Name.ShouldBe(knob.Name);
                var before = target.GetValue(property);
                if (knob.Kind == GalleryKnobKind.Choice)
                {
                    knob.Choices.ShouldNotBeEmpty(knob.Name);
                    foreach (var choice in knob.Choices)
                    {
                        target.SetValue(property, choice.Value);
                        target.GetValue(property).ShouldBe(choice.Value);
                    }
                }
                target.SetValue(property, before);
            }
        }
    }

    [AvaloniaFact]
    public void The_Hero_Of_A_Playground_Demo_Gets_The_Panel_With_The_Listed_Knobs_In_Order()
    {
        var (view, panel, _) = Open("Button.Playground");
        panel.Rows.Select(r => r.Knob.Name).ShouldBe(["Variant", "Size", "Content", "IsLoading", "IsEnabled"]);
        view.Playground.ShouldBeSameAs(panel);
        // Another example of the page has none.
        new GalleryDemoView("Button.Sizes").Playground.ShouldBeNull();
    }

    [AvaloniaFact]
    public void An_Editor_Changes_The_Real_Control_And_The_Xaml_Follows()
    {
        var (view, panel, _) = Open("Button.Playground");
        var button = (Button)Target(view);
        Flat(panel.Xaml).ShouldBe("<arc:Button Content=\"Save changes\" />");

        var variant = panel.Rows.First(r => r.Knob.Name == "Variant");
        variant.Write(ButtonVariant.Danger);
        button.Variant.ShouldBe(ButtonVariant.Danger);
        Flat(panel.Xaml).ShouldBe("<arc:Button Content=\"Save changes\" Variant=\"Danger\" />");

        panel.Rows.First(r => r.Knob.Name == "IsLoading").Write(true);
        button.IsLoading.ShouldBeTrue();
        panel.Xaml.ShouldContain("IsLoading=\"True\"");

        panel.Rows.First(r => r.Knob.Name == "Content").Write("Delete project");
        button.Content.ShouldBe("Delete project");
        panel.Xaml.ShouldContain("Content=\"Delete project\"");
    }

    [AvaloniaFact]
    public void The_Panel_Follows_A_Property_The_Demo_Changes_Itself()
    {
        var (view, panel, _) = Open("Button.Playground");
        var button = (Button)Target(view);
        var enabled = panel.Rows.First(r => r.Knob.Name == "IsEnabled");
        enabled.IsChanged.ShouldBeFalse();

        button.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        enabled.IsChanged.ShouldBeTrue();
        panel.Xaml.ShouldContain("IsEnabled=\"False\"");
    }

    [AvaloniaFact]
    public void Reset_Puts_Every_Property_Back_To_The_Demos_Start()
    {
        var (view, panel, _) = Open("Button.Playground");
        var button = (Button)Target(view);
        var start = panel.Xaml;
        panel.Rows.First(r => r.Knob.Name == "Size").Write(ControlSize.Large);
        panel.Rows.First(r => r.Knob.Name == "Content").Write("Export report");
        panel.Xaml.ShouldNotBe(start);

        panel.ResetAll();
        button.Size.ShouldBe(ControlSize.Medium);
        button.Content.ShouldBe("Save changes");
        panel.Xaml.ShouldBe(start);
        panel.Rows.Any(r => r.IsChanged).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void The_Panel_Sits_Beside_A_Wide_Stage_And_Is_A_Closed_Drawer_On_A_Narrow_Page()
    {
        var (view, panel, window) = Open("Button.Playground", 1200);
        panel.IsDrawer.ShouldBeFalse();
        panel.IsOpen.ShouldBeTrue();

        window.Width = 600;
        Dispatcher.UIThread.RunJobs();
        panel.IsDrawer.ShouldBeTrue();
        panel.IsOpen.ShouldBeFalse();

        panel.SetOpen(true);
        panel.IsOpen.ShouldBeTrue();

        window.Width = 1200;
        Dispatcher.UIThread.RunJobs();
        panel.IsDrawer.ShouldBeFalse();
        panel.IsOpen.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void The_Code_Tab_Hides_The_Panel()
    {
        var (view, panel, _) = Open("Button.Playground");
        view.ShowCode(true);
        panel.IsVisible.ShouldBeFalse();
        view.ShowCode(false);
        panel.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Input_Follows_Typing_In_The_Preview_And_Reflects_Properties_In_The_Xaml()
    {
        var (view, panel, _) = Open("Input.Playground");
        var input = (Input)Target(view);
        input.Text = "hello";
        Dispatcher.UIThread.RunJobs();
        panel.Rows.First(r => r.Knob.Name == "Text").Current.ShouldBe("hello");
        Flat(panel.Xaml).ShouldContain("Text=\"hello\"");

        panel.Rows.First(r => r.Knob.Name == "IsReadOnly").Write(true);
        input.IsReadOnly.ShouldBeTrue();
        Flat(panel.Xaml).ShouldContain("IsReadOnly=\"True\"");
    }

    [AvaloniaFact]
    public void Switch_Edits_A_Nullable_Bool_And_Reset_Restores_The_Demos_Start()
    {
        var (view, panel, _) = Open("Switch.Playground");
        var toggle = (Switch)Target(view);
        var checkedRow = panel.Rows.First(r => r.Knob.Name == "IsChecked");
        checkedRow.Knob.IsNullable.ShouldBeTrue();
        toggle.IsChecked.ShouldBe(true);

        checkedRow.Write(false);
        toggle.IsChecked.ShouldBe(false);
        // Off is the default, so the XAML leaves the property out.
        panel.Xaml.ShouldNotContain("IsChecked");

        panel.ResetAll();
        toggle.IsChecked.ShouldBe(true);
        Flat(panel.Xaml).ShouldContain("IsChecked=\"True\"");
    }

    [AvaloniaFact]
    public void Slider_Takes_Numbers_Inside_Its_Range()
    {
        var (view, panel, _) = Open("Slider.Playground");
        var slider = (Slider)Target(view);
        var value = panel.Rows.First(r => r.Knob.Name == "Value");
        value.Knob.Kind.ShouldBe(GalleryKnobKind.Number);
        (value.Knob.Min, value.Knob.Max, value.Knob.Step).ShouldBe((0d, 100d, 1d));

        value.Write(30d);
        slider.Value.ShouldBe(30d);
        Flat(panel.Xaml).ShouldContain("Value=\"30\"");

        panel.Rows.First(r => r.Knob.Name == "ThumbDiameter").Write(24d);
        slider.ThumbDiameter.ShouldBe(24d);
    }

    [AvaloniaFact]
    public void Select_Keeps_Its_Options_In_The_Xaml()
    {
        var (view, panel, _) = Open("Select.Playground");
        panel.Xaml.ShouldContain("<arc:Select");
        panel.Xaml.ShouldContain("    <arc:SelectItem Content=\"North America\" Value=\"na\" />");
        panel.Xaml.ShouldEndWith("</arc:Select>");
        panel.Xaml.ShouldNotContain("xmlns");

        panel.Rows.First(r => r.Knob.Name == "Size").Write(ControlSize.Large);
        Flat(panel.Xaml).ShouldContain("Size=\"Large\"");
    }

    [Fact]
    public void The_Snippet_Leaves_Out_Defaults_Wraps_Long_Elements_And_Escapes()
    {
        var variant = new GalleryKnob("Variant", GalleryKnobKind.Choice, static () => Button.VariantProperty, false, null, "Button")
        {
            Choices = [new("Primary", ButtonVariant.Primary), new("Danger", ButtonVariant.Danger)]
        };
        var label = new GalleryKnob("Content", GalleryKnobKind.Text, static () => ContentControl.ContentProperty, false, null, null);
        var playground = new GalleryPlayground("X.Y", "arc:Button", [("Variant", "Primary")], null, [variant, label]);

        GalleryPlaygroundSnippet.Build(playground, k => k == variant ? ButtonVariant.Primary : "", _ => ButtonVariant.Primary)
            .ShouldBe("<arc:Button />");
        Flat(GalleryPlaygroundSnippet.Build(playground, k => k == variant ? ButtonVariant.Danger : "Tom & \"Jerry\"", k => k == variant ? ButtonVariant.Primary : null))
            .ShouldBe("<arc:Button Variant=\"Danger\" Content=\"Tom &amp; &quot;Jerry&quot;\" />");

        var long1 = new GalleryKnob("Content", GalleryKnobKind.Text, static () => ContentControl.ContentProperty, false, null, null);
        var wide = new GalleryPlayground("X.Z", "arc:Button", [("Variant", "Danger"), ("Size", "Large")], null, [long1]);
        GalleryPlaygroundSnippet.Build(wide, _ => new string('a', 60), _ => null)
            .ShouldBe("<arc:Button\n    Variant=\"Danger\"\n    Size=\"Large\"\n    Content=\"" + new string('a', 60) + "\" />");
    }
}
