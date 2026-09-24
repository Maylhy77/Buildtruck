using BuildTruckMachineryService.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckMachineryService.Machinery.Domain.Model.Commands;

public record CreateMachineryCommand(
    int ProjectId,
    string Name,
    string LicensePlate,
    string MachineryType,
    MachineryStatus Status,
    string Provider,
    string Description,
    int? PersonnelId,
    DateTime RegisterDate,
    byte[]? ImageBytes, // Added for image
    string? ImageFileName) // Added for image file name
{
    
}