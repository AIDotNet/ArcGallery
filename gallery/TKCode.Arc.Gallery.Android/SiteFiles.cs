using System.Security.Cryptography;
using System.Text;
using TKCode.Arc.Gallery.Docs;
using TKCode.Arc.Gallery.Shell;

namespace TKCode.Arc.Gallery.Android;

/// <summary>
/// The files the APK does not carry (the docs screenshots and the pages' text, about 35 MB): they are fetched from the site, the
/// same files the WebAssembly host reads (<c>shots/</c>, <c>content/</c>), and kept in the app's cache folder, so a page that
/// was opened once opens again without a network. The installer stays small; the first visit to a page needs a connection.
/// </summary>
internal sealed class SiteFiles
{
    /// <summary>Where the files are served; the same address as the site.</summary>
    public static readonly Uri Root = new("https://docs.tkcode.si/");

    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly string _cache = Path.Combine(Path.GetTempPath(), "site-files");

    /// <summary>
    /// The bytes of <paramref name="relative"/> (<c>shots/Docs/...webp</c>), or null when neither the site nor the cache has it.
    /// <paramref name="refresh"/> asks the site first and falls back to the cache (text changes with a deploy); otherwise the
    /// cache is asked first (a screenshot is the same for a given path).
    /// </summary>
    public async Task<byte[]?> GetAsync(string relative, bool refresh)
    {
        var file = Path.Combine(_cache, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(relative)))[..32]);
        if (!refresh && TryRead(file) is { } cached)
        {
            return cached;
        }
        try
        {
            using var response = await s_http.GetAsync(new Uri(Root, relative)).ConfigureAwait(false);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType != "text/html")
            {
                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                Write(file, bytes);
                return bytes;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            // Offline or the site is down: what was fetched before still shows.
        }
        return TryRead(file);
    }

    /// <summary>The same, synchronously, for the map outlines the demo data reads in synchronous code.</summary>
    public byte[]? Get(string relative, bool refresh) => Task.Run(() => GetAsync(relative, refresh)).GetAwaiter().GetResult();

    private static byte[]? TryRead(string file)
    {
        try
        {
            return File.Exists(file) ? File.ReadAllBytes(file) : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static void Write(string file, byte[] bytes)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllBytes(file, bytes);
        }
        catch (IOException)
        {
            // A full or read-only cache only means the next visit asks again.
        }
    }
}

/// <summary>The docs screenshots, from <c>shots/</c> on the site.</summary>
internal sealed class SiteShotSource(SiteFiles files) : IGalleryShotSource
{
    public async ValueTask<Stream?> OpenAsync(Uri uri)
    {
        // uri is avares://TKCode.Arc.Gallery.Shots/Docs/<Category>/<Component>/images/<Name>.webp
        var bytes = await files.GetAsync("shots" + uri.AbsolutePath, refresh: false);
        return bytes is null ? null : new MemoryStream(bytes, writable: false);
    }
}

/// <summary>The text of the docs pages and guides, from <c>content/</c> on the site.</summary>
internal sealed class SiteTextSource(SiteFiles files) : IGalleryTextSource
{
    public async ValueTask<string?> ReadAsync(string path)
    {
        var bytes = await files.GetAsync("content/" + path, refresh: true);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }
}
