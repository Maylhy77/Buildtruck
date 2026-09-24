namespace BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;

public interface IProjectFacade
{
    Task<bool> ExistsByIdAsync(int projectId);
    Task<ProjectInfo?> GetProjectByIdAsync(int projectId);
    Task<bool> UserHasAccessToProjectAsync(int userId, int projectId);
}

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
    public bool IsReadyToStart { get; init; }
}
