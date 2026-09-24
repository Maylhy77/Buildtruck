namespace BuildTruckPersonnelService.Personnel.Application.ACL.Services;

public interface ICloudinaryService
{
    Task<string> UploadPersonnelPhotoAsync(byte[] imageBytes, string fileName, int personnelId);
    Task<bool> DeletePersonnelPhotoAsync(string imageUrl);
    string GetOptimizedPhotoUrl(string imageUrl, int width = 200, int height = 200);
}
