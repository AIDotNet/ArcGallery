using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class CodeMagicMovePreviewDemo : UserControl
{
    public CodeMagicMovePreviewDemo()
    {
        InitializeComponent();
        Walkthrough.Steps.Add(new CodeMagicMoveStep("Start with a label", """
            export function ThemeToggle() {
              return <span>Light</span>;
            }
            """, "A label that always says light."));
        Walkthrough.Steps.Add(new CodeMagicMoveStep("Remember the mode", """
            import { useState } from "react";

            export function ThemeToggle() {
              const [mode, setMode] = useState("light");
              return <span>Light</span>;
            }
            """, "Import useState and keep the current mode."));
        Walkthrough.Steps.Add(new CodeMagicMoveStep("Flip it on click", """
            import { useState } from "react";

            export function ThemeToggle() {
              const [mode, setMode] = useState("light");
              const dark = mode === "dark";
              function flip() {
                setMode(dark ? "light" : "dark");
              }
              return (
                <button onClick={flip}>{mode}</button>
              );
            }
            """, "Turn the label into a button that swaps the mode."));
        Walkthrough.Steps.Add(new CodeMagicMoveStep("Say what it does", """
            import { useState } from "react";

            export function ThemeToggle() {
              const [mode, setMode] = useState("light");
              const dark = mode === "dark";
              function flip() {
                setMode(dark ? "light" : "dark");
              }
              return (
                <button
                  onClick={flip}
                  aria-pressed={dark}
                >
                  {dark ? "Dark" : "Light"}
                </button>
              );
            }
            """, "Mark the button pressed while dark mode is on."));
    }
}
