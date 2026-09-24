using BuildTruckUserService.Auth.Domain.Model.Commands;
using BuildTruckUserService.Auth.Domain.Model.ValueObjects;

namespace BuildTruckUserService.Auth.Domain.Services;

/// <summary>
/// Auth Command Service Interface
/// </summary>
/// <remarks>
/// Domain service interface for authentication commands
/// </remarks>
public interface IAuthCommandService
{
    /// <summary>
    /// Handle user sign-in command
    /// </summary>
    /// <param name="command">Sign-in command with credentials and audit info</param>
    /// <returns>AuthToken if successful, null if authentication fails</returns>
    Task<AuthToken?> HandleSignInAsync(SignInCommand command);

    /// <summary>
    /// Handle send password reset command
    /// </summary>
    /// <param name="command">Password reset request command with email and audit info</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> HandleSendPasswordResetAsync(SendPasswordResetCommand command);

    /// <summary>
    /// Handle reset password command
    /// </summary>
    /// <param name="command">Reset password command with token, email, new password and audit info</param>
    /// <returns>True if password was reset successfully, false otherwise</returns>
    Task<bool> HandleResetPasswordAsync(ResetPasswordCommand command);
}