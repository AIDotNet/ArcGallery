using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>A code editor with a short C# file: click into it and type, select with Shift and the arrows, undo with Ctrl+Z.</summary>
public partial class CodeEditorPreviewDemo : UserControl
{
    public CodeEditorPreviewDemo()
    {
        InitializeComponent();
        Editor.Text = """
            using System;

            namespace Demo;

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var name = args.Length > 0 ? args[0] : "world";
                    Console.WriteLine($"Hello, {name}!");

                    for (var i = 0; i < 3; i++)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
            """;
    }
}
