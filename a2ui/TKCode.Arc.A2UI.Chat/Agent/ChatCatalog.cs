namespace TKCode.Arc.A2UI.Chat;

/// <summary>
/// The chat app's own catalog, used beside the basic and the Arc catalogs: one agent function the app answers itself. It
/// shows how an app adds to A2UI with a catalog document of its own (components would add a view each, registered in an
/// <see cref="A2UIComponentRegistry"/>).
/// </summary>
public static class ChatCatalog
{
    public const string Id = "https://a2ui.tkcode.si/catalogs/chat/v1";

    private const string Document = """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "$id": "https://a2ui.tkcode.si/catalogs/chat/v1",
          "catalogId": "https://a2ui.tkcode.si/catalogs/chat/v1",
          "protocolVersion": "1.0",
          "title": "A2UI Chat catalog",
          "description": "Functions the chat app answers itself (callAgentFunction). Components come from the basic and the Arc catalogs.",
          "instructions": "getBaggageAllowance returns a flight's baggage allowance; call it with callAgentFunction.",
          "components": {},
          "functions": {
            "getBaggageAllowance": {
              "type": "object",
              "description": "The baggage allowance of a flight, answered by the agent (callAgentFunction).",
              "returnType": "string",
              "properties": {
                "@call": { "const": "getBaggageAllowance" },
                "args": {
                  "type": "object",
                  "properties": { "flightId": { "$ref": "common_types.json#/$defs/DynamicString" } },
                  "required": ["flightId"],
                  "unevaluatedProperties": false
                }
              },
              "required": ["@call", "args"]
            }
          }
        }
        """;

    public static A2UICatalog Create() => A2UICatalog.FromJson(Document);
}
