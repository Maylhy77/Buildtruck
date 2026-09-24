
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Application.Internal.QueryServices
{
    /// <summary>
    /// Application service for querying inventory summary
    /// </summary>
    public class InventoryQueryService : IInventoryQueryService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IMaterialEntryRepository _entryRepository;
        private readonly IMaterialUsageRepository _usageRepository;

        public InventoryQueryService(
            IMaterialRepository materialRepository,
            IMaterialEntryRepository entryRepository,
            IMaterialUsageRepository usageRepository)
        {
            _materialRepository = materialRepository;
            _entryRepository = entryRepository;
            _usageRepository = usageRepository;
        }

        public async Task<List<Material>> Handle(GetInventoryByProjectQuery query)
        {
            var materials = await _materialRepository.GetByProjectIdAsync(query.ProjectId);
            var entries = await _entryRepository.GetByProjectIdAsync(query.ProjectId);
            var usages = await _usageRepository.GetByProjectIdAsync(query.ProjectId);

            // Actualizar stock y precio de cada material
            foreach (var material in materials)
            {
                var materialEntries = entries.Where(e => e.MaterialId == material.Id).ToList();
                var materialUsages = usages.Where(u => u.MaterialId == material.Id).ToList();

                var totalEntries = materialEntries.Sum(e => (decimal)e.Quantity);
                var totalUsages = materialUsages.Sum(u => (decimal)u.Quantity);
                var currentStock = totalEntries - totalUsages;

                // Calcular precio promedio ponderado
                var totalEntryCost = materialEntries.Sum(e => e.UnitCost.Value * (decimal)e.Quantity);
                var averagePrice = totalEntries > 0 ? totalEntryCost / totalEntries : 0;

                // Actualizar el material con stock y precio calculados
                material.UpdateStock(new Domain.Model.ValueObjects.MaterialQuantity(currentStock));
                material.UpdatePrice(new Domain.Model.ValueObjects.MaterialCost(averagePrice, "PEN"));
            }

            return materials;
        }

        // ✅ MÉTODO NUEVO AGREGADO CORRECTAMENTE DENTRO DE LA CLASE
        public async Task<List<InventoryItemResource>> HandleDetailed(GetInventoryByProjectQuery query)
        {
            var materials = await _materialRepository.GetByProjectIdAsync(query.ProjectId);
            var entries = await _entryRepository.GetByProjectIdAsync(query.ProjectId);
            var usages = await _usageRepository.GetByProjectIdAsync(query.ProjectId);

            var result = new List<InventoryItemResource>();

            foreach (var material in materials)
            {
                var materialEntries = entries.Where(e => e.MaterialId == material.Id).ToList();
                var materialUsages = usages.Where(u => u.MaterialId == material.Id).ToList();

                var totalEntries = materialEntries.Sum(e => (decimal)e.Quantity);
                var totalUsages = materialUsages.Sum(u => (decimal)u.Quantity);
                var currentStock = totalEntries - totalUsages;

                // Calcular precio promedio ponderado
                var totalEntryCost = materialEntries.Sum(e => e.UnitCost.Value * (decimal)e.Quantity);
                var averagePrice = totalEntries > 0 ? totalEntryCost / totalEntries : 0;

                result.Add(new InventoryItemResource(
                    material.Id,
                    material.Name.Value,
                    material.Type.Value,
                    material.Unit.Value,
                    (decimal)material.MinimumStock.Value,
                    material.Provider,
                    totalEntries,        // ✅ Total de entradas
                    totalUsages,         // ✅ Total de usos
                    currentStock,        // Stock actual calculado
                    averagePrice,        // Precio promedio
                    currentStock * averagePrice  // Total
                ));
            }

            return result;
        }
    }
}