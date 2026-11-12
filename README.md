# ArcGallery

The gallery and documentation site of [TKCode.Arc](https://github.com/AIDotNet/TKCode.Arc), the Arc-style component library for
Avalonia: live examples for every component and block, the docs pages in English, 简体中文 and 繁體中文, and the desktop and
WebAssembly hosts that show them. It is published at **[docs.tkcode.si](https://docs.tkcode.si/)**.

The libraries themselves live in TKCode.Arc. This repository builds against a checkout of it.

## Layout

| Path | Contents |
|---|---|
| `gallery/TKCode.Arc.Gallery` | The shell (home page, routes, top bar), the catalog, the pages, the docs (`Docs/<Category>/<Component>/<lang>.md`), the guides and the demos (`Demos/`). |
| `gallery/TKCode.Arc.Gallery.Shots` | The screenshots of the docs pages. |
| `gallery/TKCode.Arc.Gallery.Content` | The pages' Markdown as resources (the browser host fetches a page's text when it is shown). |
| `gallery/TKCode.Arc.Gallery.Desktop` | The desktop host (Windows, macOS, Linux). |
| `gallery/TKCode.Arc.Gallery.Browser` | The WebAssembly host; not in the solution because it needs the `wasm-tools` workload. |
| `gallery/TKCode.Arc.Gallery.Generators` | Source generators: demos, API tables from the library's XML docs, translation checks. |
| `gallery/TKCode.Arc.Gallery.Llms` | Writes the site's files for AI assistants (`llms.txt`, every page as Markdown, the agent skill). |
| `tests/TKCode.Arc.Gallery.Tests` | Headless Avalonia tests of the gallery, and of the blocks and special components through their demos. |
| `tools/` | `docshots` (checks docs pages, captures screenshots), fonts, branding, macOS signing, `onigwrap` for the browser host, demo media. |
| `Dockerfile`, `docker/` | The WebAssembly host in an nginx image. |

## Building

The .NET 11 SDK (see `global.json`) and the .NET 10 runtime are needed. The libraries come from a TKCode.Arc checkout, found in this order:

1. `-p:ArcRoot=<path>` on the command line,
2. the `ARC_ROOT` environment variable,
3. `./arc` (what CI and the Dockerfile check out),
4. `../AtomUI.Arc`, next to this repository.

```bash
git clone https://github.com/AIDotNet/TKCode.Arc ../AtomUI.Arc      # once; any folder name works with ARC_ROOT
dotnet build gallery/TKCode.Arc.Gallery.Desktop
dotnet run --project gallery/TKCode.Arc.Gallery.Desktop -f net10.0
dotnet test tests/TKCode.Arc.Gallery.Tests --filter "FullyQualifiedName~GalleryDocsTests"
```

A full run of `TKCode.Arc.Gallery.Tests` takes several minutes; CI runs it. The package versions are TKCode.Arc's
(`Directory.Packages.props` imports its file), so the two repositories always build with the same dependencies.

## Releases

The desktop app for Windows, macOS and Linux is attached to this repository's releases. Tag a release with the same `v*` version as
TKCode.Arc: the workflows then build against that TKCode.Arc tag. The macOS build needs the signing secrets described in
`tools/macos/README.md`.

## License

MIT, see [LICENSE](LICENSE). The third-party assets the gallery ships (fonts, the Onigwrap build for WebAssembly, StreamMark, icons and
data) are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
