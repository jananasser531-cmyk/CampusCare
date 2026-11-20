namespace CampusCare.Frontend.SampleData;

public sealed record Ticket(
    string Number,
    string Subject,
    string Description,
    string Requester,
    string Location,
    string Category,
    string Priority,
    string Status,
    string Agent,
    DateTime SubmittedOn,
    DateTime UpdatedOn,
    DateTime SlaDueOn);

public sealed record TicketActivity(DateTime At, string Kind, string Actor, string Text, bool Internal = false);
public sealed record Notice(string Id, string TicketNumber, string Kind, string Category, string Text, DateTime At, bool IsRead);
public sealed record SampleUser(string Name, string Role, string Email, string State);

public static class DemoData
{
    // Explicitly illustrative SLA model: 48 hours from submission. This is a demo threshold only,
    // not a CampusCare policy. Sample dates are fixed to keep overdue examples stable.
    public const int SampleSlaHours = 48;
    public const string SampleAgent = "Sara Khalil";

    public static readonly Ticket[] Tickets =
    [
        new("CC-0142", "AC unit not cooling", "The air conditioning unit in room 214 stopped cooling during afternoon classes.", "Nour Hassan", "Engineering, Building B, Room 214", "HVAC", "High", "Open", SampleAgent,
            new(2026,11,16,8,0,0,DateTimeKind.Utc), new(2026,11,16,9,30,0,DateTimeKind.Utc), new(2026,11,18,8,0,0,DateTimeKind.Utc)),
        new("CC-0139", "Window latch broken", "The window latch is loose and the window will not stay closed.", "Karim Adel", "Science A, Room 105", "Carpentry", "Medium", "In progress", SampleAgent,
            new(2026,11,13,10,15,0,DateTimeKind.Utc), new(2026,11,16,11,0,0,DateTimeKind.Utc), new(2026,11,15,10,15,0,DateTimeKind.Utc)),
        new("CC-0131", "Flickering lights in reading room", "Several ceiling lights flicker in the east reading area.", "Mariam Samir", "Main library, second floor", "Electrical", "Low", "Waiting for requester", SampleAgent,
            new(2026,11,14,7,0,0,DateTimeKind.Utc), new(2026,11,16,13,0,0,DateTimeKind.Utc), new(2026,11,16,7,0,0,DateTimeKind.Utc)),
        new("CC-0127", "Leaking tap in washroom", "A tap continues to leak after being turned off.", "Omar Fathy", "Engineering, Building A, ground floor", "Plumbing", "High", "Resolved", "Omar Hassan",
            new(2026,11,10,9,0,0,DateTimeKind.Utc), new(2026,11,12,14,0,0,DateTimeKind.Utc), new(2026,11,12,9,0,0,DateTimeKind.Utc)),
        new("CC-0124", "Damaged corridor bench", "One bench slat is cracked and needs inspection.", "Hana Mostafa", "Student Center, west corridor", "Carpentry", "Low", "Closed", "Mina Adel",
            new(2026,11,9,8,30,0,DateTimeKind.Utc), new(2026,11,11,16,0,0,DateTimeKind.Utc), new(2026,11,11,8,30,0,DateTimeKind.Utc))
    ];

    public static readonly string[] Agents = ["Sara Khalil", "Omar Hassan", "Mina Adel", "Unassigned"];
    public static readonly string[] Categories = ["HVAC", "Carpentry", "Electrical", "Plumbing", "Cleaning"];
    public static readonly string[] Statuses = ["Open", "In progress", "Waiting for requester", "Resolved", "Closed"];
    public static readonly string[] Priorities = ["Low", "Medium", "High"];
    public static readonly string[] Roles = ["Requester", "SupportAgent", "Administrator"];

    public static readonly SampleUser[] Users =
    [
        new("Sara Khalil", "SupportAgent", "sara.khalil@campuscare.edu", "Active"),
        new("Omar Hassan", "SupportAgent", "omar.hassan@campuscare.edu", "Active"),
        new("Mina Adel", "Administrator", "mina.adel@campuscare.edu", "Active"),
        new("Youssef Nabil", "Requester", "youssef.nabil@campuscare.edu", "Active")
    ];

    public static readonly IReadOnlyDictionary<string, TicketActivity[]> Activity = new Dictionary<string, TicketActivity[]>
    {
        ["CC-0142"] = [
            new(new(2026,11,16,8,0,0,DateTimeKind.Utc), "Status", "System sample", "Ticket submitted as Open"),
            new(new(2026,11,16,8,20,0,DateTimeKind.Utc), "Assignment", "Admin sample", "Assigned to Sara Khalil"),
            new(new(2026,11,16,9,30,0,DateTimeKind.Utc), "Comment", "Sara Khalil", "We are checking the unit and will update you shortly."),
            new(new(2026,11,16,10,0,0,DateTimeKind.Utc), "Internal note", "Sara Khalil", "Sample staff note: inspect filter and thermostat.", true)
        ],
        ["CC-0139"] = [
            new(new(2026,11,13,10,15,0,DateTimeKind.Utc), "Status", "System sample", "Ticket submitted as Open"),
            new(new(2026,11,13,11,0,0,DateTimeKind.Utc), "Assignment", "Admin sample", "Assigned to Sara Khalil"),
            new(new(2026,11,16,11,0,0,DateTimeKind.Utc), "Status", "Sara Khalil", "Status changed to In progress")
        ]
    };

    public static readonly Notice[] Notifications =
    [
        new("N-01", "CC-0142", "Assignment", "Assigned ticket", "CC-0142 was assigned to you", new(2026,11,16,8,20,0,DateTimeKind.Utc), false),
        new("N-02", "CC-0139", "Status", "Status changed", "CC-0139 moved to In progress", new(2026,11,16,11,0,0,DateTimeKind.Utc), false),
        new("N-03", "CC-0131", "Comment", "New comment", "A requester added a sample comment to CC-0131", new(2026,11,15,15,45,0,DateTimeKind.Utc), true)
    ];

    public static string DisplayDate(DateTime value) => value.ToString("dd MMM yyyy, HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " UTC";
}
