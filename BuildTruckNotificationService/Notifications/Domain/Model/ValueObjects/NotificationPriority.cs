namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public record NotificationPriority
{
    public static readonly NotificationPriority Low = new("LOW", 1, false);
    public static readonly NotificationPriority Normal = new("NORMAL", 2, false);
    public static readonly NotificationPriority High = new("HIGH", 3, true);
    public static readonly NotificationPriority Critical = new("CRITICAL", 4, true);

    public string Value { get; init; }
    public int Level { get; init; }
    public bool RequiresEmailByDefault { get; init; }

    private NotificationPriority(string value, int level, bool requiresEmailByDefault)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Level = level;
        RequiresEmailByDefault = requiresEmailByDefault;
    }
    private NotificationPriority() { Value = string.Empty; Level = 0; RequiresEmailByDefault = false; }

    public bool IsHigherThan(NotificationPriority other) => Level > other.Level;
    public bool IsLowerThan(NotificationPriority other) => Level < other.Level;
    public bool IsHighPriority() => Level >= High.Level;
    public bool IsCritical() => Level >= Critical.Level;

    public static NotificationPriority FromString(string value) =>
        GetAllPriorities().FirstOrDefault(p => p.Value == value)
        ?? throw new ArgumentException($"Invalid notification priority: {value}");

    public static NotificationPriority FromLevel(int level) =>
        GetAllPriorities().FirstOrDefault(p => p.Level == level)
        ?? throw new ArgumentException($"Invalid priority level: {level}");

    public static IEnumerable<NotificationPriority> GetAllPriorities() =>
        new[] { Low, Normal, High, Critical };
}
