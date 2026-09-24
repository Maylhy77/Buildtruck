using BuildTruckProjectService.Projects.Application.ACL.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckProjectService.Projects.Infrastructure.ACL;

/// <summary>
/// Cloudinary Service Implementation for Projects Context
/// </summary>
/// <remarks>
/// ACL adapter that wraps the Shared Context ICloudinaryImageService
/// for Projects Context specific needs
/// </remarks>
public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<CloudinaryService> _logger;
    
    // Projects specific folder structure
    private const string ProjectsFolder = "buildtruck/projects/projectImages/";

    public CloudinaryService(
        ICloudinaryImageService cloudinaryImageService,
        ILogger<CloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile imageFile, string fileName)
    {
        try
        {
            if (imageFile == null || imageFile.Length == 0)
                return CloudinaryUploadResult.Failure("Image file is empty or null");

            // Convert IFormFile to byte array
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // Validate image using Shared service
            var validation = _cloudinaryImageService.ValidateImage(imageBytes, fileName);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Image validation failed for {FileName}: {Error}", fileName, validation.ErrorMessage);
                return CloudinaryUploadResult.Failure(validation.ErrorMessage);
            }

            // Upload to Cloudinary
            var imageUrl = await _cloudinaryImageService.UploadImageAsync(imageBytes, fileName, ProjectsFolder);
            
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogError("Cloudinary upload returned empty URL for {FileName}", fileName);
                return CloudinaryUploadResult.Failure("Upload failed: no URL returned");
            }

            // Extract public ID from URL for future operations
            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);

            _logger.LogInformation("✅ Project image uploaded successfully: {FileName} -> {PublicId}", fileName, publicId);

            return CloudinaryUploadResult.Success(
                imageUrl: imageUrl,
                publicId: publicId,
                fileSize: imageFile.Length,
                format: Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to upload project image: {FileName}", fileName);
            return CloudinaryUploadResult.Failure($"Upload failed: {ex.Message}");
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogWarning("Cannot delete image: URL is null or empty");
                return false;
            }

            // Extract public ID from URL
            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Cannot extract public ID from URL: {ImageUrl}", imageUrl);
                return false;
            }

            // Delete from Cloudinary
            var success = await _cloudinaryImageService.DeleteImageAsync(publicId);

            if (success)
            {
                _logger.LogInformation("✅ Project image deleted successfully: {PublicId}", publicId);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to delete project image: {PublicId}", publicId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting project image: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public string GetOptimizedImageUrl(string publicId, int width = 800, int height = 600)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            return _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate optimized URL for: {PublicId}", publicId);
            return string.Empty;
        }
    }

    public string GenerateThumbnailUrl(string publicId, int size = 600) 
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            return _cloudinaryImageService.GetOptimizedImageUrl(publicId, size, size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate thumbnail URL for: {PublicId}", publicId);
            return string.Empty;
        }
    }
}