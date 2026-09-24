using BuildTruckUserService.Users.Domain.Model.Aggregates;

namespace BuildTruckUserService.Users.Application.ACL.Services;

/// <summary>
/// Image service interface specific to Users domain
/// Translates User domain concepts to external image services
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Upload profile image for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="imageBytes">Image file bytes</param>
    /// <param name="fileName">Original file name</param>
    /// <returns>Public URL of uploaded image</returns>
    Task<string> UploadUserProfileImageAsync(User user, byte[] imageBytes, string fileName);

    /// <summary>
    /// Delete current profile image for a user
    /// </summary>
    /// <param name="user">User entity with current profile image URL</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteUserProfileImageAsync(User user);

    /// <summary>
    /// Get optimized profile image URL for display
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="size">Desired image size (width and height)</param>
    /// <returns>Optimized image URL or default avatar URL</returns>
    string GetUserProfileImageUrl(User user, int size = 200);

    /// <summary>
    /// Validate image file for user profile
    /// </summary>
    /// <param name="imageBytes">Image file bytes</param>
    /// <param name="fileName">File name with extension</param>
    /// <returns>Validation result with user-friendly error message</returns>
    (bool IsValid, string ErrorMessage) ValidateUserProfileImage(byte[] imageBytes, string fileName);
}