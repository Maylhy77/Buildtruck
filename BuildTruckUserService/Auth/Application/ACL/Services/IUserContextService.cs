using BuildTruckUserService.Auth.Domain.Model.ValueObjects;

namespace BuildTruckUserService.Auth.Application.ACL.Services;

/// <summary>
/// ACL Service interface for communication with Users Context
/// </summary>
/// <remarks>
/// Anti-Corruption Layer that isolates Auth Context from Users Context implementation details
/// </remarks>
public interface IUserContextService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <param name="password">Plain text password</param>
    /// <returns>AuthenticatedUser if credentials are valid, null otherwise</returns>
    Task<AuthenticatedUser?> AuthenticateUserAsync(string email, string password);

    /// <summary>
    /// Get user information by user ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>AuthenticatedUser if found, null otherwise</returns>
    Task<AuthenticatedUser?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>True if updated successfully, false otherwise</returns>
    Task<bool> UpdateUserLastLoginAsync(int userId);

    /// <summary>
    /// Check if user is active and can authenticate
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <returns>True if user is active, false otherwise</returns>
    Task<bool> IsUserActiveAsync(string email);

    /// <summary>
    /// Get user profile image URL with specified size
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="size">Image size in pixels (default: 200)</param>
    /// <returns>Profile image URL or null if not available</returns>
    Task<string?> GetUserProfileImageUrlAsync(int userId, int size = 200);

    /// <summary>
    /// Get user information by email address
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <returns>AuthenticatedUser if found, null otherwise</returns>
    Task<AuthenticatedUser?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Send password reset email to user
    /// </summary>
    /// <param name="user">User to send email to</param>
    /// <param name="resetToken">Password reset token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPasswordResetEmailAsync(AuthenticatedUser user, string resetToken);

    /// <summary>
    /// Reset user password with new password
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="newPassword">New plain text password</param>
    /// <returns>True if password was reset successfully, false otherwise</returns>
    Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
}