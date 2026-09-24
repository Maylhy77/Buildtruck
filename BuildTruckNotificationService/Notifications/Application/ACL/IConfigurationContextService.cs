namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IConfigurationContextService
{
    Task<string?> GetSettingAsync(string key);
    Task<bool> IsFeatureEnabledAsync(string featureKey);
}
