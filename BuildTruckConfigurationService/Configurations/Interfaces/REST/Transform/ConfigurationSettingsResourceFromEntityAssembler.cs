using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;

namespace BuildTruckConfigurationService.Configurations.Interfaces.REST.Transform;

/// <summary>
/// Assembler for ConfigurationSettingsResource from ConfigurationSettings entity
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public static class ConfigurationSettingsResourceFromEntityAssembler
{
    public static ConfigurationSettingsResource ToResourceFromEntity(ConfigurationSettings configurationSettings)
    {
        return new ConfigurationSettingsResource(
            configurationSettings.Id,
            configurationSettings.UserId,
            configurationSettings.Themes.ToString(),
            configurationSettings.Plans.ToString(),
            configurationSettings.NotificationsEnable.ToString().ToLower(),
            configurationSettings.EmailNotifications.ToString().ToLower(),
            configurationSettings.TutorialsCompleted.ToJsonString()
        );
        /*{
            Id = entity.Id,
            UserId = entity.UserId,
            Theme = Enum.Parse<Theme>(entity.Theme, true),
            Plan = Enum.Parse<Plan>(entity.Plan, true),
            NotificationsEnable = entity.NotificationsEnable.ToString().ToLower(),
            EmailNotifications = entity.EmailNotifications.ToString().ToLower()
        };
        */
    }
}
