namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public record UserRole
{
    public static readonly UserRole Admin = new("Admin");
    public static readonly UserRole Manager = new("Manager");
    public static readonly UserRole Supervisor = new("Supervisor");

    public string Value { get; init; }

    private UserRole(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
    private UserRole() { Value = string.Empty; }

    public bool IsAdmin() => Value == Admin.Value;
    public bool IsManager() => Value == Manager.Value;
    public bool IsSupervisor() => Value == Supervisor.Value;
    public bool CanManageProjects() => IsAdmin() || IsManager();
    public bool CanSuperviseProjects() => IsAdmin() || IsManager() || IsSupervisor();
    public bool HasSystemAccess() => IsAdmin();

    public static UserRole FromString(string value) =>
        GetAllRoles().FirstOrDefault(r => r.Value == value)
        ?? throw new ArgumentException($"Invalid user role: {value}");

    public static IEnumerable<UserRole> GetAllRoles() => new[] { Admin, Manager, Supervisor };
    public static IEnumerable<UserRole> GetProjectRoles() => new[] { Manager, Supervisor };
}
