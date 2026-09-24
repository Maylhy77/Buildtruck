namespace BuildTruckUserService.Users.Domain.Model.ValueObjects;

/// <summary>
/// UserRole Value Object
/// </summary>
/// <remarks>
/// Represents user roles in BuildTruck platform with validation
/// </remarks>
public record UserRole
{
    // ✅ PRIMERO: Definir ValidRoles
    private static readonly HashSet<string> ValidRoles = new()
    {
        "Admin", "Manager", "Supervisor", "Worker"
    };

    // ✅ DESPUÉS: Instancias estáticas (ValidRoles ya existe)
    public static readonly UserRole Admin = new("Admin");
    public static readonly UserRole Manager = new("Manager");
    public static readonly UserRole Supervisor = new("Supervisor");
    public static readonly UserRole Worker = new("Worker");

    public string Role { get; init; }

    public UserRole()
    {
        Role = "Worker"; // Default role
    }

    public UserRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));

        if (!ValidRoles.Contains(role))
            throw new ArgumentException($"Invalid role: {role}. Valid roles: {string.Join(", ", ValidRoles)}", nameof(role));

        Role = role;
    }

    // Helper methods
    public bool IsAdmin => Role == "Admin";
    public bool IsManager => Role == "Manager";
    public bool IsSupervisor => Role == "Supervisor";
    public bool IsWorker => Role == "Worker";
    
    public bool CanManageProjects => IsAdmin || IsManager;
    public bool CanBeAssignedToProject => IsSupervisor;

    public override string ToString() => Role;
}