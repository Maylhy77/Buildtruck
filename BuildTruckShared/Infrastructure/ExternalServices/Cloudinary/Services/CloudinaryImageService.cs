using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;

public class CloudinaryImageService : ICloudinaryImageService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!_settings.IsValid)
            throw new InvalidOperationException("Cloudinary settings are not properly configured");

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string folder)
    {
        var validation = ValidateImage(imageBytes, fileName);
        if (!validation.IsValid)
            throw new ArgumentException(validation.ErrorMessage);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var publicId = $"{folder}{fileNameWithoutExt}_{timestamp}";

        using var stream = new MemoryStream(imageBytes);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            PublicId = publicId,
            Folder = folder,
            Transformation = new Transformation().Quality("auto:best").FetchFormat("auto").Width(1200).Height(1200).Crop("limit"),
            Overwrite = true,
            UseFilename = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", publicId);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return false;

        var deleteResult = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        var success = deleteResult.Result == "ok";
        if (!success)
            _logger.LogWarning("Cloudinary delete result: {Result} for {PublicId}", deleteResult.Result, publicId);
        return success;
    }

    public string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200)
    {
        if (string.IsNullOrEmpty(publicId)) return string.Empty;

        var transformation = new Transformation().Width(width).Height(height).Crop("fill").Quality("auto").FetchFormat("auto");
        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    }

    public (bool IsValid, string ErrorMessage) ValidateImage(byte[] imageBytes, string fileName)
    {
        if (imageBytes.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            return (false, $"File size exceeds maximum allowed size of {maxSizeMB:F1}MB");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            return (false, $"File type {extension} not allowed. Allowed: {string.Join(", ", _settings.AllowedExtensions)}");

        if (!IsValidImageFormat(imageBytes))
            return (false, "File does not appear to be a valid image");

        return (true, string.Empty);
    }

    public string ExtractPublicIdFromUrl(string cloudinaryUrl)
    {
        if (string.IsNullOrEmpty(cloudinaryUrl)) return string.Empty;

        var uri = new Uri(cloudinaryUrl);
        var pathSegments = uri.AbsolutePath.Split('/');
        var uploadIndex = Array.IndexOf(pathSegments, "upload");
        if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length) return string.Empty;

        var startIndex = uploadIndex + 1;
        if (pathSegments[startIndex].StartsWith('v') && pathSegments[startIndex].Skip(1).All(char.IsDigit))
            startIndex++;

        var publicIdSegments = pathSegments[startIndex..];
        publicIdSegments[^1] = Path.GetFileNameWithoutExtension(publicIdSegments[^1]);
        return string.Join("/", publicIdSegments);
    }

    private static bool IsValidImageFormat(byte[] imageBytes)
    {
        if (imageBytes.Length < 4) return false;
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF) return true;
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47) return true;
        return false;
    }
}
