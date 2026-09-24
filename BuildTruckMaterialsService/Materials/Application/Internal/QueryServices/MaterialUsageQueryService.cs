
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Application.Internal.QueryServices
{
    /// <summary>
    /// Application service for querying material usages
    /// </summary>
    public class MaterialUsageQueryService : IMaterialUsageQueryService
    {
        private readonly IMaterialUsageRepository _usageRepository;

        public MaterialUsageQueryService(IMaterialUsageRepository usageRepository)
        {
            _usageRepository = usageRepository;
        }

        public async Task<List<MaterialUsage>> Handle(GetMaterialUsagesByProjectQuery query)
        {
            return await _usageRepository.GetByProjectIdAsync(query.ProjectId);
        }

        public async Task<MaterialUsage?> Handle(GetMaterialUsageByIdQuery query)
        {
            return await _usageRepository.GetByIdAsync(query.UsageId);
        }
    }
}