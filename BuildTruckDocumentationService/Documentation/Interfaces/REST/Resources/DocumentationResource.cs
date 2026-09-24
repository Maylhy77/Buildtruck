namespace BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;

/// <summary>
/// Resource for documentation data responses
/// </summary>
public record DocumentationResource(
    int Id,
    int ProjectId,
    string Title,
    string Description,
    string ImagePath,
    DateTime Date,
    int CreatedBy,
    
    // Computed fields
    bool HasValidImage,
    string FormattedDate,
    
    // Timestamps
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt
);