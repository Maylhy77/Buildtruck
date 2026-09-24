using BuildTruckUserService.Users.Domain.Model.Aggregates;

namespace BuildTruckUserService.Users.Application.Internal.OutboundServices;

/// <summary>
/// User Facade - Interface for communication between bounded contexts
/// Provides a simplified interface for other contexts to interact with Users
/// </summary>
public interface IUserFacade
{
    /// <summary>
    /// Verify user credentials for authentication
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <param name="password">Plain text password</param>
    /// <returns>User if credentials are valid, null otherwise</returns>
    Task<User?> VerifyCredentialsAsync(string email, string password);

    /// <summary>
    /// Find user by corporate email
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// Find user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> FindByIdAsync(int userId);

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateLastLoginAsync(int userId);

    /// <summary>
    /// Check if user exists and is active
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <returns>True if user exists and is active</returns>
    Task<bool> IsActiveUserAsync(string email);

    /// <summary>
    /// Send password reset email (for Auth context)
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <returns>Success status</returns>
    Task SendPasswordResetEmailAsync(int userId, string email, string fullName, string resetToken);
    
    /// <summary>
    /// Get user's profile image URL for display
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="size">Image size (default 200px)</param>
    /// <returns>Optimized image URL or default avatar</returns>
    Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200);

    Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
}