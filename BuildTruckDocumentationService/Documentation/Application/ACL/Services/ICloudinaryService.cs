namespace BuildTruckDocumentationService.Documentation.Application.ACL.Services;

/// <summary>
/// Cloudinary service interface for Documentation context
/// Handles documentation image uploads to cloudinary.com/documentation/
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload documentation image
    /// </summary>
    /// <param name="imageBytes">Image as byte array</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="documentationId">Documentation ID for unique naming</param>
    /// <returns>Cloudinary URL</returns>
    Task<string> UploadDocumentationImageAsync(byte[] imageBytes, string fileName, int documentationId);

    /// <summary>
    /// Delete documentation image
    /// </summary>
    /// <param name="imageUrl">Cloudinary URL</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteDocumentationImageAsync(string imageUrl);

    /// <summary>
    /// Get optimized documentation image URL
    /// </summary>
    /// <param name="imageUrl">Original Cloudinary URL</param>
    /// <param name="width">Target width (default: 400)</param>
    /// <param name="height">Target height (default: 300)</param>
    /// <returns>Optimized URL</returns>
    string GetOptimizedImageUrl(string imageUrl, int width = 400, int height = 300);
}