namespace BuildTruckProjectService.Projects.Application.ACL.Services;

/// <summary>
/// ACL Service Interface for Cloudinary Integration
/// </summary>
/// <remarks>
/// Provides abstraction for Projects Context to interact with Cloudinary
/// without direct coupling to Shared Context implementation
/// </remarks>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload image to Cloudinary
    /// </summary>
    /// <param name="imageFile">Image file to upload</param>
    /// <param name="fileName">File name for the image</param>
    /// <returns>Upload result with success status and image URL</returns>
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile imageFile, string fileName);

    /// <summary>
    /// Delete image from Cloudinary
    /// </summary>
    /// <param name="imageUrl">Image URL to delete</param>
    /// <returns>True if deletion successful, false otherwise</returns>
    Task<bool> DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Get optimized image URL with transformations
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <param name="width">Desired width</param>
    /// <param name="height">Desired height</param>
    /// <returns>Optimized image URL</returns>
    string GetOptimizedImageUrl(string publicId, int width = 800, int height = 600);

    /// <summary>
    /// Generate thumbnail URL
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <param name="size">Thumbnail size (default 200px)</param>
    /// <returns>Thumbnail URL</returns>
    string GenerateThumbnailUrl(string publicId, int size = 200);
}

/// <summary>
/// Result of Cloudinary upload operation
/// </summary>
/// <remarks>
/// Encapsulates upload result without exposing Cloudinary-specific details
/// </remarks>
public record CloudinaryUploadResult
{
    public bool IsSuccess { get; init; }
    public string? ImageUrl { get; init; }
    public string? PublicId { get; init; }
    public string? Error { get; init; }
    public long? FileSize { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string? Format { get; init; }

    public static CloudinaryUploadResult Success(string imageUrl, string publicId, long? fileSize = null, 
        int? width = null, int? height = null, string? format = null)
    {
        return new CloudinaryUploadResult
        {
            IsSuccess = true,
            ImageUrl = imageUrl,
            PublicId = publicId,
            FileSize = fileSize,
            Width = width,
            Height = height,
            Format = format
        };
    }

    public static CloudinaryUploadResult Failure(string error)
    {
        return new CloudinaryUploadResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}