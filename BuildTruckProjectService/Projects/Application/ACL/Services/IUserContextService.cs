namespace BuildTruckProjectService.Projects.Application.ACL.Services;

/// <summary>
/// ACL Service Interface for User Context Integration
/// </summary>
/// <remarks>
/// Provides abstraction for Projects Context to interact with Users Context
/// without direct coupling - follows Anti-Corruption Layer pattern
/// </remarks>
public interface IUserContextService
{
    /// <summary>
    /// Find user by ID
    /// </summary>
    /// <param name="userId">User ID to find</param>
    /// <returns>User information or null if not found</returns>
    Task<UserDto?> FindByIdAsync(int userId);

    /// <summary>
    /// Check if user is active
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is active, false otherwise</returns>
    Task<bool> IsActiveUserAsync(int userId);

    /// <summary>
    /// Check if user has Manager role
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is Manager, false otherwise</returns>
    Task<bool> IsManagerAsync(int userId);

    /// <summary>
    /// Check if user has Supervisor role
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is Supervisor, false otherwise</returns>
    Task<bool> IsSupervisorAsync(int userId);

    /// <summary>
    /// Check if user has Admin role
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user is Admin, false otherwise</returns>
    Task<bool> IsAdminAsync(int userId);

    /// <summary>
    /// Check if supervisor is available for assignment
    /// </summary>
    /// <param name="supervisorId">Supervisor ID to check</param>
    /// <returns>True if supervisor is available, false otherwise</returns>
    Task<bool> IsSupervisorAvailableAsync(int supervisorId);

    /// <summary>
    /// Find an available supervisor for automatic assignment
    /// </summary>
    /// <returns>Available supervisor ID or null if none available</returns>
    Task<int?> FindAvailableSupervisorAsync();

    /// <summary>
    /// Assign supervisor to a project
    /// </summary>
    /// <param name="supervisorId">Supervisor ID to assign</param>
    /// <param name="projectId">Project ID to assign to</param>
    /// <returns>True if assignment successful, false otherwise</returns>
    Task<bool> AssignSupervisorToProjectAsync(int supervisorId, int projectId);

    /// <summary>
    /// Release supervisor from current project assignment
    /// </summary>
    /// <param name="supervisorId">Supervisor ID to release</param>
    /// <returns>True if release successful, false otherwise</returns>
    Task<bool> ReleaseSupervisorFromProjectAsync(int supervisorId);

    /// <summary>
    /// Get user's profile image URL
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="size">Image size (default 200px)</param>
    /// <returns>Profile image URL or default image</returns>
    Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200);
}

/// <summary>
/// DTO for User data transfer between contexts
/// </summary>
/// <remarks>
/// Represents user data without exposing internal User entity structure
/// </remarks>
public record UserDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? ProfileImageUrl { get; init; }
    public int? CurrentProjectId { get; init; } // For supervisors
    public DateTimeOffset? LastLogin { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}