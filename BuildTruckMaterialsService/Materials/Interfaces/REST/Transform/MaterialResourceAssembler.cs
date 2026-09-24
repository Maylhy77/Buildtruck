using System;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Transform
{
    public static class MaterialResourceAssembler
    {
        // Para crear materiales desde el recurso unificado
        public static CreateMaterialCommand ToCreateCommandFromResource(CreateOrUpdateMaterialResource resource)
        {
            return new CreateMaterialCommand(
                resource.ProjectId,
                resource.Name,
                resource.Type,
                resource.Unit,
                resource.MinimumStock,
                resource.Provider
            );
        }

        // Para actualizar materiales desde el recurso unificado
        public static UpdateMaterialCommand ToUpdateCommandFromResource(int materialId, CreateOrUpdateMaterialResource resource)
        {
            return new UpdateMaterialCommand(
                materialId,
                resource.Name,
                resource.Type,
                resource.Unit,
                resource.MinimumStock,
                resource.Provider
            );
        }

        // Mantener métodos existentes para compatibilidad
        public static CreateMaterialCommand ToCommandFromResource(CreateOrUpdateMaterialResource resource)
        {
            return new CreateMaterialCommand(
                resource.ProjectId,
                resource.Name,
                resource.Type,
                resource.Unit,
                resource.MinimumStock,
                resource.Provider
            );
        }

        public static UpdateMaterialCommand ToCommandFromResource(int materialId, CreateOrUpdateMaterialResource resource)
        {
            return new UpdateMaterialCommand(
                materialId,
                resource.Name,
                resource.Type,
                resource.Unit,
                resource.MinimumStock,
                resource.Provider
            );
        }

        public static MaterialResource ToResourceFromEntity(Material material)
        {
            return new MaterialResource(
                material.Id,
                material.ProjectId,
                material.Name.Value,
                material.Type.Value,
                material.Unit.Value,
                material.MinimumStock.Value,
                material.Provider,
                material.Stock.Value,
                material.Price.Value,
                material.Stock.Value * material.Price.Value
            );
        }
    }
}
