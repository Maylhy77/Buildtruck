using BuildTruckMachineryService.Machinery.Domain.Model.Commands;
using BuildTruckMachineryService.Machinery.Domain.Model.ValueObjects;
using BuildTruckMachineryService.Machinery.Interfaces.REST.Resources;

namespace BuildTruckMachineryService.Machinery.Interfaces.REST.Transform;

public static class MachineryResourceAssembler
{
    public static MachineryResource ToResource(this Domain.Model.Aggregates.Machinery machinery)
    {
        return new MachineryResource
        {
            Id = machinery.Id,
            ProjectId = machinery.ProjectId,
            Name = machinery.Name,
            LicensePlate = machinery.LicensePlate,
            MachineryType = machinery.MachineryType,
            Status = Enum.Parse<MachineryStatus>(machinery.Status, ignoreCase: true),
            Provider = machinery.Provider,
            Description = machinery.Description,
            PersonnelId = machinery.PersonnelId,
            ImageUrl = machinery.ImageUrl,
            CreatedAt = machinery.CreatedAt,
            UpdatedAt = machinery.UpdatedAt,
            RegisterDate = machinery.RegisterDate
        };
    }

    public static CreateMachineryCommand ToCommandFromResource(this CreateMachineryResource resource)
    {
        byte[]? imageBytes = null;
        string? imageFileName = null;

        if (resource.ImageFile != null)
        {
            using var memoryStream = new MemoryStream();
            resource.ImageFile.CopyTo(memoryStream);
            imageBytes = memoryStream.ToArray();
            imageFileName = resource.ImageFile.FileName;
        }

        return new CreateMachineryCommand(
            resource.ProjectId,
            resource.Name,
            resource.LicensePlate,
            resource.MachineryType,
            resource.Status,
            resource.Provider,
            resource.Description,
            resource.PersonnelId,
            resource.RegisterDate,
            imageBytes,
            imageFileName
        );
    }
    
    
    public static UpdateMachineryCommand ToCommandFromResource(this UpdateMachineryResource resource, int id)
    {
        return new UpdateMachineryCommand(
            id,
            resource.ProjectId,
            resource.Name,
            resource.LicensePlate,
            resource.MachineryType,
            resource.Status,
            resource.Provider,
            resource.Description,
            resource.PersonnelId
        );
    }
}