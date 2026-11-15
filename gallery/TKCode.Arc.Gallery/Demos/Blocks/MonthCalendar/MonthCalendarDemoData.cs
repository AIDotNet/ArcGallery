namespace TKCode.Arc.Gallery.Demos;

/// <summary>
/// The MonthCalendar demo's workspace, people, calendars and events (all invented): a small design studio's month, built
/// around a given month so the demo always shows the current one. About forty events on five calendars, a few multi-day
/// ones, three invitations waiting for a reply, and a day (the 12th) with more events than its cell can hold.
/// </summary>
public static class MonthCalendarDemoData
{
    public const string Workspace = "Tidewater";
    public const string Me = "maya";

    public const string Studio = "studio";
    public const string Releases = "releases";
    public const string Home = "home";
    public const string Trips = "trips";
    public const string Holidays = "holidays";

    /// <summary>Five calendars with Lucide icons in their tones.</summary>
    public static List<MonthCalendarSource> CreateCalendars() =>
    [
        new(Studio, "Studio", MonthCalendarTone.Teal, ArcIcons.Parse(Briefcase)),
        new(Releases, "Releases", MonthCalendarTone.Violet, ArcIcons.Parse(Rocket)),
        new(Home, "Home", MonthCalendarTone.Amber, ArcIcons.Parse(Sprout)),
        new(Trips, "Trips", MonthCalendarTone.Blue, ArcIcons.Parse(Plane)),
        new(Holidays, "Holidays", MonthCalendarTone.Rose, ArcIcons.Flag),
    ];

    /// <summary>The signed-in person, five teammates and two outside guests (generated avatars).</summary>
    public static List<MonthCalendarPerson> CreatePeople() =>
    [
        new(Me, "Maya Okafor", "maya@tidewater.studio") { IsTeammate = false },
        new("jonah", "Jonah Reyes", "jonah@tidewater.studio"),
        new("priya", "Priya Natarajan", "priya@tidewater.studio"),
        new("lukas", "Lukas Brandt", "lukas@tidewater.studio"),
        new("amara", "Amara Osei", "amara@tidewater.studio"),
        new("theo", "Theo Lindqvist", "theo@tidewater.studio"),
        new("sana", "Sana Malik", "sana@ferrolabs.example") { IsTeammate = false },
        new("felix", "Felix Duarte", "felix@arborhealth.example") { IsTeammate = false },
    ];

    /// <summary>The events of the month that starts at <paramref name="month"/> (and a few days around it).</summary>
    public static List<MonthCalendarEvent> CreateEvents(DateOnly month)
    {
        var first = new DateOnly(month.Year, month.Month, 1);
        DateOnly D(int day) => first.AddDays(day - 1);
        var n = 0;
        MonthCalendarEvent E(int day, string title, string calendar, int? h = null, int m = 0, int? eh = null, int em = 0,
                             int toDay = 0, string? video = null, string? place = null, string? notes = null)
        {
            var e = new MonthCalendarEvent("e" + ++n, title, calendar, D(day),
                h is { } sh ? new TimeOnly(sh, m) : null, eh is { } ehh ? new TimeOnly(ehh, em) : null)
            {
                EndDate = toDay != 0 && toDay > day ? D(toDay) : null,
                VideoProvider = video,
                VideoUrl = video is null ? null : "https://video.example/" + title.ToLowerInvariant().Replace(' ', '-').Replace(":", ""),
                Location = place,
                Notes = notes,
                OrganizerId = Me,
            };
            return e;
        }
        MonthCalendarEvent With(MonthCalendarEvent e, params (string Id, MonthCalendarRsvp Status)[] guests)
        {
            foreach (var (id, status) in guests)
            {
                e.Attendees.Add(new MonthCalendarAttendee(id, status));
            }
            return e;
        }
        MonthCalendarEvent Invite(MonthCalendarEvent e, string from)
        {
            e.OrganizerId = from;
            e.Rsvp = MonthCalendarRsvp.Pending;
            return e;
        }
        const MonthCalendarRsvp Yes = MonthCalendarRsvp.Yes;
        const MonthCalendarRsvp Pending = MonthCalendarRsvp.Pending;
        const string Room = "Video room";

        return
        [
            // The week before the month.
            E(-3, "Roadmap check-in", Studio, 10, 0, 11, 0, video: Room),
            E(0, "Weekly wrap-up", Studio, 16, 0, 16, 45),
            E(-1, "Lantern night", Holidays),
            E(1, "All Saints' Day", Holidays),

            // Week 1.
            With(E(2, "Monday planning", Studio, 9, 30, 10, 15, video: Room), (Me, Yes), ("jonah", Yes), ("priya", Yes), ("lukas", Yes)),
            E(3, "Checkout flow review", Studio, 11, 0, 12, 0),
            E(4, "Morning swim", Home, 7, 0, 8, 0, place: "Harbour baths"),
            With(E(4, "Client call: Ferro Labs", Studio, 14, 0, 14, 45, video: Room), (Me, Yes), ("sana", Yes)),
            E(5, "Pricing copy freeze", Releases),
            E(5, "Dinner with Ines", Home, 19, 30, 21, 0, place: "Osteria Nove"),
            E(6, "Weekly wrap-up", Studio, 16, 0, 16, 45),
            E(7, "Weekend in Bergen", Trips, toDay: 8),

            // Week 2 (today is the 12th in the snapshots).
            With(E(9, "Monday planning", Studio, 9, 30, 10, 15, video: Room), (Me, Yes), ("jonah", Yes), ("priya", Yes)),
            With(E(9, "1:1 with Jonah", Studio, 15, 0, 15, 30, video: Room), (Me, Yes), ("jonah", Yes)),
            E(10, "Analyst notes review", Releases, 11, 0, 12, 0),
            E(10, "Dentist", Home, 17, 15, 18, 0),
            E(11, "Docs freeze", Releases),
            E(11, "Budget prep", Studio, 10, 0, 11, 0),
            E(12, "Quarter planning", Studio, toDay: 13, place: "Loft, 3rd floor",
                notes: "Two days on next quarter's bets. Bring one page per project."),
            With(E(12, "Daily sync", Studio, 9, 15, 9, 30, video: Room), (Me, Yes), ("jonah", Yes), ("priya", Yes), ("lukas", Yes), ("amara", Yes)),
            With(E(12, "Type review", Studio, 10, 0, 10, 45, video: Room), (Me, Yes), ("theo", Yes)),
            E(12, "Lunch with Ruth", Home, 12, 30, 13, 30, place: "Café Lindgren"),
            With(E(12, "User research sessions", Studio, 14, 0, 15, 30, video: Room,
                notes: "Five short sessions on the new onboarding. Notes go in the research folder."), (Me, Yes), ("amara", Yes)),
            With(E(12, "Ship or hold", Releases, 16, 30, 17, 0, video: Room), (Me, Yes), ("lukas", Yes), ("theo", Yes)),
            E(13, "Board update", Studio, 11, 0, 11, 45, video: Room),
            E(14, "Flea market", Home, 10, 0, 11, 30, place: "Market Square"),

            // Week 3: the release.
            E(16, "Release week", Releases, toDay: 20),
            E(16, "Release standup", Releases, 9, 0, 9, 20),
            With(Invite(E(16, "Partner sync: Arbor Health", Studio, 13, 0, 13, 45, video: Room,
                notes: "Walk through the shared reports before their team rolls them out."), "felix"),
                ("felix", Yes), (Me, Pending), ("amara", Yes)),
            E(17, "Version 4 goes live", Releases, 8, 0, 8, 30),
            E(17, "Analyst briefing", Releases, 11, 0, 12, 0),
            With(Invite(E(18, "Interview panel: product designer", Studio, 10, 0, 11, 0, video: Room,
                notes: "Portfolio review with the final candidate. Scorecards are in the hiring folder."), "priya"),
                ("priya", Yes), (Me, Pending), ("jonah", Yes)),
            E(18, "Webinar: what is new", Releases, 15, 0, 16, 0, video: Room),
            E(19, "Release party", Releases, 19, 0, 23, 0, place: "Roof terrace"),
            E(20, "Release retro", Releases, 16, 0, 17, 0),
            E(21, "Noor's birthday", Home),

            // Week 4: the conference.
            With(E(23, "Monday planning", Studio, 9, 30, 10, 15, video: Room), (Me, Yes), ("jonah", Yes)),
            E(24, "Design Days Lisbon", Trips, toDay: 26, place: "Lisbon"),
            E(24, "Flight TP 541 to Lisbon", Trips, 7, 5, 8, 35),
            E(25, "Talk: shipping small", Studio, 14, 0, 14, 40),
            E(26, "Harvest holiday", Holidays),
            E(27, "Flight TP 548 home", Trips, 17, 40, 19, 5),
            E(28, "Brunch with Amara", Home, 10, 30, 12, 0, place: "Bakery Hus"),

            // The last week and the first days after the month.
            With(E(30, "Monday planning", Studio, 9, 30, 10, 15, video: Room), (Me, Yes), ("jonah", Yes)),
            E(31, "Payroll run", Studio, 12, 0, 12, 30),
            With(Invite(E(33, "Retreat planning", Studio, 15, 0, 16, 0, video: Room,
                notes: "Pick the venue and the dates for the winter retreat."), "amara"),
                ("amara", Yes), (Me, Pending), ("jonah", Yes), ("theo", Pending)),
            E(34, "Studio winter dinner", Studio, 18, 30, 21, 0),
            E(36, "Winter market opens", Holidays),
        ];
    }

    // Lucide icons not in Core's subset (Lucide, ISC license).
    private const string Briefcase = "M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16 M4 6h16a2 2 0 0 1 2 2v10a2 2 0 0 1 -2 2h-16a2 2 0 0 1 -2 -2v-10a2 2 0 0 1 2 -2z";
    private const string Rocket = "M12 15v5s3.03-.55 4-2c1.08-1.62 0-5 0-5 M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09 M9 12a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.4 22.4 0 0 1-4 2z M9 12H4s.55-3.03 2-4c1.62-1.08 5 .05 5 .05";
    private const string Sprout = "M14 9.536V7a4 4 0 0 1 4-4h1.5a.5.5 0 0 1 .5.5V5a4 4 0 0 1-4 4 4 4 0 0 0-4 4c0 2 1 3 1 5a5 5 0 0 1-1 3 M4 9a5 5 0 0 1 8 4 5 5 0 0 1-8-4 M5 21h14";
    private const string Plane = "M17.8 19.2 16 11l3.5-3.5C21 6 21.5 4 21 3c-1-.5-3 0-4.5 1.5L13 8 4.8 6.2c-.5-.1-.9.1-1.1.5l-.3.5c-.2.5-.1 1 .3 1.3L9 12l-2 3H4l-1 1 3 2 2 3 1-1v-3l3-2 3.5 5.3c.3.4.8.5 1.3.3l.5-.2c.4-.3.6-.7.5-1.2z";
}
