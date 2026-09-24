namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public record NotificationContext
{
    public static readonly NotificationContext System = new("SYSTEM");
    public static readonly NotificationContext Projects = new("PROJECTS");
    public static readonly NotificationContext Personnel = new("PERSONNEL");
    public static readonly NotificationContext Materials = new("MATERIALS");
    public static readonly NotificationContext Machinery = new("MACHINERY");
    public static readonly NotificationContext Incidents = new("INCIDENTS");

    public string Value { get; }

    private NotificationContext(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
    private NotificationContext() { Value = string.Empty; }

    public bool IsSystem() => Value == System.Value;
    public bool IsProject() => Value == Projects.Value;
    public bool IsPersonnel() => Value == Personnel.Value;
    public bool IsMaterials() => Value == Materials.Value;
    public bool IsMachinery() => Value == Machinery.Value;
    public bool IsIncidents() => Value == Incidents.Value;

    public static NotificationContext FromString(string value) =>
        GetAllContexts().FirstOrDefault(c => c.Value == value)
        ?? throw new ArgumentException($"Invalid notification context: {value}");

    public static IEnumerable<NotificationContext> GetAllContexts() =>
        new[] { System, Projects, Personnel, Materials, Machinery, Incidents };

    public static IEnumerable<NotificationContext> GetProjectContexts() =>
        new[] { Projects, Personnel, Materials, Machinery, Incidents };
}
