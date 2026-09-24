// Materials/Application/Internal/QueryServices/MaterialQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Application.Internal.QueryServices
{
    /// <summary>
    /// Application service for querying materials
    /// </summary>
    public class MaterialQueryService : IMaterialQueryService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IMaterialCacheService _cache;

        public MaterialQueryService(IMaterialRepository materialRepository, IMaterialCacheService cache)
        {
            _materialRepository = materialRepository;
            _cache = cache;
        }

        public async Task<List<Material>> Handle(GetMaterialsByProjectQuery query)
        {
            var result = await _cache.GetOrSetProjectAsync(
                query.ProjectId,
                "list",
                async () => await _materialRepository.GetByProjectIdAsync(query.ProjectId));

            return result ?? new List<Material>();
        }

        public async Task<Material?> Handle(GetMaterialByIdQuery query)
        {
            return await _cache.GetOrSetAsync(
                $"materials:material:{query.MaterialId}",
                async () => await _materialRepository.GetByIdAsync(query.MaterialId));
        }
    }
}