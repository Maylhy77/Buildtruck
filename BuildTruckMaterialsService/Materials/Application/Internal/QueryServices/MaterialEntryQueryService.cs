// Materials/Application/Internal/QueryServices/MaterialEntryQueryService.cs
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
    /// Application service for querying material entries
    /// </summary>
    public class MaterialEntryQueryService : IMaterialEntryQueryService
    {
        private readonly IMaterialEntryRepository _entryRepository;

        public MaterialEntryQueryService(IMaterialEntryRepository entryRepository)
        {
            _entryRepository = entryRepository;
        }

        public async Task<List<MaterialEntry>> Handle(GetMaterialEntriesByProjectQuery query)
        {
            return await _entryRepository.GetByProjectIdAsync(query.ProjectId);
        }

        public async Task<MaterialEntry?> Handle(GetMaterialEntryByIdQuery query)
        {
            return await _entryRepository.GetByIdAsync(query.EntryId);
        }
    }
}