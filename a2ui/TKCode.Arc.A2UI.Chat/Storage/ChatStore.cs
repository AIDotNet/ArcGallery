using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>Key-value storage on this device (a settings folder on desktop, local storage in a browser).</summary>
public interface IChatStorage
{
    string? Read(string key);

    void Write(string key, string value);

    void Remove(string key);

    IEnumerable<string> Keys(string prefix);
}

/// <summary>Storage in memory only (tests, private windows).</summary>
public sealed class MemoryChatStorage : IChatStorage
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public string? Read(string key) => _values.TryGetValue(key, out var v) ? v : null;

    public void Write(string key, string value) => _values[key] = value;

    public void Remove(string key) => _values.Remove(key);

    public IEnumerable<string> Keys(string prefix) => _values.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
}

/// <summary>Storage in files under the user's application data folder (desktop).</summary>
public sealed class FileChatStorage : IChatStorage
{
    private readonly string _root;

    public FileChatStorage(string? root = null)
    {
        _root = root ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TKCode.A2UIChat");
        Directory.CreateDirectory(_root);
    }

    private string PathOf(string key) => Path.Combine(_root, string.Concat(key.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_')) + ".json");

    public string? Read(string key)
    {
        var path = PathOf(key);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public void Write(string key, string value)
    {
        var path = PathOf(key);
        File.WriteAllText(path, value);
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public void Remove(string key)
    {
        var path = PathOf(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public IEnumerable<string> Keys(string prefix)
        => Directory.EnumerateFiles(_root, "*.json").Select(f => Path.GetFileNameWithoutExtension(f)).Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
}

/// <summary>Reads and writes the app's settings and conversations as JSON.</summary>
public static class ChatSerializer
{
    public static JsonObject Message(ChatMessage message)
    {
        var parts = new JsonArray();
        foreach (var part in message.Parts)
        {
            parts.Add((JsonNode?)(part switch
            {
                ChatTextPart t => new JsonObject { ["type"] = "text", ["text"] = t.Text },
                ChatReasoningPart r => new JsonObject { ["type"] = "reasoning", ["summary"] = r.Summary, ["protocol"] = r.Protocol.ToString(), ["raw"] = r.Raw?.DeepClone() },
                ChatToolCallPart c => new JsonObject { ["type"] = "tool_call", ["id"] = c.Id, ["name"] = c.Name, ["arguments"] = c.ArgumentsJson },
                ChatToolResultPart r => new JsonObject { ["type"] = "tool_result", ["callId"] = r.CallId, ["name"] = r.Name, ["content"] = r.Content, ["isError"] = r.IsError },
                _ => null
            }));
        }

        return new JsonObject { ["role"] = message.Role == ChatRole.User ? "user" : "assistant", ["parts"] = parts };
    }

    public static ChatMessage Message(JsonObject json)
    {
        var parts = new List<ChatPart>();
        foreach (var p in (json["parts"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
        {
            ChatPart? part = A2UIJson.AsString(p["type"]) switch
            {
                "text" => new ChatTextPart(A2UIJson.AsString(p["text"]) ?? string.Empty),
                "reasoning" => new ChatReasoningPart(A2UIJson.AsString(p["summary"]) ?? string.Empty,
                    Enum.TryParse<ChatProtocol>(A2UIJson.AsString(p["protocol"]), out var protocol) ? protocol : ChatProtocol.AnthropicMessages,
                    p["raw"]?.DeepClone()),
                "tool_call" => new ChatToolCallPart(A2UIJson.AsString(p["id"]) ?? string.Empty, A2UIJson.AsString(p["name"]) ?? string.Empty,
                    A2UIJson.AsString(p["arguments"]) ?? "{}"),
                "tool_result" => new ChatToolResultPart(A2UIJson.AsString(p["callId"]) ?? string.Empty, A2UIJson.AsString(p["name"]) ?? string.Empty,
                    A2UIJson.AsString(p["content"]) ?? string.Empty, A2UIJson.AsBoolean(p["isError"]) == true),
                _ => null
            };
            if (part is not null)
            {
                parts.Add(part);
            }
        }

        return new ChatMessage(A2UIJson.AsString(json["role"]) == "user" ? ChatRole.User : ChatRole.Assistant, parts);
    }
}
