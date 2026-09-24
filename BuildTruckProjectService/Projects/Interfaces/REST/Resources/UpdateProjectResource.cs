using System.ComponentModel.DataAnnotations;

namespace BuildTruckProjectService.Projects.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating an existing project
/// </summary>
/// <remarks>
/// Contains optional fields for partial project updates
/// </remarks>
public record UpdateProjectResource
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 100 characters")]
    public string? Name { get; init; }

    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Project description must be between 10 and 1000 characters")]
    public string? Description { get; init; }

    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project location must be between 3 and 200 characters")]
    public string? Location { get; init; }

    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Coordinates from Mapbox: {lat: -12.0464, lng: -77.0428}
    /// </summary>
    public object? Coordinates { get; init; }

    /// <summary>
    /// New image file for project (max 5MB, JPG/PNG/WebP)
    /// </summary>
    public IFormFile? ImageFile { get; init; }

    /// <summary>
    /// Flag to remove existing image
    /// </summary>
    public bool RemoveImage { get; init; }

    /// <summary>
    /// New supervisor assignment
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Supervisor ID must be greater than 0")]
    public int? SupervisorId { get; init; }

    /// <summary>
    /// Flag to remove current supervisor
    /// </summary>
    public bool RemoveSupervisor { get; init; }

    /// <summary>
    /// New project state: "En estudio", "Planificado", "Activo", "Completado"
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// Validation method for business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate state if provided
        if (!string.IsNullOrWhiteSpace(State))
        {
            var validStates = new[] { "En estudio", "Planificado", "Activo", "Completado" };
            if (!validStates.Contains(State))
            {
                errors.Add($"Invalid state. Valid states: {string.Join(", ", validStates)}");
            }
        }

        // Validate start date
        if (StartDate.HasValue && StartDate < DateTime.Now.Date.AddDays(-1))
        {
            errors.Add("Start date cannot be in the past");
        }

        // Validate image file
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
}