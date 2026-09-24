using BuildTruckPersonnelService.Personnel.Application.ACL.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.ACL;

public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<CloudinaryService> _logger;

    private const string PERSONNEL_FOLDER = "buildtruck/personnel/";

    public CloudinaryService(ICloudinaryImageService cloudinaryImageService, ILogger<CloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    public async Task<string> UploadPersonnelPhotoAsync(byte[] imageBytes, string fileName, int personnelId)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"personnel_{personnelId}_{timestamp}{extension}";

            var imageUrl = await _cloudinaryImageService.UploadImageAsync(imageBytes, uniqueFileName, PERSONNEL_FOLDER);

            _logger.LogInformation("Personnel photo uploaded: {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload personnel photo for personnel {PersonnelId}", personnelId);
            throw new InvalidOperationException($"Failed to upload personnel photo: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeletePersonnelPhotoAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                return false;

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
                return false;

            return await _cloudinaryImageService.DeleteImageAsync(publicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personnel photo: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public string GetOptimizedPhotoUrl(string imageUrl, int width = 200, int height = 200)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                return imageUrl;

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
                return imageUrl;

            return _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
        }
        catch
        {
            return imageUrl;
        }
    }
}
