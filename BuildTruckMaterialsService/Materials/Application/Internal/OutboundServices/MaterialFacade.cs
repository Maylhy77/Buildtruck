// Materials/Application/Internal/OutboundServices/MaterialFacade.cs (Fixed)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Application.Internal.OutboundServices
{
    /// <summary>
    /// Facade implementation for external access to Materials bounded context
    /// </summary>
    public class MaterialFacade : IMaterialFacade
    {
        private readonly IMaterialQueryService _materialQueryService;
        private readonly IMaterialRepository _materialRepository;
        private readonly IMaterialEntryRepository _entryRepository;
        private readonly IMaterialUsageRepository _usageRepository;

        public MaterialFacade(
            IMaterialQueryService materialQueryService,
            IMaterialRepository materialRepository,
            IMaterialEntryRepository entryRepository,
            IMaterialUsageRepository usageRepository)
        {
            _materialQueryService = materialQueryService;
            _materialRepository = materialRepository;
            _entryRepository = entryRepository;
            _usageRepository = usageRepository;
        }

        public async Task<List<Material>> GetMaterialsByProjectAsync(int projectId)
        {
            var query = new GetMaterialsByProjectQuery(projectId);
            return await _materialQueryService.Handle(query);
        }

        public async Task<decimal> GetMaterialStockAsync(int materialId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null) return 0;

            var entries = await _entryRepository.GetByMaterialIdAsync(materialId);
            var usages = await _usageRepository.GetByMaterialIdAsync(materialId);

            var totalEntries = entries.Sum(e => (decimal)e.Quantity);
            var totalUsages = usages.Sum(u => (decimal)u.Quantity);

            return totalEntries - totalUsages;
        }

        public async Task<bool> ValidateMaterialExistsInProjectAsync(int materialId, int projectId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            return material != null && material.ProjectId == projectId;
        }

        public async Task<decimal> GetProjectMaterialCostAsync(int projectId)
        {
            var entries = await _entryRepository.GetByProjectIdAsync(projectId);
            return entries.Sum(e => e.TotalCost.Value);
        }
    }
}

