---
title: A2UI surface
summary: "Render A2UI 1.0 (agent-to-user interface) messages with Arc controls: an agent describes a screen as JSON, the surface draws it, keeps the data the user enters, and sends the user's actions back to the agent."
---

<Demo name="Preview" />

## When to use

- An AI agent should answer with an interface instead of text: a form to fill, choices to make, a list or a chart to compare.
- The interface comes from a model or a server at run time and must stay inside your design system: the agent can only use the components you register, never code or styles.
- One protocol for several front ends: A2UI is the same on the web, on mobile and here.

## When not to use

- Interfaces you build yourself in XAML or C#: use the controls directly.
- Rich documents and long answers: use `Markdown`.

## Usage

Add the package and register it in `UseArc`:

```csharp
AppBuilder.Configure<App>()
    .UseArc(arc => arc.UseArcControls().UseArcA2UI());
```

Create an `A2UIRenderer`, feed it the agent's messages (one JSON object per line) and show a surface with `A2UISurfaceView`. The view waits for the surface to be created, fills in as components arrive, and clears when the surface is deleted.

```csharp
var renderer = new A2UIRenderer();
renderer.Outbound += (_, e) => SendToAgent(e.ToJson());   // actions, function calls, errors
var view = new A2UISurfaceView { Renderer = renderer, SurfaceId = "booking_1" };

await renderer.ProcessAsync(responseStream);                // JSON Lines, applied on the UI thread
```

### Two-way data

Inputs (`TextField`, `CheckBox`, `ChoicePicker`, `Slider`, `DateTimeInput`) write what the user enters into the surface's data model at the path their `value` is bound to. Other components bound to the same path update at once. Nothing is sent to the agent until an action runs; with `sendDataModel` the data goes with the action (`GetRendererDataModel`).

<Demo name="Form" />

### Checks and actions

`@checks` rules validate inputs and buttons with the catalog's functions (`required`, `email`, `regex`, `length`, `numeric`, `and`, `or`, `not`); a button whose checks fail is disabled. A button's `action.event` becomes an `action` message with its `context` resolved; `action.functionCall` runs a local function such as `openUrl`, which asks `UrlOpening` first.

### Catalogs

The renderer supports two catalogs by default:

- the A2UI basic catalog (`A2UICatalogs.Basic`, 18 components, 14 functions): text, images, icons, rows and columns, lists, cards, tabs, modals, buttons and the form inputs;
- the Arc catalog (`A2UICatalogs.Arc`, 18 components drawn with Arc controls): `BarChart`, `LineChart`, `DonutChart`, `Sparkline`, `Metric`, `Table`, `Timeline`, `Stepper`, `Progress`, `Alert`, `Badge`, `Avatar`, `Markdown`, `CodeBlock`, `Accordion`, `Switch`, `SegmentedControl` and `Rating`. A component uses it with `"@catalogId": "https://github.com/AIDotNet/TKCode.Arc/a2ui/catalogs/arc/v1"`; its inputs bind two-way like the basic ones, and Markdown links go through `UrlOpening`.

Add your own catalog as a JSON Schema document and register a view per component:

```csharp
var charts = A2UICatalog.FromJson(chartsCatalogJson);
var options = new A2UIRendererOptions { Catalogs = [A2UICatalogs.Basic, A2UICatalogs.Arc, charts] };
var registry = A2UIComponentRegistry.CreateDefault()
    .Register(charts.Id, "BarChart", () => new MyBarChartView());
```

A component picks a catalog with `@catalogId`; functions an agent implements are routed to it with `callAgentFunction`.

### Validation

By default (`A2UIValidationMode.Lenient`) every component is checked against its catalog schema and children that have not arrived yet show as placeholders, which suits answers streamed by a model. `Strict` also rejects incomplete batches (missing root, dangling references, cycles). Rejected messages are reported through `ErrorReported` and as `error` messages for the agent, so a model can correct itself.

## API reference

<!-- api: A2UISurfaceView -->

## Accessibility

`@accessibility` maps to the platform: `label` to the automation name, `description` to the help text, `live` to the live setting and `hidden` to a raw automation view. Arc controls keep their own keyboard and screen reader behavior.

## Motion

Components take the motion of the Arc controls they are drawn with. A component that arrives while the answer streams fills its place without moving its neighbours; there is no animation of its own.

## Responsive behavior

Rows, columns and lists use the width they are given; a horizontal list scrolls. Put the view in a scrolling container when an agent may send long interfaces.

## Related

- `Markdown`: rich text answers from the same agent.
- `ChatThread`: a conversation that can hold surfaces in its messages.
- `Form` and `FormItem`: forms written in XAML.
