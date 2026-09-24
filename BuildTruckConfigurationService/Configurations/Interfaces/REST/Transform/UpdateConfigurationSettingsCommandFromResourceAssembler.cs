using BuildTruckConfigurationService.Configurations.Domain.Model.Commands;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;

namespace BuildTruckConfigurationService.Configurations.Interfaces.REST.Transform;

/// <summary>
/// Assembler for UpdateConfigurationSettingsCommand from UpdateConfigurationSettingsResource
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public static class UpdateConfigurationSettingsCommandFromResourceAssembler
{
    public static UpdateConfigurationSettingsCommand ToCommandFromResource(UpdateConfigurationSettingsResource resource, int id)
    {
        bool ParseToBoolean(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.Equals("true", StringComparison.OrdinalIgnoreCase) && !value.Equals("false", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid boolean value: {value}. Must be 'true' or 'false'.");
            return value.ToLower() == "true";
        }
        
        if (!Enum.TryParse<Theme>(resource.Theme, out var theme))
            throw new ArgumentException($"Invalid theme: {resource.Theme}");
        if (!Enum.TryParse<Plan>(resource.Plan, out var plan))
            throw new ArgumentException($"Invalid plan: {resource.Plan}");

        return new UpdateConfigurationSettingsCommand(
            Ids: id, 
            UserIds: resource.UserId, 
            theme,
            plan,
            NotificationsEnables: ParseToBoolean(resource.NotificationsEnable),
            EmailNotification: ParseToBoolean(resource.EmailNotifications),
            TutorialsCompleted: new TutorialProgress(resource.TutorialsCompleted)); 
    }
}
