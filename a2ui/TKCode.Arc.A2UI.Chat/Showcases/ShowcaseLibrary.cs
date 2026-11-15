using System.Text.Json.Nodes;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>The built-in examples (all text and data made up for the demo).</summary>
public static partial class ShowcaseLibrary
{
    private const string B = A2UICatalogs.BasicCatalogId;
    private const string C = A2UICatalogs.ArcCatalogId;
    private const string F = ChatCatalog.Id;

    // {B} the basic catalog, {C} the Arc catalog (components), {F} the chat catalog (the app's own function).
    private static string S(string text) => text.Replace("{B}", B).Replace("{C}", C).Replace("{F}", F);

    /// <summary>Every example: the rich ones first, then the ones that show one feature each.</summary>
    public static IReadOnlyList<Showcase> All => s_all ??= [.. Rich, .. Basics];

    private static IReadOnlyList<Showcase>? s_all;

    /// <summary>Examples that each show one part of the protocol.</summary>
    public static IReadOnlyList<Showcase> Basics { get; } =
    [
        new Showcase
        {
            Id = "overview",
            Title = "Overview",
            Description = "A card with a chart, a choice and a button, streamed line by line.",
            Prompt = "Show this week's sign-ups and let me pick a plan.",
            Start = S("""
                Here are this week's sign-ups. Pick a plan to continue:
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"overview_1","catalogId":"{B}","sendDataModel":true,"dataModel":{"days":["Mon","Tue","Wed","Thu","Fri","Sat","Sun"],"signups":[42,51,48,63,58,87,66],"plan":[]},"components":[{"id":"root","component":"Card","child":"body"},{"id":"body","component":"Column","children":["title","subtitle","chart","plans","actions"]}]}}
                {"version":"1.0","updateComponents":{"surfaceId":"overview_1","components":[{"id":"title","component":"Text","text":"## Overview"},{"id":"subtitle","component":"Text","variant":"caption","text":{"@call":"formatString","args":{"value":"${/days/0} to ${/days/6} · 415 sign-ups"}}}]}}
                {"version":"1.0","updateComponents":{"surfaceId":"overview_1","components":[{"id":"chart","component":"BarChart","@catalogId":"{C}","title":"Sign-ups","labels":{"@path":"/days"},"values":{"@path":"/signups"},"unit":"users"}]}}
                {"version":"1.0","updateComponents":{"surfaceId":"overview_1","components":[{"id":"plans","component":"ChoicePicker","label":"Plan","options":[{"label":"Starter · $9/mo","value":"starter"},{"label":"Team · $29/mo","value":"team"},{"label":"Business · $99/mo","value":"business"}],"value":{"@path":"/plan"}}]}}
                {"version":"1.0","updateComponents":{"surfaceId":"overview_1","components":[{"id":"actions","component":"Row","justify":"end","children":["continue"]},{"id":"continue","component":"Button","variant":"primary","child":"continue_label","action":{"event":{"name":"continue","context":{"plan":{"@path":"/plan/0"}}}},"@checks":[{"condition":{"@call":"required","args":{"value":{"@path":"/plan"}}},"message":"Pick a plan."}]},{"id":"continue_label","component":"Text","text":"Continue"}]}}
                ```
                """),
            OnAction = new Dictionary<string, string>
            {
                ["continue"] = S("""
                    Done — the plan is set.
                    ```a2ui
                    {"version":"1.0","updateComponents":{"surfaceId":"overview_1","components":[{"id":"body","component":"Column","children":["title","done_row","chart"]},{"id":"done_row","component":"Row","align":"center","children":["done_icon","done_text"]},{"id":"done_icon","component":"Icon","name":"check"},{"id":"done_text","component":"Text","text":{"@call":"formatString","args":{"value":"Plan selected: **${/plan/0}**"}}}]}}
                    ```
                    """)
            }
        },
        new Showcase
        {
            Id = "contact",
            Title = "Contact form",
            Description = "Two-way binding, validation checks and a submit action.",
            Prompt = "I'd like to get in touch with sales.",
            Start = S("""
                Sure — fill this in and the team will reply within a day.
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"contact_1","catalogId":"{B}","sendDataModel":true,"dataModel":{"contact":{"name":"","email":"","topic":["pricing"],"message":"","subscribe":false}},"components":[{"id":"root","component":"Card","child":"form"},{"id":"form","component":"Column","children":["heading","names","email","topic","message","subscribe","submit_row"]},{"id":"heading","component":"Text","text":"### Contact sales"},{"id":"names","component":"TextField","label":"Name","placeholder":"Ada Lovelace","value":{"@path":"/contact/name"},"@checks":[{"condition":{"@call":"required","args":{"value":{"@path":"/contact/name"}}},"message":"Your name, please."}]},{"id":"email","component":"TextField","label":"Email","placeholder":"ada@example.com","value":{"@path":"/contact/email"},"@checks":[{"condition":{"@call":"email","args":{"value":{"@path":"/contact/email"}}},"message":"Enter a valid email address."}]},{"id":"topic","component":"ChoicePicker","label":"Topic","displayStyle":"chips","options":[{"label":"Pricing","value":"pricing"},{"label":"Security","value":"security"},{"label":"A demo","value":"demo"}],"value":{"@path":"/contact/topic"}},{"id":"message","component":"TextField","label":"Message","variant":"longText","value":{"@path":"/contact/message"}},{"id":"subscribe","component":"CheckBox","label":"Send me product updates","value":{"@path":"/contact/subscribe"}},{"id":"submit_row","component":"Row","justify":"end","children":["submit"]},{"id":"submit","component":"Button","variant":"primary","child":"submit_label","action":{"event":{"name":"submit","userMessage":{"@call":"formatString","args":{"value":"Sent the form as ${/contact/name}"}},"context":{"name":{"@path":"/contact/name"},"email":{"@path":"/contact/email"}}}},"@checks":[{"condition":{"@call":"and","args":{"values":[{"@call":"required","args":{"value":{"@path":"/contact/name"}}},{"@call":"email","args":{"value":{"@path":"/contact/email"}}}]}},"message":"Name and a valid email are needed."}]},{"id":"submit_label","component":"Text","text":"Send"}]}}
                ```
                """),
            OnAction = new Dictionary<string, string>
            {
                ["submit"] = S("""
                    Thanks! Your message is on its way.
                    ```a2ui
                    {"version":"1.0","updateComponents":{"surfaceId":"contact_1","components":[{"id":"form","component":"Column","children":["heading","sent"]},{"id":"heading","component":"Text","text":"### Message sent"},{"id":"sent","component":"Text","text":{"@call":"formatString","args":{"value":"We'll write to ${/contact/email} within one business day."}}}]}}
                    ```
                    """)
            }
        },
        new Showcase
        {
            Id = "flights",
            Title = "Flight search",
            Description = "A list template over data, currency formatting and an agent function.",
            Prompt = "Find flights from Berlin to Lisbon next Friday.",
            Start = S("""
                I found four flights for Friday:
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"flights_1","catalogId":"{B}","sendDataModel":true,"dataModel":{"flights":[{"id":"NW412","airline":"Northwind Air","depart":"07:15","arrive":"10:05","price":189},{"id":"CA208","airline":"Contoso Airlines","depart":"11:40","arrive":"14:35","price":142},{"id":"FJ771","airline":"Fabrikam Jet","depart":"15:05","arrive":"18:10","price":231},{"id":"LW090","airline":"Litware Wings","depart":"19:50","arrive":"22:40","price":118}]},"components":[{"id":"root","component":"Column","children":["heading","list"]},{"id":"heading","component":"Text","text":"### Berlin → Lisbon · Fri"},{"id":"list","component":"List","children":{"componentId":"flight","path":"/flights"}},{"id":"flight","component":"Card","child":"flight_row"},{"id":"flight_row","component":"Row","align":"center","children":["flight_info","flight_price","flight_select"]},{"id":"flight_info","component":"Column","weight":1,"children":["flight_title","flight_times","flight_bags"]},{"id":"flight_title","component":"Text","text":{"@call":"formatString","args":{"value":"**${airline}** · ${id}"}}},{"id":"flight_times","component":"Text","variant":"caption","text":{"@call":"formatString","args":{"value":"${depart} → ${arrive}"}}},{"id":"flight_bags","component":"Text","variant":"caption","text":{"@call":"getBaggageAllowance","catalogId":"{F}","args":{"flightId":{"@path":"id"}}}},{"id":"flight_price","component":"Text","text":{"@call":"formatCurrency","args":{"value":{"@path":"price"},"currency":"USD","decimals":0}}},{"id":"flight_select","component":"Button","child":"flight_select_label","action":{"event":{"name":"select_flight","userMessage":{"@call":"formatString","args":{"value":"Chose ${id}"}},"context":{"flight":{"@path":"id"},"price":{"@path":"price"}}}}},{"id":"flight_select_label","component":"Text","text":"Select"}]}}
                ```
                """),
            OnAction = new Dictionary<string, string>
            {
                ["select_flight"] = S("""
                    Good choice. I've held the seat for 20 minutes — confirm when you're ready.
                    ```a2ui
                    {"version":"1.0","createSurface":{"surfaceId":"flights_hold","catalogId":"{B}","components":[{"id":"root","component":"Card","child":"hold"},{"id":"hold","component":"Row","align":"center","children":["hold_icon","hold_text"]},{"id":"hold_icon","component":"Icon","name":"lock"},{"id":"hold_text","component":"Text","text":"Seat held until 20 minutes from now."}]}}
                    ```
                    """)
            },
            AgentFunctions = new Dictionary<string, Func<JsonObject, JsonNode?>>
            {
                ["getBaggageAllowance"] = args => (A2UIJson.AsString(args["flightId"]) ?? string.Empty).StartsWith("LW", StringComparison.Ordinal)
                    ? "Cabin bag only"
                    : "Cabin bag + 1 checked bag (23 kg)"
            }
        },
        new Showcase
        {
            Id = "booking",
            Title = "Restaurant booking",
            Description = "Date and time input, a slider, chips and a modal.",
            Prompt = "Book a table for Saturday evening.",
            Start = S("""
                Let's find you a table.
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"booking_1","catalogId":"{B}","sendDataModel":true,"dataModel":{"when":"2026-11-21T19:30:00","guests":2,"seating":["indoor"],"name":""},"components":[{"id":"root","component":"Card","child":"form"},{"id":"form","component":"Column","children":["heading","when","guests_label","guests","seating","name","row"]},{"id":"heading","component":"Text","text":"### Table at Olive & Thyme"},{"id":"when","component":"DateTimeInput","label":"When","enableDate":true,"enableTime":true,"value":{"@path":"/when"}},{"id":"guests_label","component":"Text","variant":"caption","text":{"@call":"pluralize","args":{"value":{"@path":"/guests"},"one":"1 guest","other":{"@call":"formatString","args":{"value":"${/guests} guests"}}}}},{"id":"guests","component":"Slider","label":"Guests","min":1,"max":12,"steps":11,"value":{"@path":"/guests"}},{"id":"seating","component":"ChoicePicker","label":"Seating","displayStyle":"chips","options":[{"label":"Indoor","value":"indoor"},{"label":"Terrace","value":"terrace"},{"label":"Bar","value":"bar"}],"value":{"@path":"/seating"}},{"id":"name","component":"TextField","label":"Name for the booking","value":{"@path":"/name"}},{"id":"row","component":"Row","justify":"spaceBetween","align":"center","children":["policy","book"]},{"id":"policy","component":"Modal","trigger":"policy_label","content":"policy_text"},{"id":"policy_label","component":"Text","text":"*Cancellation policy*"},{"id":"policy_text","component":"Text","text":"Free cancellation up to 2 hours before. Late cancellations are charged $10 per guest."},{"id":"book","component":"Button","variant":"primary","child":"book_label","action":{"event":{"name":"book","context":{"when":{"@path":"/when"},"guests":{"@path":"/guests"},"name":{"@path":"/name"}}}},"@checks":[{"condition":{"@call":"required","args":{"value":{"@path":"/name"}}},"message":"Add a name."}]},{"id":"book_label","component":"Text","text":"Book"}]}}
                ```
                """),
            OnAction = new Dictionary<string, string>
            {
                ["book"] = S("""
                    You're booked!
                    ```a2ui
                    {"version":"1.0","updateComponents":{"surfaceId":"booking_1","components":[{"id":"form","component":"Column","children":["heading","summary"]},{"id":"heading","component":"Text","text":"### Booked"},{"id":"summary","component":"Text","text":{"@call":"formatString","args":{"value":"${formatDate(value: ${/when}, format: 'EEEE, d MMMM \\'at\\' HH:mm')} · ${/guests} guests · ${/name}"}}}]}}
                    ```
                    """)
            }
        },
        new Showcase
        {
            Id = "preferences",
            Title = "Preferences",
            Description = "Tabs, check boxes, a filterable picker and multiple choice.",
            Prompt = "Open my notification and language settings.",
            Start = S("""
                Here are your settings:
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"prefs_1","catalogId":"{B}","sendDataModel":true,"dataModel":{"dark":true,"language":["en"],"channels":["email"],"digest":3},"components":[{"id":"root","component":"Card","child":"tabs"},{"id":"tabs","component":"Tabs","tabs":[{"title":"General","child":"general"},{"title":"Notifications","child":"notify"}]},{"id":"general","component":"Column","children":["dark","language"]},{"id":"dark","component":"CheckBox","label":"Dark appearance","value":{"@path":"/dark"}},{"id":"language","component":"ChoicePicker","label":"Language","filterable":true,"options":[{"label":"English","value":"en"},{"label":"简体中文","value":"zh-CN"},{"label":"繁體中文","value":"zh-TW"},{"label":"Deutsch","value":"de"},{"label":"Français","value":"fr"},{"label":"日本語","value":"ja"}],"value":{"@path":"/language"}},{"id":"notify","component":"Column","children":["channels","digest","save"]},{"id":"channels","component":"ChoicePicker","label":"Channels","variant":"multipleSelection","options":[{"label":"Email","value":"email"},{"label":"Push","value":"push"},{"label":"SMS","value":"sms"}],"value":{"@path":"/channels"}},{"id":"digest","component":"Slider","label":"Digests per week","min":0,"max":7,"steps":7,"value":{"@path":"/digest"}},{"id":"save","component":"Button","variant":"primary","child":"save_label","action":{"event":{"name":"save","userMessage":"Saved settings"}}},{"id":"save_label","component":"Text","text":"Save"}]}}
                ```
                """),
            OnAction = new Dictionary<string, string>
            {
                ["save"] = "Saved. Your settings apply right away."
            }
        },
        new Showcase
        {
            Id = "live",
            Title = "Live list",
            Description = "A list that grows as data streams in, with a metric bound to the same data.",
            Prompt = "Watch the build queue for me.",
            Start = S("""
                Watching the queue — new jobs appear as they arrive.
                ```a2ui
                {"version":"1.0","createSurface":{"surfaceId":"queue_1","catalogId":"{B}","dataModel":{"count":0,"jobs":[]},"components":[{"id":"root","component":"Column","children":["metrics","list"]},{"id":"metrics","component":"Row","children":["metric"]},{"id":"metric","component":"Metric","@catalogId":"{C}","label":"Jobs queued","value":{"@path":"/count"},"context":"live"},{"id":"list","component":"List","children":{"componentId":"job","path":"/jobs"}},{"id":"job","component":"Row","align":"center","children":["job_icon","job_text"]},{"id":"job_icon","component":"Icon","name":"refresh"},{"id":"job_text","component":"Text","text":{"@call":"formatString","args":{"value":"#${@index(offset: 1)} ${name} · ${branch}"}}}]}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/0","value":{"name":"build-web","branch":"main"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":1}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/1","value":{"name":"test-api","branch":"feature/login"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":2}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/2","value":{"name":"lint","branch":"main"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":3}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/3","value":{"name":"deploy-preview","branch":"feature/cart"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":4}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/4","value":{"name":"e2e","branch":"release/2.4"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":5}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/jobs/5","value":{"name":"docs","branch":"main"}}}
                {"version":"1.0","updateDataModel":{"surfaceId":"queue_1","path":"/count","value":6}}
                ```
                """)
        }
    ];
}
