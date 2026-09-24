namespace BuildTruckProjectService.Projects.Domain.Model.Commands;

/// <summary>
/// Command for deleting a project
/// </summary>
/// <remarks>
/// Contains necessary data to delete a project with business validations
/// </remarks>
public record DeleteProjectCommand
{
    public int ProjectId { get; init; }
    public int RequestedByUserId { get; init; } // User requesting the deletion
    public bool ForceDelete { get; init; } // Force delete even with dependencies
    public string? Reason { get; init; } // Optional reason for deletion

    public DeleteProjectCommand() { }

    public DeleteProjectCommand(
        int projectId,
        int requestedByUserId,
        bool forceDelete = false,
        string? reason = null)
    {
        ProjectId = projectId;
        RequestedByUserId = requestedByUserId;
        ForceDelete = forceDelete;
        Reason = reason;
    }

    // Business validation methods
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ProjectId <= 0)
            errors.Add("ProjectId must be greater than 0");

        if (RequestedByUserId <= 0)
            errors.Add("RequestedByUserId must be greater than 0");

        // Validate reason length if provided
        if (!string.IsNullOrWhiteSpace(Reason) && Reason.Length > 500)
            errors.Add("Deletion reason cannot exceed 500 characters");

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    // Helper methods for business logic
    public bool HasReason() => !string.IsNullOrWhiteSpace(Reason);

    public string GetNormalizedReason()
    {
        return string.IsNullOrWhiteSpace(Reason) 
            ? "No reason provided" 
            : Reason.Trim();
    }

    public bool RequiresSpecialPermissions()
    {
        return ForceDelete; // Force delete might require admin permissions
    }
}