using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>General preferences.</summary>
public sealed class ChatPreferences
{
    /// <summary>Show the inspector (A2UI messages, data, errors).</summary>
    public bool DeveloperMode { get; set; }

    public bool SidebarCollapsed { get; set; }

    public ChatAgentSettings Agent { get; set; } = new();
}

/// <summary>
/// The app's state: providers, the current provider and model, preferences and conversations, kept in
/// <see cref="IChatStorage"/>. Keys are stored only for providers marked "remember".
/// </summary>
public sealed class ChatAppState
{
    private const string SettingsKey = "a2ui-chat.settings";
    private const string ConversationPrefix = "a2ui-chat.conv.";
    private readonly IChatStorage _storage;

    public ChatAppState(IChatStorage storage)
    {
        _storage = storage;
        Load();
    }

    public List<ChatProviderConfig> Providers { get; } = new();

    public string? ActiveProviderId { get; set; }

    public string? ActiveModel { get; set; }

    public ChatPreferences Preferences { get; } = new();

    public ObservableCollection<ChatConversation> Conversations { get; } = new();

    public ChatProviderConfig? ActiveProvider => Providers.FirstOrDefault(p => p.Id == ActiveProviderId) ?? Providers.FirstOrDefault();

    /// <summary>The catalogs every conversation's renderer supports.</summary>
    public static IReadOnlyList<A2UICatalog> Catalogs { get; } = [A2UICatalogs.Basic, A2UICatalogs.Arc, ChatCatalog.Create()];

    /// <summary>The views of those catalogs (the basic and the Arc catalogs; the chat catalog has functions only).</summary>
    public static A2UIComponentRegistry Registry => A2UIComponentRegistry.Default;

    public event EventHandler? Changed;

    public ChatConversation NewConversation()
    {
        var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = Catalogs.ToList() })
        {
            Settings = Preferences.Agent,
            ProviderSource = () => ActiveProvider is { } config && (ActiveModel ?? config.DefaultModel ?? config.Models.FirstOrDefault()) is { Length: > 0 } model
                ? (HttpChatProvider.Create(config), model)
                : null
        };
        conversation.Saved += (_, _) => SaveConversation(conversation);
        Conversations.Insert(0, conversation);
        return conversation;
    }

    /// <summary>Opens a built-in example in a new conversation and plays its first answer.</summary>
    public ChatConversation OpenShowcase(Showcase showcase)
    {
        // An example opens once: choosing it again returns to the same conversation (where you left it).
        if (Conversations.FirstOrDefault(c => c.ShowcaseId == showcase.Id) is { } open)
        {
            return open;
        }

        var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = Catalogs.ToList() })
        {
            ShowcaseId = showcase.Id,
            Title = showcase.Title,
            Settings = new ChatAgentSettings { Tools = [], AutoCorrect = false }
        };
        var replay = new ReplayProvider(showcase);
        conversation.ProviderSource = () => (replay, "replay");
        foreach (var (name, function) in showcase.AgentFunctions)
        {
            conversation.AgentFunctions[name] = function;
        }

        Conversations.Insert(0, conversation);
        _ = conversation.SendAsync(showcase.Prompt);
        conversation.Title = showcase.Title;
        return conversation;
    }

    public void Delete(ChatConversation conversation)
    {
        conversation.Stop();
        Conversations.Remove(conversation);
        _storage.Remove(ConversationPrefix + conversation.Id);
        conversation.Dispose();
    }

    public void Save()
    {
        var providers = new JsonArray();
        foreach (var provider in Providers)
        {
            providers.Add((JsonNode?)(provider.ToJson(includeKey: true)));
        }

        var json = new JsonObject
        {
            ["providers"] = providers,
            ["activeProviderId"] = ActiveProviderId,
            ["activeModel"] = ActiveModel,
            ["developerMode"] = Preferences.DeveloperMode,
            ["sidebarCollapsed"] = Preferences.SidebarCollapsed,
            ["autoCorrect"] = Preferences.Agent.AutoCorrect,
            ["fullSchema"] = Preferences.Agent.FullSchemaInPrompt
        };
        TryWrite(SettingsKey, json.ToJsonString());
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SaveConversation(ChatConversation conversation)
    {
        if (conversation.ShowcaseId is null)
        {
            TryWrite(ConversationPrefix + conversation.Id, conversation.ToJson().ToJsonString());
        }
    }

    /// <summary>Exports everything except API keys.</summary>
    public string Export()
    {
        var conversations = new JsonArray();
        foreach (var conversation in Conversations.Where(c => c.ShowcaseId is null))
        {
            conversations.Add((JsonNode?)(conversation.ToJson()));
        }

        var providers = new JsonArray();
        foreach (var provider in Providers)
        {
            providers.Add((JsonNode?)(provider.ToJson(includeKey: false)));
        }

        return new JsonObject { ["providers"] = providers, ["conversations"] = conversations }.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    public void ClearAll()
    {
        foreach (var key in _storage.Keys(ConversationPrefix))
        {
            _storage.Remove(key);
        }

        _storage.Remove(SettingsKey);
        foreach (var conversation in Conversations.ToList())
        {
            conversation.Dispose();
        }

        Conversations.Clear();
        Providers.Clear();
        ActiveProviderId = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void Load()
    {
        try
        {
            if (_storage.Read(SettingsKey) is { } text && A2UIJson.Parse(text) is JsonObject json)
            {
                foreach (var provider in (json["providers"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                {
                    Providers.Add(ChatProviderConfig.FromJson(provider));
                }

                ActiveProviderId = A2UIJson.AsString(json["activeProviderId"]);
                ActiveModel = A2UIJson.AsString(json["activeModel"]);
                Preferences.DeveloperMode = A2UIJson.AsBoolean(json["developerMode"]) == true;
                Preferences.SidebarCollapsed = A2UIJson.AsBoolean(json["sidebarCollapsed"]) == true;
                Preferences.Agent.AutoCorrect = A2UIJson.AsBoolean(json["autoCorrect"]) != false;
                Preferences.Agent.FullSchemaInPrompt = A2UIJson.AsBoolean(json["fullSchema"]) == true;
            }
        }
        catch (A2UIException)
        {
            // Unreadable settings: start fresh.
        }
    }

    /// <summary>Restores saved conversations (call on the UI thread: each conversation owns a renderer).</summary>
    public void LoadConversations()
    {
        var restored = new List<ChatConversation>();
        foreach (var key in _storage.Keys(ConversationPrefix))
        {
            try
            {
                if (_storage.Read(key) is { } text && A2UIJson.Parse(text) is JsonObject json)
                {
                    var conversation = new ChatConversation(new A2UIRendererOptions { Catalogs = Catalogs.ToList() })
                    {
                        Settings = Preferences.Agent,
                        ProviderSource = () => ActiveProvider is { } config && (ActiveModel ?? config.DefaultModel ?? config.Models.FirstOrDefault()) is { Length: > 0 } model
                            ? (HttpChatProvider.Create(config), model)
                            : null
                    };
                    conversation.Restore(json);
                    conversation.Saved += (_, _) => SaveConversation(conversation);
                    restored.Add(conversation);
                }
            }
            catch (A2UIException)
            {
                _storage.Remove(key);
            }
        }

        foreach (var conversation in restored.OrderByDescending(c => c.UpdatedAt))
        {
            Conversations.Add(conversation);
        }
    }

    private void TryWrite(string key, string value)
    {
        try
        {
            _storage.Write(key, value);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            // Storage full or blocked: the app keeps working in memory.
        }
    }
}
