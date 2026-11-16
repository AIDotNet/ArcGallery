using Avalonia.Platform;
using TKCode.Arc.Gallery.Shell;

namespace TKCode.Arc.Gallery.Android;

/// <summary>Reads the logos of the Friends page from this assembly's resources (the files of the browser host's <c>wwwroot/friends</c>).</summary>
internal sealed class AssetLogoSource : IGalleryLogoSource
{
    public ValueTask<Stream?> OpenAsync(string name)
    {
        var uri = new Uri($"avares://TKCode.Arc.Gallery.Android/Friends/{name}.png");
        return new(AssetLoader.Exists(uri) ? AssetLoader.Open(uri) : null);
    }
}
