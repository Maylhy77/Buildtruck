using BuildTruckUserService.Users.Domain.Model.Aggregates;

namespace BuildTruckUserService.Users.Application.ACL.Services;

/// <summary>
/// Email service interface specific to Users domain
/// Anti-Corruption Layer for email operations in Users bounded context
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send user credentials to their personal email address
    /// </summary>
    /// <param name="user">User domain aggregate containing all necessary information</param>
    /// <param name="temporalPassword">Generated temporal password</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SendUserCredentialsAsync(User user, string temporalPassword);
    /// <summary>
    /// Send password reset email with token
    /// </summary>
    /// <param name="user">User requesting password reset</param>
    /// <param name="resetToken">Password reset token</param>
    /// <returns>Success status</returns>
    Task SendPasswordResetEmailAsync(User user, string resetToken);
}