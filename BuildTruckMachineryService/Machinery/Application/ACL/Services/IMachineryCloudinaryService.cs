namespace BuildTruckMachineryService.Machinery.Application.ACL.Services;
using Microsoft.AspNetCore.Http;


public interface IMachineryCloudinaryService
{
    Task<string> UploadImageAsync(byte[] imageBytes, string fileName);
    Task<string> UploadImageAsync(IFormFile imageFile);
    Task<bool> DeleteImageAsync(string publicId);
    string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200);
    string ExtractPublicIdFromUrl(string cloudinaryUrl);
}