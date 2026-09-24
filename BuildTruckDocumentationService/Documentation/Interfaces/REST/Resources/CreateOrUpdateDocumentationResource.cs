using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating or updating documentation with image upload support
/// </summary>
public class CreateOrUpdateDocumentationResource
{
    /// <summary>
    /// Documentation ID for updates (null for create)
    /// </summary>
    public int? Id { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Project ID must be greater than 0")]
    public int ProjectId { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime Date { get; init; }

    /// <summary>
    /// Image file for documentation (max 5MB, JPG/PNG/WebP/GIF)
    /// Required for create, optional for update
    /// </summary>
    public IFormFile? ImageFile { get; init; }

    /// <summary>
    /// Validation method for business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate date
        if (Date < DateTime.Now.Date.AddYears(-5))
        {
            errors.Add("Date cannot be more than 5 years in the past");
        }

        if (Date > DateTime.Now.Date.AddDays(1))
        {
            errors.Add("Date cannot be in the future");
        }

        // Validate image file
        if (ImageFile != null)
        {
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            if (ImageFile.Length > maxFileSize)
                errors.Add("Image file size cannot exceed 5MB");

            var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                errors.Add("Image file must be JPG, PNG, WebP, or GIF format");

            if (ImageFile.Length == 0)
                errors.Add("Image file cannot be empty");
        }
        else if (!Id.HasValue) // Creating new documentation
        {
            errors.Add("Image file is required for new documentation");
        }

        // Validate title format
        if (!string.IsNullOrEmpty(Title) && Title.Trim() != Title)
        {
            errors.Add("Title cannot start or end with whitespace");
        }

        // Validate description format
        if (!string.IsNullOrEmpty(Description) && Description.Trim() != Description)
        {
            errors.Add("Description cannot start or end with whitespace");
        }

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    public bool IsUpdate() => Id.HasValue && Id.Value > 0;

    public bool IsCreate() => !Id.HasValue;
}