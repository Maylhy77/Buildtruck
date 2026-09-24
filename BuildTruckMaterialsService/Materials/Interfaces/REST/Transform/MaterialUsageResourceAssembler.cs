using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Transform
{
    public static class MaterialUsageResourceAssembler
    {
        // Para crear usos desde el recurso unificado
        public static CreateMaterialUsageCommand ToCreateCommandFromResource(CreateOrUpdateMaterialUsageResource resource)
        {
            return new CreateMaterialUsageCommand(
                resource.ProjectId,
                resource.MaterialId,
                resource.Date,
                resource.Quantity,
                resource.Area,
                resource.UsageType,
                resource.Worker,
                resource.Observations ?? string.Empty
            );
        }

        // Para actualizar usos desde el recurso unificado
        public static UpdateMaterialUsageCommand ToUpdateCommandFromResource(int usageId, CreateOrUpdateMaterialUsageResource resource)
        {
            return new UpdateMaterialUsageCommand(
                usageId,
                resource.Date,
                resource.Quantity,
                resource.Area,
                resource.UsageType,
                resource.Worker,
                resource.Observations ?? string.Empty
            );
        }

        
        public static CreateMaterialUsageCommand ToCommandFromResource(CreateOrUpdateMaterialUsageResource resource)
        {
            return new CreateMaterialUsageCommand(
                resource.ProjectId,    
                resource.MaterialId,   
                resource.Date,
                resource.Quantity,
                resource.Area,
                resource.UsageType,
                resource.Worker,
                resource.Observations ?? string.Empty
            );
        }

        public static UpdateMaterialUsageCommand ToCommandFromResource(int usageId, CreateOrUpdateMaterialUsageResource resource)
        {
            return new UpdateMaterialUsageCommand(
                usageId,
                resource.Date,
                resource.Quantity,
                resource.Area,
                resource.UsageType,
                resource.Worker,
                resource.Observations ?? string.Empty
            );
        }

        public static MaterialUsageResource ToResourceFromEntity(MaterialUsage usage)
        {
            return new MaterialUsageResource(
                usage.Id,
                usage.ProjectId,
                usage.MaterialId,
                usage.Date,
                usage.Quantity.Value,
                usage.Area,
                usage.UsageType.Value,
                usage.Worker,
                usage.Observations
            );
        }
    }
}