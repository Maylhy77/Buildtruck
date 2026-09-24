namespace BuildTruckIncidentService.Incidents.Application.ACL.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(string imagePath);
    Task<string> UploadIncidentImageAsync(byte[] imageBytes, string fileName, int incidentId);
    Task<bool> DeleteIncidentImageAsync(string imageUrl); // <-- Agrega este método
}
