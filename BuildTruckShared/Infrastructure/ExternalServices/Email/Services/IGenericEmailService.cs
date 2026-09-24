namespace BuildTruckShared.Infrastructure.ExternalServices.Email.Services;

public interface IGenericEmailService
{
    Task SendWelcomeEmailAsync(string email, string fullName, string temporalPassword);
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
    Task SendEmailAsync(string email, string subject, string htmlBody);
}
