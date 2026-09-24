namespace BuildTruckUserService.Auth.Interfaces.REST.Resources;

/// <summary>
/// Authenticated user resource
/// </summary>
/// <remarks>
/// Resource representing an authenticated user's information
/// </remarks>
public record AuthenticatedUserResource
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Corporate email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User role (Admin, Manager, Supervisor, Worker)
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Profile image URL (optional)
    /// </summary>
    public string? ProfileImageUrl { get; init; }

    /// <summary>
    /// Last login timestamp (optional)
    /// </summary>
    public DateTime? LastLogin { get; init; }

    /// <summary>
    /// Localized role display name
    /// </summary>
    public string RoleDisplay { get; init; } = string.Empty;

    /// <summary>
    /// User permissions/capabilities
    /// </summary>
    public UserCapabilitiesResource Capabilities { get; init; } = new();
}

/// <summary>
/// User capabilities resource
/// </summary>
/// <remarks>
/// Represents what the user can do based on their role
/// </remarks>
public record UserCapabilitiesResource
{
    /// <summary>
    /// Can manage projects (Admin, Manager)
    /// </summary>
    public bool CanManageProjects { get; init; }

    /// <summary>
    /// Can be assigned to projects (Supervisor)
    /// </summary>
    public bool CanBeAssignedToProject { get; init; }

    /// <summary>
    /// Is administrator (Admin only)
    /// </summary>
    public bool IsAdmin { get; init; }

    /// <summary>
    /// Is manager (Manager only)
    /// </summary>
    public bool IsManager { get; init; }

    /// <summary>
    /// Is supervisor (Supervisor only)
    /// </summary>
    public bool IsSupervisor { get; init; }

    /// <summary>
    /// Is worker (Worker only)
    /// </summary>
    public bool IsWorker { get; init; }
}