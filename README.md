# ArcGallery

The gallery and documentation site of [TKCode.Arc](https://github.com/AIDotNet/TKCode.Arc), the Arc-style component library for
Avalonia: live examples for every component and block, the docs pages in English, 简体中文 and 繁體中文, and the desktop and
WebAssembly hosts that show them. It is published at **[docs.tkcode.si](https://docs.tkcode.si/)**.

The libraries themselves live in TKCode.Arc. This repository builds against a checkout of it, including the StreamMark
engine that renders the docs and streamed replies.

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

The .NET 11 SDK (see `global.json`) and the .NET 10 runtime are needed. Use TKCode.Arc `v1.3.0-preview.4` or a newer
checkout with the incremental Markdown parser. The checkout is found in this order:

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
StreamMark ships with Arc; use the `arc_ref` workflow input to select the checkout for both.

## Releases

Windows x64, Linux x64 and both macOS architectures publish with Native AOT. Extract the Windows ZIP or Linux tar.gz and keep the
native libraries beside the executable. The macOS DMG is signed and notarized; see `tools/macos/README.md` for the signing setup.
The Android workflow publishes an arm64 APK with .NET for Android's experimental Native AOT runtime. Release publishes of both
browser apps compile the managed assemblies to WebAssembly and retain the IL needed by the interpreter fallback. Pass
`-p:ArcAot=false` to publish without AOT. The Gallery Docker image uses the interpreter by default. To enable AOT for that image,
select `wasm_aot` when running the **Docker gallery** workflow manually or pass `--build-arg ARC_WASM_AOT=true` to Docker.

Tag a release with the same `v*` version as TKCode.Arc so the workflows build against the matching library tag. Existing release
downloads are unchanged until rebuilt.

## License

MIT, see [LICENSE](LICENSE). The third-party assets the gallery ships (fonts, the Onigwrap build for WebAssembly, StreamMark, icons and
data) are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
