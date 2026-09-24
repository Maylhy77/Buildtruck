using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckMachineryService.Machinery.Infrastructure.ACL;

/// <summary>
/// ACL Adapter for Cloudinary specific to Machinery context
/// Uses shared CloudinaryImageService with documentation/ folder
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<CloudinaryService> _logger;
    
    private const string DOCUMENTATION_FOLDER = "buildtruck/documentation/";

    public CloudinaryService(ICloudinaryImageService cloudinaryImageService, ILogger<CloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    public async Task<string> UploadMachineryImageAsync(byte[] imageBytes, string fileName, int documentationId)
    {
        try
        {
            _logger.LogDebug("Uploading documentation image: {FileName} for documentation: {MachineryId}", fileName, documentationId);

            // Generate unique filename with documentation ID
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"doc_{documentationId}_{timestamp}{extension}";

            var imageUrl = await _cloudinaryImageService.UploadImageAsync(imageBytes, uniqueFileName, DOCUMENTATION_FOLDER);
            
            _logger.LogInformation("✅ Machinery image uploaded successfully: {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to upload documentation image for documentation: {MachineryId}", documentationId);
            throw new InvalidOperationException($"Failed to upload documentation image: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteMachineryImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
            {
                _logger.LogWarning("Invalid image URL for deletion: {ImageUrl}", imageUrl);
                return false;
            }

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Could not extract public ID from URL: {ImageUrl}", imageUrl);
                return false;
            }

            var success = await _cloudinaryImageService.DeleteImageAsync(publicId);
            
            if (success)
            {
                _logger.LogInformation("✅ Machinery image deleted successfully: {ImageUrl}", imageUrl);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to delete documentation image: {ImageUrl}", imageUrl);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting documentation image: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public string GetOptimizedImageUrl(string imageUrl, int width = 400, int height = 300)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                return imageUrl;

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
                return imageUrl;

            var optimizedUrl = _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
            
            _logger.LogDebug("Generated optimized URL for documentation image: {OptimizedUrl}", optimizedUrl);
            return optimizedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating optimized URL for: {ImageUrl}", imageUrl);
            return imageUrl;
        }
    }
}