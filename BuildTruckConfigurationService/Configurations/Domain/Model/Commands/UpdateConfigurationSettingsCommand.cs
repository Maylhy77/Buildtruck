using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;

namespace BuildTruckConfigurationService.Configurations.Domain.Model.Commands;

/// <summary>
/// Command to update a configuration settings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public record UpdateConfigurationSettingsCommand(
    int Ids,
    int UserIds,
    Theme Themes,
    Plan Plans,
    bool NotificationsEnables,
    bool EmailNotification,
    TutorialProgress TutorialsCompleted);
