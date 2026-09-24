namespace BuildTruckProjectService.Projects.Interfaces.REST.Resources;

/// <summary>
/// Resource representing a project for API responses
/// </summary>
/// <remarks>
/// Contains all project data optimized for frontend consumption
/// </remarks>
public record ProjectResource
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int ManagerId { get; init; }
    public string ManagerName { get; init; } = string.Empty;
    public string ManagerEmail { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string ShortLocation { get; init; } = string.Empty;
    public ProjectCoordinatesResource? Coordinates { get; init; }
    public DateTime? StartDate { get; init; }
    public int? SupervisorId { get; init; }
    public string? SupervisorName { get; init; }
    public string? SupervisorEmail { get; init; }
    public string State { get; init; } = string.Empty;
    public string StateDisplayName { get; init; } = string.Empty;
    public string StateColor { get; init; } = string.Empty;
    
    // Calculated properties
    public bool HasSupervisor { get; init; }
    public bool HasCoordinates { get; init; }
    public bool HasImage { get; init; }
    public bool IsActive { get; init; }
    public bool IsCompleted { get; init; }
    public bool CanBeDeleted { get; init; }
    public bool IsReadyToStart { get; init; }
    public bool IsOverdue { get; init; }
    public int DaysUntilStart { get; init; }
    
    // Project summary and metadata
    public string ProjectSummary { get; init; } = string.Empty;
    public string DescriptionSummary { get; init; } = string.Empty;
    
    // Audit fields
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    
    // Business status
    public List<string> ValidationErrors { get; init; } = new();
    public bool IsValid { get; init; }
}

/// <summary>
/// Resource for project coordinates (Mapbox compatible)
/// </summary>
public record ProjectCoordinatesResource
{
    public double Lat { get; init; }
    public double Lng { get; init; }
    public bool IsInPeru { get; init; }
    public double DistanceToLima { get; init; }
    public string GoogleMapsUrl { get; init; } = string.Empty;
    public string DecimalString { get; init; } = string.Empty;
}

/// <summary>
/// Simplified project resource for lists and summaries
/// </summary>
public record ProjectSummaryResource
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DescriptionSummary { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string ManagerName { get; init; } = string.Empty;
    public string ShortLocation { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public string? SupervisorName { get; init; }
    public string State { get; init; } = string.Empty;
    public string StateDisplayName { get; init; } = string.Empty;
    public string StateColor { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsOverdue { get; init; }
    public int DaysUntilStart { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

/// <summary>
/// Resource for paginated project results
/// </summary>
public record ProjectPagedResource
{
    public List<ProjectSummaryResource> Projects { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}