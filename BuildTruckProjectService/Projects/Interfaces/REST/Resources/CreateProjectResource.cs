using System.ComponentModel.DataAnnotations;

namespace BuildTruckProjectService.Projects.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a new project
/// </summary>
/// <remarks>
/// Contains all necessary data from frontend to create a project
/// </remarks>
public record CreateProjectResource
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Project name must be between 2 and 100 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Project description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Project description must be between 10 and 1000 characters")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "Manager ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Manager ID must be greater than 0")]
    public int ManagerId { get; init; }

    [Required(ErrorMessage = "Project location is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project location must be between 3 and 200 characters")]
    public string Location { get; init; } = string.Empty;

    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Coordinates from Mapbox: {lat: -12.0464, lng: -77.0428}
    /// </summary>
    public object? Coordinates { get; init; }

    /// <summary>
    /// Image file for project (max 5MB, JPG/PNG/WebP)
    /// </summary>
    public IFormFile? ImageFile { get; init; }

    /// <summary>
    /// Optional supervisor assignment
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Supervisor ID must be greater than 0")]
    public int? SupervisorId { get; init; }

    /// <summary>
    /// Project state: "En estudio", "Planificado", "Activo", "Completado"
    /// </summary>
    [Required(ErrorMessage = "Project state is required")]
    public string State { get; init; } = "En estudio";

    /// <summary>
    /// Validation method for additional business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate state
        var validStates = new[] { "En estudio", "Planificado", "Activo", "Completado" };
        if (!validStates.Contains(State))
        {
            errors.Add($"Invalid state. Valid states: {string.Join(", ", validStates)}");
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

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}