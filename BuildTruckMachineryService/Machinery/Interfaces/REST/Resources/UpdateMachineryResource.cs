using BuildTruckMachineryService.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckMachineryService.Machinery.Interfaces.REST.Resources;

public class UpdateMachineryResource
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string MachineryType { get; set; } = string.Empty;
    public MachineryStatus Status { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? PersonnelId { get; set; }
    public IFormFile? ImageFile { get; set; } // Added for image upload
}