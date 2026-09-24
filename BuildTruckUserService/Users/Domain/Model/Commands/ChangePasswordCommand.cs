namespace BuildTruckUserService.Users.Domain.Model.Commands;

/// <summary>
/// Command to change user password
/// </summary>
/// <param name="UserId">The user ID</param>
/// <param name="CurrentPassword">Current password for verification</param>
/// <param name="NewPassword">New password</param>
public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword
);