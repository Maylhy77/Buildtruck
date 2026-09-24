namespace BuildTruckConfigurationService.Configurations.Application.ACL.Services;

public interface INotificationContextService
{
    Task NotifyConfigurationCreatedAsync(int userId);
}
