# ArcGallery agent guide

ArcGallery is the gallery and documentation site of TKCode.Arc (https://github.com/AIDotNet/TKCode.Arc), an Arc-style component
library for Avalonia 12. The libraries are not here: every project builds against a TKCode.Arc checkout (`ArcRoot`, resolved in
`Directory.Build.props`: `-p:ArcRoot`, `$ARC_ROOT`, `./arc`, then `../AtomUI.Arc`). Read TKCode.Arc's `AGENTS.md` and
`design/engineering/gallery-app.md` there before changing the gallery.

## Layout

- `gallery/TKCode.Arc.Gallery`: shell (home page, routes, top bar) and pages. A component's page is its docs page `Docs/<Category>/<Component>/<lang>.md` with live examples embedded by `<Demo name="Name" />`, each written as `Demos/<Category>/<Component>/<Name>.axaml` (plus `.axaml.cs` only when it needs logic). Shell text comes from `Localization/*.xlf` via `GalleryText` (hosts call `UseArcGallery()`). The sidebar's Docs section is `Guides/<Page>/<lang>.md`.
- `gallery/TKCode.Arc.Gallery.Shots` (docs screenshots), `.Content` (the pages' Markdown as resources), `.Desktop` and `.Browser` (hosts), `.Generators` (collects demos, builds API tables from the library's XML docs, checks translations), `.Llms` (the site's files for AI assistants).
- `tests/TKCode.Arc.Gallery.Tests`: headless Avalonia tests of the gallery and of the blocks and special components that are tested through their demos. It shares `TestHelpers.cs`, `NullTemplateProbe.cs` and `PdfTestSupport.cs` with TKCode.Arc's `tests/TKCode.Arc.Tests` and sees the libraries' internals (`InternalsVisibleTo TKCode.Arc.Gallery.Tests`).
- `tools/docshots`: checks a docs page without a build (`check`), captures its screenshots (`shoot`) and renders a gallery page (`page`).

## Rules

- Every Arc component needs docs pages `gallery/TKCode.Arc.Gallery/Docs/<Category>/<Component>/{en-US,zh-CN,zh-TW}.md` (checked by `GalleryDocsTests`), written in this repository when the component is added to TKCode.Arc. Its public properties and events need XML `<summary>` text in TKCode.Arc (they feed the API tables).
- Every new component or block gets the gallery's "New" badge: its catalog entry carries `AddedOn: "yyyy-MM-dd"`. Arc components: the entry in `gallery/TKCode.Arc.Gallery/Catalog/GalleryCatalog.g.cs` (keep it when regenerating); Extras, Special and Workflow: the entry in `GalleryCatalog.cs`; Blocks and Mobile: the `s_addedOn` table in `GalleryCatalog.cs` (checked by `GalleryNewBadgeTests`).
- Controls use only public Avalonia APIs; colors, radii, shadows and fonts come from `{arc:ArcDesignTokenResource ...}`. C# control-flow bodies always use braces. No reflection in the gallery's code; it must stay AOT and trimming safe (the tests may use reflection).
- Package versions are TKCode.Arc's (`Directory.Packages.props` imports its file). Change them there.
- Do not commit without an explicit request.

## Verify

Do not run the full test suite locally (it takes 8 minutes or more); CI runs it. Build what you touched and run the tests for it:

```bash
dotnet build gallery/TKCode.Arc.Gallery.Desktop         # no GDOC warnings for pages you touched
dotnet test tests/TKCode.Arc.Gallery.Tests --filter "FullyQualifiedName~GalleryDocsTests|FullyQualifiedName~GalleryDemosTests|FullyQualifiedName~GalleryNewBadgeTests|FullyQualifiedName~GallerySmokeTests.Catalog_Lists_All_Components"
```

If `dotnet test` hangs at discovery under load, run the test executable directly: `tests/TKCode.Arc.Gallery.Tests/bin/Debug/net10.0/TKCode.Arc.Gallery.Tests -class "*.GalleryDocsTests"`.
