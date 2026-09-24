using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckUserService.Users.Application.ACL.Services;

/// <summary>
/// Image Service Adapter (Anti-Corruption Layer)
/// Translates Users domain concepts to Cloudinary external service
/// </summary>
public class ImageServiceAdapter : IImageService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<ImageServiceAdapter> _logger;

    // 🎯 Domain-specific constants for Users
    private const string USERS_FOLDER = "buildtruck/profiles/";
    private const string DEFAULT_AVATAR_URL = "https://via.placeholder.com/200x200/f97316/ffffff?text=BT";

    public ImageServiceAdapter(
        ICloudinaryImageService cloudinaryImageService,
        ILogger<ImageServiceAdapter> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload profile image for a user (Domain-specific logic)
    /// Automatically deletes previous image before uploading new one
    /// </summary>
    public async Task<string> UploadUserProfileImageAsync(User user, byte[] imageBytes, string fileName)
    {
        try
        {
            _logger.LogInformation("Uploading profile image for user {UserId} via ACL", user.Id);

            // ✅ Delete previous image first (if exists)
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !IsDefaultAvatar(user.ProfileImageUrl))
            {
                _logger.LogInformation("Deleting previous profile image for user {UserId}", user.Id);
                await DeleteUserProfileImageAsync(user);
            }

            // ✅ Domain-specific filename generation
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var domainSpecificFileName = $"user_{user.Id}_profile_{timestamp}{extension}";

            // ✅ Delegate to generic Cloudinary service
            var imageUrl = await _cloudinaryImageService.UploadImageAsync(
                imageBytes,
                domainSpecificFileName,
                USERS_FOLDER);

            _logger.LogInformation("✅ Profile image uploaded successfully for user {UserId}: {ImageUrl}",
                user.Id, imageUrl);

            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to upload profile image for user {UserId} via ACL", user.Id);
            throw new InvalidOperationException(
                $"Failed to upload profile image for user {user.FullName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete current profile image for a user
    /// </summary>
    public async Task<bool> DeleteUserProfileImageAsync(User user)
    {
        try
        {
            if (string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                _logger.LogInformation("No profile image to delete for user {UserId}", user.Id);
                return true; // Nothing to delete is considered success
            }

            // ✅ Domain logic: Don't delete default avatar
            if (IsDefaultAvatar(user.ProfileImageUrl))
            {
                _logger.LogInformation("User {UserId} has default avatar, no deletion needed", user.Id);
                return true;
            }

            _logger.LogInformation("Deleting profile image for user {UserId} via ACL", user.Id);

            // ✅ Extract publicId and delegate to Cloudinary
            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(user.ProfileImageUrl);
            var success = await _cloudinaryImageService.DeleteImageAsync(publicId);

            if (success)
            {
                _logger.LogInformation("✅ Profile image deleted successfully for user {UserId}", user.Id);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to delete profile image for user {UserId}", user.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting profile image for user {UserId} via ACL", user.Id);
            return false; // Don't throw on delete failures
        }
    }

    /// <summary>
    /// Get optimized profile image URL for display (Domain-specific logic)
    /// </summary>
    public string GetUserProfileImageUrl(User user, int size = 200)
    {
        try
        {
            // ✅ Domain logic: Return default avatar if no image
            if (string.IsNullOrEmpty(user.ProfileImageUrl) || IsDefaultAvatar(user.ProfileImageUrl))
            {
                return GenerateDefaultAvatarUrl(user, size);
            }

            // ✅ Extract publicId and get optimized URL
            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(user.ProfileImageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Could not extract publicId from URL for user {UserId}, using default avatar",
                    user.Id);
                return GenerateDefaultAvatarUrl(user, size);
            }

            return _cloudinaryImageService.GetOptimizedImageUrl(publicId, size, size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting optimized image URL for user {UserId}, using default", user.Id);
            return GenerateDefaultAvatarUrl(user, size);
        }
    }

    /// <summary>
    /// Validate image file for user profile (Domain-specific validation)
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateUserProfileImage(byte[] imageBytes, string fileName)
    {
        // ✅ Use generic Cloudinary validation
        var (isValid, errorMessage) = _cloudinaryImageService.ValidateImage(imageBytes, fileName);

        if (!isValid)
        {
            // ✅ Domain-specific error messages for better UX
            if (errorMessage.Contains("size exceeds"))
            {
                return (false, "La imagen de perfil es demasiado grande. El tamaño máximo es 5MB.");
            }

            if (errorMessage.Contains("not allowed"))
            {
                return (false, "Formato de imagen no válido. Solo se permiten archivos JPG y PNG.");
            }

            if (errorMessage.Contains("not appear to be"))
            {
                return (false, "El archivo no parece ser una imagen válida.");
            }
        }

        return (isValid, errorMessage);
    }

    /// <summary>
    /// Check if URL is the default avatar
    /// </summary>
    private static bool IsDefaultAvatar(string imageUrl)
    {
        return string.IsNullOrEmpty(imageUrl) ||
               imageUrl.Contains("placeholder") ||
               imageUrl == DEFAULT_AVATAR_URL;
    }

    /// <summary>
    /// Generate default avatar URL with user initials
    /// </summary>
    private static string GenerateDefaultAvatarUrl(User user, int size)
    {
        // ✅ Domain-specific default avatar with user initials
        var initials = GetUserInitials(user.FullName);
        return $"https://via.placeholder.com/{size}x{size}/f97316/ffffff?text={initials}";
    }

    /// <summary>
    /// Extract user initials from full name
    /// </summary>
    private static string GetUserInitials(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return "BT"; // BuildTruck default

        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return nameParts.Length switch
        {
            1 => nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length)).ToUpper(),
            >= 2 => $"{nameParts[0][0]}{nameParts[^1][0]}".ToUpper(),
            _ => "BT"
        };
    }
}