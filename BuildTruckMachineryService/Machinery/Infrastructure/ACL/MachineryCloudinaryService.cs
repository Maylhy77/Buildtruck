using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using Microsoft.Extensions.Options;

namespace BuildTruckMachineryService.Machinery.Infrastructure.ACL;

public class MachineryCloudinaryService : IMachineryCloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<MachineryCloudinaryService> _logger;

    public MachineryCloudinaryService(
        ICloudinaryImageService cloudinaryImageService,
        IOptions<CloudinarySettings> settings,
        ILogger<MachineryCloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
    {
        _logger.LogInformation("Uploading image for Machinery with fileName {FileName}", fileName);
        

        var url = await _cloudinaryImageService.UploadImageAsync(imageBytes, fileName, _settings.MachineryImagesFolder.TrimEnd('/').TrimStart('/'));
        _logger.LogInformation("Successfully uploaded image: {Url}", url);
        return url;
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        _logger.LogInformation("Uploading image for Machinery with fileName {FileName}", imageFile.FileName);

        // Validate file
        if (imageFile.Length > _settings.MaxFileSizeBytes)
            throw new ArgumentException($"Image file size exceeds {_settings.MaxFileSizeBytes / 1024 / 1024} MB");

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            throw new ArgumentException($"Invalid file extension. Allowed: {string.Join(", ", _settings.AllowedExtensions)}");

        using var stream = new MemoryStream();
        await imageFile.CopyToAsync(stream);
        var imageBytes = stream.ToArray();

        var url = await _cloudinaryImageService.UploadImageAsync(imageBytes, imageFile.FileName, _settings.MachineryImagesFolder.TrimEnd('/').TrimStart('/'));
        _logger.LogInformation("Successfully uploaded image: {Url}", url);
        return url;
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        _logger.LogInformation("Deleting image for Machinery with publicId {PublicId}", publicId);
        var success = await _cloudinaryImageService.DeleteImageAsync(publicId);
        if (success)
            _logger.LogInformation("Successfully deleted image: {PublicId}", publicId);
        else
            _logger.LogWarning("Failed to delete image: {PublicId}", publicId);
        return success;
    }

    public string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200)
    {
        _logger.LogInformation("Generating optimized URL for Machinery image with publicId {PublicId}", publicId);
        return _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
    }

    public string ExtractPublicIdFromUrl(string cloudinaryUrl)
    {
        _logger.LogInformation("Extracting public ID from URL: '{Url}'", cloudinaryUrl);
    
    try
    {
        if (string.IsNullOrEmpty(cloudinaryUrl))
        {
            _logger.LogWarning("Empty or null URL provided");
            return string.Empty;
        }

        // Example URL: https://res.cloudinary.com/dyaufzff7/image/upload/v1751516053/buildtruck/machinery/buildtruck/machinerycamion 11_1751516043.jpg
        // We need to extract: buildtruck/machineryupload_1751518183 (the actual public ID)
        
        var uri = new Uri(cloudinaryUrl);
        var pathSegments = uri.AbsolutePath.Split('/');
        
        // Find the version segment (starts with 'v')
        var versionIndex = -1;
        for (int i = 0; i < pathSegments.Length; i++)
        {
            if (pathSegments[i].StartsWith("v") && pathSegments[i].Length > 1 && char.IsDigit(pathSegments[i][1]))
            {
                versionIndex = i;
                break;
            }
        }
        
        if (versionIndex == -1 || versionIndex + 1 >= pathSegments.Length)
        {
            _logger.LogWarning("Could not find version segment in URL: '{Url}'", cloudinaryUrl);
            return string.Empty;
        }
        
        // Get everything after the version segment
        var pathAfterVersion = string.Join("/", pathSegments.Skip(versionIndex + 1));
        
        // Remove file extension
        var publicId = Path.GetFileNameWithoutExtension(pathAfterVersion);
        
        // Handle the folder structure - looking at your logs, it seems like the actual structure is simpler
        // Let's try to extract just the filename without the duplicated folder structure
        var actualPublicId = pathAfterVersion.Replace(".jpg", "").Replace(".png", "").Replace(".jpeg", "");
        
        _logger.LogInformation("✅ Extracted public ID: '{PublicId}' from URL: '{Url}'", actualPublicId, cloudinaryUrl);
        return actualPublicId;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Failed to extract public ID from URL: '{Url}'", cloudinaryUrl);
        return string.Empty;
    }
    }
}