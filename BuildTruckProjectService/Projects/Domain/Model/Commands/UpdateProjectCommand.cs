using BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

namespace BuildTruckProjectService.Projects.Domain.Model.Commands;

/// <summary>
/// Command for updating an existing project
/// </summary>
/// <remarks>
/// Contains all necessary data to update a project with business validations
/// </remarks>
public record UpdateProjectCommand
{
    public int ProjectId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Location { get; init; }
    public DateTime? StartDate { get; init; }
    public object? Coordinates { get; init; } // Object for Mapbox compatibility
    public IFormFile? ImageFile { get; init; } // For new image upload
    public bool RemoveImage { get; init; } // Flag to remove existing image
    public int? SupervisorId { get; init; } // New supervisor assignment
    public bool RemoveSupervisor { get; init; } // Flag to remove supervisor
    public string? State { get; init; } // New state

    public UpdateProjectCommand() { }

    public UpdateProjectCommand(
        int projectId,
        string? name = null,
        string? description = null,
        string? location = null,
        DateTime? startDate = null,
        object? coordinates = null,
        IFormFile? imageFile = null,
        bool removeImage = false,
        int? supervisorId = null,
        bool removeSupervisor = false,
        string? state = null)
    {
        ProjectId = projectId;
        Name = name;
        Description = description;
        Location = location;
        StartDate = startDate;
        Coordinates = coordinates;
        ImageFile = imageFile;
        RemoveImage = removeImage;
        SupervisorId = supervisorId;
        RemoveSupervisor = removeSupervisor;
        State = state;
    }

    // Business validation methods
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ProjectId <= 0)
            errors.Add("ProjectId must be greater than 0");

        // Validate Name if provided
        if (!string.IsNullOrWhiteSpace(Name))
        {
            try
            {
                _ = new ProjectName(Name);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Name validation failed: {ex.Message}");
            }
        }

        // Validate Description if provided
        if (!string.IsNullOrWhiteSpace(Description))
        {
            try
            {
                _ = new ProjectDescription(Description);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Description validation failed: {ex.Message}");
            }
        }

        // Validate Location if provided
        if (!string.IsNullOrWhiteSpace(Location))
        {
            try
            {
                _ = new ProjectLocation(Location);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Location validation failed: {ex.Message}");
            }
        }

        // Validate State if provided
        if (!string.IsNullOrWhiteSpace(State))
        {
            try
            {
                _ = new ProjectState(State);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"State validation failed: {ex.Message}");
            }
        }

        if (SupervisorId.HasValue && SupervisorId <= 0)
            errors.Add("SupervisorId must be greater than 0 when provided");

        if (StartDate.HasValue && StartDate < DateTime.Now.Date.AddDays(-1))
            errors.Add("Start date cannot be in the past");

        // Validate coordinates if provided
        if (Coordinates != null)
        {
            var coordinates = ProjectCoordinates.FromObject(Coordinates);
            if (coordinates == null)
                errors.Add("Invalid coordinates format");
            else if (!coordinates.IsValid())
                errors.Add("Coordinates are outside valid range");
        }

        // Validate image file if provided
        if (ImageFile != null)
        {
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (ImageFile.Length > maxFileSize)
                errors.Add("Image file size cannot exceed 5MB");

            var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                errors.Add("Image file must be JPG, PNG, or WebP format");
        }

        // Business rule validations
        if (RemoveImage && ImageFile != null)
            errors.Add("Cannot remove image and upload new image simultaneously");

        if (RemoveSupervisor && SupervisorId.HasValue)
            errors.Add("Cannot remove supervisor and assign new supervisor simultaneously");

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    // Helper methods for business logic
    public bool HasChanges()
    {
        return !string.IsNullOrWhiteSpace(Name) ||
               !string.IsNullOrWhiteSpace(Description) ||
               !string.IsNullOrWhiteSpace(Location) ||
               StartDate.HasValue ||
               Coordinates != null ||
               ImageFile != null ||
               RemoveImage ||
               SupervisorId.HasValue ||
               RemoveSupervisor ||
               !string.IsNullOrWhiteSpace(State);
    }

    public bool RequiresStateValidation()
    {
        return !string.IsNullOrWhiteSpace(State);
    }

    public bool RequiresSupervisorValidation()
    {
        return SupervisorId.HasValue || RemoveSupervisor;
    }

    public bool RequiresImageHandling()
    {
        return ImageFile != null || RemoveImage;
    }

    public bool HasImageToUpload() => ImageFile != null && ImageFile.Length > 0;

    public ProjectCoordinates? GetValidCoordinates()
    {
        return ProjectCoordinates.FromObject(Coordinates);
    }

    public ProjectState? GetValidState()
    {
        if (string.IsNullOrWhiteSpace(State))
            return null;

        try
        {
            return new ProjectState(State);
        }
        catch
        {
            return null;
        }
    }

    public bool IsProjectInPeru()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(Location))
            {
                var location = new ProjectLocation(Location);
                if (location.IsInPeru())
                    return true;
            }

            var coordinates = GetValidCoordinates();
            return coordinates?.IsInPeru() ?? false;
        }
        catch
        {
            return false;
        }
    }
}