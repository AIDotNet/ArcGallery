using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>Two versions of a C# file side by side: changed lines and characters are highlighted, and the right side can be edited.</summary>
public partial class DiffEditorPreviewDemo : UserControl
{
    public DiffEditorPreviewDemo()
    {
        InitializeComponent();
        Diff.OriginalText = """
            using System;

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var name = args.Length > 0 ? args[0] : "world";
                    Console.WriteLine("Hello, " + name + "!");
                }

                private static int Add(int a, int b)
                {
                    return a + b;
                }
            }
            """;
        Diff.ModifiedText = """
            using System;
            using System.Linq;

            public static class Program
            {
                public static void Main(string[] args)
                {
                    var name = args.FirstOrDefault() ?? "world";
                    Console.WriteLine($"Hello, {name}!");
                }

                private static int Add(int a, int b) => a + b;
            }
            """;
    }
}
