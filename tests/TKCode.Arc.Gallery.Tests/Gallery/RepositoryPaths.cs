using System.Reflection;

namespace TKCode.Arc.Tests;

/// <summary>
/// The two checkouts the gallery tests read files from: this repository (the gallery) and TKCode.Arc (the libraries, whose
/// location the build resolved as <c>ArcRoot</c> in Directory.Build.props).
/// </summary>
internal static class RepositoryPaths
{
    /// <summary>The root of this repository: the folder above the test folder that holds ArcGallery.slnx.</summary>
    public static string Gallery { get; } = FindGallery();

    /// <summary>The root of the TKCode.Arc checkout the tests were built against.</summary>
    public static string Arc { get; } = FindArc();

    private static string FindGallery()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "ArcGallery.slnx")))
            {
                return folder.FullName;
            }
        }
        throw new InvalidOperationException("ArcGallery.slnx was not found above the test folder");
    }

    private static string FindArc()
    {
        foreach (var metadata in typeof(RepositoryPaths).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (metadata.Key == "ArcRoot" && !string.IsNullOrEmpty(metadata.Value))
            {
                return Path.GetFullPath(metadata.Value);
            }
        }
        throw new InvalidOperationException("The test assembly carries no ArcRoot metadata (see TKCode.Arc.Gallery.Tests.csproj)");
    }
}
