using BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

namespace BuildTruckProjectService.Projects.Domain.Model.Commands;

/// <summary>
/// Command for creating a new project
/// </summary>
/// <remarks>
/// Contains all necessary data to create a project with business validations
/// </remarks>
public record CreateProjectCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ManagerId { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public object? Coordinates { get; init; } // Object for Mapbox compatibility
    public IFormFile? ImageFile { get; init; } // For image upload
    public int? SupervisorId { get; init; } // Optional supervisor assignment
    public string State { get; init; } = "En estudio"; // Default state

    public CreateProjectCommand() { }

    public CreateProjectCommand(
        string name,
        string description,
        int managerId,
        string location,
        DateTime? startDate = null,
        object? coordinates = null,
        IFormFile? imageFile = null,
        int? supervisorId = null,
        string state = "En estudio")
    {
        Name = name;
        Description = description;
        ManagerId = managerId;
        Location = location;
        StartDate = startDate;
        Coordinates = coordinates;
        ImageFile = imageFile;
        SupervisorId = supervisorId;
        State = state;
    }

    // Business validation methods
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        try
        {
            // Validate using Value Objects
            _ = new ProjectName(Name);
        }
        catch (ArgumentException ex)
        {
            errors.Add($"Name validation failed: {ex.Message}");
        }

        try
        {
            _ = new ProjectDescription(Description);
        }
        catch (ArgumentException ex)
        {
            errors.Add($"Description validation failed: {ex.Message}");
        }

        try
        {
            _ = new ProjectLocation(Location);
        }
        catch (ArgumentException ex)
        {
            errors.Add($"Location validation failed: {ex.Message}");
        }

        try
        {
            _ = new ProjectState(State);
        }
        catch (ArgumentException ex)
        {
            errors.Add($"State validation failed: {ex.Message}");
        }

        if (ManagerId <= 0)
            errors.Add("ManagerId must be greater than 0");

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

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    // Helper methods for business logic
    public bool RequiresSupervisorValidation()
    {
        var projectState = new ProjectState(State);
        return projectState.RequiresSupervisor() && !SupervisorId.HasValue;
    }

    public bool HasImageToUpload() => ImageFile != null && ImageFile.Length > 0;

    public ProjectCoordinates? GetValidCoordinates()
    {
        return ProjectCoordinates.FromObject(Coordinates);
    }

    public bool IsProjectInPeru()
    {
        try
        {
            var location = new ProjectLocation(Location);
            var coordinates = GetValidCoordinates();
            
            return location.IsInPeru() || (coordinates?.IsInPeru() ?? false);
        }
        catch
        {
            return false;
        }
    }
}