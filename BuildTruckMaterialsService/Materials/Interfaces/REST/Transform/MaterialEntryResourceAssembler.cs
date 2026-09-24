using System;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Transform
{
    public static class MaterialEntryResourceAssembler
    {
        // Para crear entradas desde el recurso unificado
        public static CreateMaterialEntryCommand ToCreateCommandFromResource(CreateOrUpdateMaterialEntryResource resource)
        {
            return new CreateMaterialEntryCommand(
                resource.ProjectId,
                resource.MaterialId,
                resource.Date,
                resource.Quantity,
                "UND", // Unit por defecto
                resource.Provider,
                resource.Ruc,
                resource.Payment,
                resource.DocumentType,
                resource.DocumentNumber,
                resource.UnitCost,
                resource.Quantity * resource.UnitCost, // TotalCost calculado
                resource.Status ?? "PENDING", // ✅ ARREGLO: Usar el status del resource
                resource.Observations ?? string.Empty
            );
        }

        // Para actualizar entradas desde el recurso unificado
        public static UpdateMaterialEntryCommand ToUpdateCommandFromResource(int entryId, CreateOrUpdateMaterialEntryResource resource)
        {
            return new UpdateMaterialEntryCommand(
                entryId,
                resource.Date,
                resource.Quantity,
                "UND", // Unit por defecto
                resource.Provider,
                resource.Ruc,
                resource.Payment,
                resource.DocumentType,
                resource.DocumentNumber,
                resource.UnitCost,
                resource.Quantity * resource.UnitCost, // TotalCost calculado
                resource.Status ?? "PENDING", // ✅ ARREGLO: Usar el status del resource
                resource.Observations ?? string.Empty
            );
        }

        // Mantener métodos existentes para compatibilidad
        public static CreateMaterialEntryCommand ToCommandFromResource(CreateOrUpdateMaterialEntryResource resource)
        {
            return new CreateMaterialEntryCommand(
                resource.ProjectId,
                resource.MaterialId,
                resource.Date,
                resource.Quantity,
                "UND",
                resource.Provider,
                resource.Ruc,
                resource.Payment,
                resource.DocumentType,
                resource.DocumentNumber,
                resource.UnitCost,
                resource.Quantity * resource.UnitCost,
                resource.Status ?? "PENDING", // ✅ ARREGLO: Usar el status del resource
                resource.Observations ?? string.Empty
            );
        }

        public static UpdateMaterialEntryCommand ToCommandFromResource(int entryId, CreateOrUpdateMaterialEntryResource resource)
        {
            return new UpdateMaterialEntryCommand(
                entryId,
                resource.Date,
                resource.Quantity,
                "UND",
                resource.Provider,
                resource.Ruc,
                resource.Payment,
                resource.DocumentType,
                resource.DocumentNumber,
                resource.UnitCost,
                resource.Quantity * resource.UnitCost,
                resource.Status ?? "PENDING", // ✅ ARREGLO: Usar el status del resource
                resource.Observations ?? string.Empty
            );
        }

        public static MaterialEntryResource ToResourceFromEntity(MaterialEntry entry)
        {
            return new MaterialEntryResource(
                entry.Id,
                entry.ProjectId,
                entry.MaterialId,
                entry.Date,
                entry.Quantity.Value,
                entry.Unit.Value,
                entry.Provider,
                entry.Ruc,
                entry.Payment.Value,
                entry.DocumentType.Value,
                entry.DocumentNumber,
                entry.UnitCost.Value,
                entry.TotalCost.Value,
                entry.Status.Value,
                entry.Observations
            );
        }
    }
}