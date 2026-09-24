using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BuildTruckPersonnelService.Personnel.Interfaces.REST.Resources;

public class CreatePersonnelResource
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Project ID must be greater than 0")]
    public int ProjectId { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Lastname { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 5)]
    public string DocumentNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Position { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Department { get; init; } = string.Empty;

    [Required]
    public string PersonnelType { get; init; } = string.Empty;

    [Required]
    public string Status { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal MonthlyAmount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Discount { get; init; }

    [StringLength(50)]
    public string? Bank { get; init; }

    [StringLength(50)]
    public string? AccountNumber { get; init; }

    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    [StringLength(20)]
    public string? Phone { get; init; }

    [StringLength(150)]
    [EmailAddress]
    public string? Email { get; init; }

    public IFormFile? ImageFile { get; init; }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        var validPersonnelTypes = new[] { "TECHNICAL", "SPECIALIST", "ADMINISTRATIVE", "RENTED_OPERATOR", "LABORER" };
        if (!validPersonnelTypes.Contains(PersonnelType))
            errors.Add($"Invalid personnel type. Valid types: {string.Join(", ", validPersonnelTypes)}");

        var validStatuses = new[] { "ACTIVE", "INACTIVE", "PENDING", "SUSPENDED", "FINISHED" };
        if (!validStatuses.Contains(Status))
            errors.Add($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            errors.Add("Start date cannot be later than end date");

        if (ImageFile != null)
        {
            const long maxFileSize = 5 * 1024 * 1024;
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
