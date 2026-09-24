using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a configuration settings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public record CreateConfigurationSettingsResource
{
    [Required]
    public int UserId { get; init; }

    [Required]
    [SwaggerSchema("Theme. Valid values: light, dark, auto")]
    //[JsonConverter(typeof(JsonStringEnumConverter))]
    public string Theme { get; init; } = string.Empty;

    [Required]
    [SwaggerSchema("Plan. Valid values: basic, business, enterprise")]
    //[JsonConverter(typeof(JsonStringEnumConverter))]
    public string Plan { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(true|false)$", ErrorMessage = "NotificationsEnable must be 'true' or 'false'.")]
    public string NotificationsEnable { get; init; } = "true";

    [Required]
    [RegularExpression("^(true|false)$", ErrorMessage = "EmailNotifications must be 'true' or 'false'.")]
    public string EmailNotifications { get; init; } = "false";
    
    [Required]
    [RegularExpression(@"^\{.*\}$", ErrorMessage = "TutorialsCompleted must be a valid JSON object.")]
    public string TutorialsCompleted { get; init; } = "{}";
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        // Validate theme
        var validPlans = new[] { "Basic", "Business", "Enterprise" };
        if (!validPlans.Contains(Plan))
        {
            errors.Add($"Invalid theme. Valid plans: {string.Join(", ", validPlans)}");
        }

        // Validate theme
        var validThemes = new[] { "Light", "Dark", "Auto" };
        if (!validThemes.Contains(Theme))
        {
            errors.Add($"Invalid theme. Valid themes: {string.Join(", ", validThemes)}");
        }

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}
