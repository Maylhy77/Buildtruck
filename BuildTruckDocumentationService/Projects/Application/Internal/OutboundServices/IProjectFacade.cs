namespace BuildTruckDocumentationService.Projects.Application.Internal.OutboundServices;

public interface IProjectFacade
{
    Task<bool> ExistsByIdAsync(int projectId);
    Task<ProjectInfo?> GetProjectByIdAsync(int projectId);
    Task<bool> UserHasAccessToProjectAsync(int userId, int projectId);
}

public record ProjectInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("managerId")]
    public int ManagerId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("supervisorId")]
    public int? SupervisorId { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("startDate")]
    public DateTime? StartDate { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("hasSupervisor")]
    public bool HasSupervisor { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("isReadyToStart")]
    public bool IsReadyToStart { get; init; }
}
