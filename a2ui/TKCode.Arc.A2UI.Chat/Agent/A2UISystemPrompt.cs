using System.Text;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The system prompt of the chat agent: how to answer with A2UI, a compact reference of the renderer's catalogs, and
/// the action protocol. It depends only on the catalogs and settings, so it stays the same across a conversation and
/// can be cached by the provider.
/// </summary>
public static class A2UISystemPrompt
{
    public static string Build(IReadOnlyList<A2UICatalog> catalogs, bool fullSchema, IReadOnlyList<ChatTool> tools)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            You are a helpful assistant in a chat app that can show interactive user interfaces. The app renders A2UI 1.0
            (agent-to-user interface) messages with native components. Use an interface when it helps the user act or
            understand: forms to fill, choices to make, data to compare, lists, charts, confirmations. Answer in plain
            Markdown when text is enough. Keep prose short when an interface carries the content.

            ## How to show an interface

            Write A2UI messages inside a fenced block that starts with a line ```a2ui and ends with a line ```.
            Each line inside the block is one complete JSON message (JSON Lines); never break a message over lines.
            Every message has "version": "1.0" and exactly one of: createSurface, updateComponents, updateDataModel,
            deleteSurface.

            - A new interface starts with createSurface: {"surfaceId", "catalogId", "components": [...], "dataModel": {...},
              "sendDataModel": true}. Put the whole first version of the interface in that one message when you can.
            - surfaceId: short, descriptive, unique in this conversation (for example "trip_form_1"). Reuse it to change
              an existing interface with updateComponents (components are replaced by id) or updateDataModel
              ({"surfaceId", "path": "/json/pointer", "value": ...}; value null deletes).
            - Components are a flat list. Exactly one has "id": "root". Containers refer to children by id
              ("child": "id" or "children": ["a", "b"]). Ids use letters, digits and '_' and do not start with a digit.
            - Values can be literals, data bindings {"@path": "/a/b"} or function calls {"@call": "name", "args": {...}}.
              Input components bind their "value" to a data path so what the user enters is kept in the data model.
            - Lists over data: "children": {"componentId": "row_template", "path": "/items"}; inside the template,
              paths without a leading "/" are relative to the item, and {"@call": "@index"} is the item's index.
            - To join text with data use {"@call": "formatString", "args": {"value": "Hello ${/user/name}"}}.
            - Buttons need "action": {"event": {"name": "...", "context": {"key": {"@path": "/..."}}}} to tell you what
              the user did, or {"functionCall": {...}} for a local function such as openUrl.
            - Use "@checks" on inputs and buttons for validation; a button with a failing check is disabled.
            - A surface's "catalogId" is usually the basic catalog. To use a component of another catalog listed below
              (charts, tables, Markdown and more), add "@catalogId": "<that catalog's id>" to that component; it then
              mixes freely with the basic components. Functions of another catalog take the same key inside "@call".

            ## What happens after you answer

            When the user acts on an interface you get a message <a2ui_action>{...}</a2ui_action>, followed by
            <a2ui_data_model surface="id">{...}</a2ui_data_model> with the current data of that surface. Reply to it:
            update the interface (updateComponents / updateDataModel), show a new one, or answer in text.
            If the renderer rejects something you get <a2ui_errors>[...]</a2ui_errors>; fix the listed messages and
            send corrected ones.
            """);

        sb.AppendLine();
        sb.AppendLine("## Catalogs");
        foreach (var catalog in catalogs)
        {
            sb.AppendLine();
            sb.Append("### ").Append(catalog.Title ?? catalog.Id).Append(" (catalogId: \"").Append(catalog.Id).AppendLine("\")");
            if (A2UIJson.AsString(catalog.Document["description"]) is { Length: > 0 } about)
            {
                sb.AppendLine(about);
            }

            if (fullSchema)
            {
                sb.AppendLine("```json").AppendLine(catalog.ToJson()).AppendLine("```");
                continue;
            }

            if (catalog.Components.Count > 0)
            {
                sb.AppendLine("Components (property: type, * required):");
            }

            foreach (var (name, definition) in catalog.Components.OrderBy(c => c.Key, StringComparer.Ordinal))
            {
                sb.Append("- ").Append(name);
                if (definition.Description is { Length: > 0 } description)
                {
                    sb.Append(" (").Append(description.TrimEnd('.')).Append(')');
                }

                sb.Append(": ");
                var props = definition.Properties
                    .Where(p => p.Key != "component")
                    .Select(p => p.Key + (definition.Required.Contains(p.Key) ? "*" : string.Empty) + ": " + Describe(p.Value, definition.Schema, p.Key));
                sb.AppendLine(string.Join("; ", props));
            }

            if (catalog.Functions.Count > 0)
            {
                sb.AppendLine("Functions:");
            }

            foreach (var (name, function) in catalog.Functions.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                sb.Append("- ").Append(name).Append('(').Append(string.Join(", ", function.Args.Keys)).Append(") -> ").Append(function.ReturnType);
                if (function.RequiresUserActivation)
                {
                    sb.Append(" [only in actions]");
                }

                sb.AppendLine();
            }

            if (catalog.Instructions is { Length: > 0 } instructions)
            {
                var cut = instructions.IndexOf("## Examples", StringComparison.Ordinal);
                sb.AppendLine("Guidance:").AppendLine(cut > 0 ? instructions[..cut].TrimEnd() : instructions);
            }
        }

        if (tools.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Tools");
            sb.AppendLine("You can call these tools to get data before you answer (they return demo data in this app):");
            foreach (var tool in tools)
            {
                sb.Append("- ").Append(tool.Name).Append(": ").AppendLine(tool.Description);
            }
        }

        return sb.ToString();
    }

    private static string Describe(A2UIPropertyShape shape, JsonObject componentSchema, string name)
    {
        var enumValues = FindEnum(componentSchema, name);
        if (enumValues is not null)
        {
            return string.Join("|", enumValues);
        }

        return shape.Kind switch
        {
            A2UIPropertyKind.DynamicString => "string",
            A2UIPropertyKind.DynamicNumber => "number",
            A2UIPropertyKind.DynamicBoolean => "boolean",
            A2UIPropertyKind.DynamicStringList => "string[]",
            A2UIPropertyKind.Child => "id",
            A2UIPropertyKind.ChildList => "ids | template",
            A2UIPropertyKind.Action => "action",
            A2UIPropertyKind.Array => "[" + string.Join(", ", shape.Items?.Fields?.Keys ?? []) + "]",
            A2UIPropertyKind.Object => "{" + string.Join(", ", shape.Fields?.Keys ?? []) + "}",
            A2UIPropertyKind.Literal => "literal",
            _ => "any"
        };
    }

    private static IEnumerable<string>? FindEnum(JsonObject schema, string name)
    {
        foreach (var node in schema.DescendantsAndSelf())
        {
            if (node is JsonObject obj && obj["properties"] is JsonObject props && props[name] is JsonObject prop && prop["enum"] is JsonArray values)
            {
                return values.Select(v => A2UIJson.ToDisplayString(v));
            }
        }

        return null;
    }

    private static IEnumerable<JsonNode> DescendantsAndSelf(this JsonNode node)
    {
        yield return node;
        switch (node)
        {
            case JsonObject obj:
                foreach (var (_, value) in obj)
                {
                    if (value is not null)
                    {
                        foreach (var d in value.DescendantsAndSelf())
                        {
                            yield return d;
                        }
                    }
                }

                break;
            case JsonArray array:
                foreach (var value in array)
                {
                    if (value is not null)
                    {
                        foreach (var d in value.DescendantsAndSelf())
                        {
                            yield return d;
                        }
                    }
                }

                break;
        }
    }
}
