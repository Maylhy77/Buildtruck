using System.Text.Json.Serialization;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;

namespace BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;

/// <summary>
/// Resource for representing a configuration settings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public record ConfigurationSettingsResource(
    int Id,
    int UserId,
    string Theme,
    string Plan,
    string NotificationsEnables,
    string EmailNotification,
    string TutorialsCompleted
        );


/*
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public Theme Theme { get; init; }
    public Plan Plan { get; init; }
    public string NotificationsEnable { get; init; } = "true";
    public string EmailNotifications { get; init; } = "false";
}
*/
