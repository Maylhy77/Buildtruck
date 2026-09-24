namespace BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;

public record NotificationType
{
    public static readonly NotificationType SystemNotification = new("SYSTEM_NOTIFICATION", UserRole.Admin, NotificationScope.System, false);
    public static readonly NotificationType UserRegistered = new("USER_REGISTERED", UserRole.Admin, NotificationScope.System, false);
    public static readonly NotificationType UserInactive = new("USER_INACTIVE", UserRole.Admin, NotificationScope.System, false);
    public static readonly NotificationType ProjectCreated = new("PROJECT_CREATED", UserRole.Admin, NotificationScope.System, false);
    public static readonly NotificationType SystemError = new("SYSTEM_ERROR", UserRole.Admin, NotificationScope.System, false);
    public static readonly NotificationType ProjectAssigned = new("PROJECT_ASSIGNED", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType ProjectStatusChanged = new("PROJECT_STATUS_CHANGED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType ProjectStartingSoon = new("PROJECT_STARTING_SOON", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType ProjectOverdue = new("PROJECT_OVERDUE", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType PersonnelAdded = new("PERSONNEL_ADDED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType PersonnelRemoved = new("PERSONNEL_REMOVED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType AttendanceAlert = new("ATTENDANCE_ALERT", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType PaymentProcessed = new("PAYMENT_PROCESSED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType PersonnelInactive = new("PERSONNEL_INACTIVE", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType LowStock = new("LOW_STOCK", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType CriticalStock = new("CRITICAL_STOCK", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType MaterialAdded = new("MATERIAL_ADDED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType MaterialUsed = new("MATERIAL_USED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType HighCostAlert = new("HIGH_COST_ALERT", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType MachineryAssigned = new("MACHINERY_ASSIGNED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType MachineryUnassigned = new("MACHINERY_UNASSIGNED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType MachineryMaintenance = new("MACHINERY_MAINTENANCE", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType MachineryAvailable = new("MACHINERY_AVAILABLE", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType MachineryStatusChanged = new("MACHINERY_STATUS_CHANGED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType IncidentReported = new("INCIDENT_REPORTED", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType IncidentAssigned = new("INCIDENT_ASSIGNED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType CriticalIncident = new("CRITICAL_INCIDENT", UserRole.Manager, NotificationScope.Project, false);
    public static readonly NotificationType IncidentResolved = new("INCIDENT_RESOLVED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType IncidentSeverityChanged = new("INCIDENT_SEVERITY_CHANGED", UserRole.Manager, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorPersonnelAdded = new("PERSONNEL_ADDED", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorPersonnelRemoved = new("PERSONNEL_REMOVED", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorAttendanceAlert = new("ATTENDANCE_ALERT", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorLowStock = new("LOW_STOCK", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorCriticalStock = new("CRITICAL_STOCK", UserRole.Supervisor, NotificationScope.Project, false);
    public static readonly NotificationType SupervisorMachineryAssigned = new("MACHINERY_ASSIGNED", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorMachineryMaintenance = new("MACHINERY_MAINTENANCE", UserRole.Supervisor, NotificationScope.Project, false);
    public static readonly NotificationType SupervisorIncidentReported = new("INCIDENT_REPORTED", UserRole.Supervisor, NotificationScope.Project, false);
    public static readonly NotificationType SupervisorIncidentAssigned = new("INCIDENT_ASSIGNED", UserRole.Supervisor, NotificationScope.Project, true);
    public static readonly NotificationType SupervisorCriticalIncident = new("CRITICAL_INCIDENT", UserRole.Supervisor, NotificationScope.Project, false);

    public string Value { get; init; }
    public UserRole TargetRole { get; init; }
    public NotificationScope Scope { get; init; }
    public bool CanBeDisabled { get; init; }

    private NotificationType(string value, UserRole targetRole, NotificationScope scope, bool canBeDisabled)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        TargetRole = targetRole ?? throw new ArgumentNullException(nameof(targetRole));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        CanBeDisabled = canBeDisabled;
    }

    private NotificationType() { Value = string.Empty; TargetRole = UserRole.Admin; Scope = NotificationScope.System; CanBeDisabled = false; }

    public bool IsSystemLevel() => Scope == NotificationScope.System;
    public bool IsProjectLevel() => Scope == NotificationScope.Project;
    public bool IsForAdmin() => TargetRole == UserRole.Admin;
    public bool IsForManager() => TargetRole == UserRole.Manager;
    public bool IsForSupervisor() => TargetRole == UserRole.Supervisor;

    public static NotificationType FromString(string value) =>
        GetAllTypes().FirstOrDefault(t => t.Value == value)
        ?? throw new ArgumentException($"Invalid notification type: {value}");

    public static IEnumerable<NotificationType> GetAllTypes() =>
        typeof(NotificationType)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(NotificationType))
            .Select(f => (NotificationType)f.GetValue(null)!)
            .Where(t => t != null);

    public static IEnumerable<NotificationType> GetTypesForRole(UserRole role) =>
        GetAllTypes().Where(t => t.TargetRole == role);
}
