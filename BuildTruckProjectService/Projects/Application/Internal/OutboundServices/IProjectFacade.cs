namespace BuildTruckProjectService.Projects.Application.Internal.OutboundServices;

/// <summary>
/// Project Facade - Interface for communication between bounded contexts
/// Provides project information and access control for other contexts
/// </summary>
public interface IProjectFacade
{
    /// <summary>
    /// Check if a project exists by ID
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>True if project exists, false otherwise</returns>
    Task<bool> ExistsByIdAsync(int projectId);

    /// <summary>
    /// Get project information by ID
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>Project information or null if not found</returns>
    Task<ProjectInfo?> GetProjectByIdAsync(int projectId);

    /// <summary>
    /// Check if a user has access to a project (as manager or supervisor)
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="projectId">Project identifier</param>
    /// <returns>True if user has access, false otherwise</returns>
    Task<bool> UserHasAccessToProjectAsync(int userId, int projectId);

    /// <summary>
    /// Get all projects where user is manager or supervisor
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>List of projects the user has access to</returns>
    Task<List<ProjectInfo>> GetProjectsByUserAsync(int userId);

    /// <summary>
    /// Get projects managed by a specific user
    /// </summary>
    /// <param name="managerId">Manager identifier</param>
    /// <returns>List of projects managed by the user</returns>
    Task<List<ProjectInfo>> GetProjectsByManagerAsync(int managerId);

    /// <summary>
    /// Get projects supervised by a specific user
    /// </summary>
    /// <param name="supervisorId">Supervisor identifier</param>
    /// <returns>List of projects supervised by the user</returns>
    Task<List<ProjectInfo>> GetProjectsBySupervisorAsync(int supervisorId);

    /// <summary>
    /// Get count of projects by state
    /// </summary>
    /// <param name="state">Project state</param>
    /// <returns>Number of projects in the specified state</returns>
    Task<int> GetProjectCountByStateAsync(string state);

    /// <summary>
    /// Get count of active projects
    /// </summary>
    /// <returns>Number of active projects</returns>
    Task<int> GetActiveProjectsCountAsync();

    /// <summary>
    /// Get projects by state
    /// </summary>
    /// <param name="state">Project state</param>
    /// <returns>List of projects in the specified state</returns>
    Task<List<ProjectInfo>> GetProjectsByStateAsync(string state);

    /// <summary>
    /// Get all projects with basic information
    /// </summary>
    /// <returns>List of all projects</returns>
    Task<List<ProjectInfo>> GetAllProjectsAsync();
}

/// <summary>
/// Project information for cross-context communication
/// </summary>
public record ProjectInfo
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public int ManagerId { get; init; }
    public int? SupervisorId { get; init; }
    public DateTime? StartDate { get; init; }
    public string? ImageUrl { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public bool IsActive { get; init; }
    public bool HasSupervisor { get; init; }
    public bool IsReadyToStart { get; init; }
}