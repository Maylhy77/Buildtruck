using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckMaterialsService.Materials.Domain.Repositories
{
    public interface IMaterialEntryRepository : IBaseRepository<MaterialEntry>
    {
        Task<List<MaterialEntry>> GetByProjectIdAsync(int projectId);
        Task<MaterialEntry?> GetByIdAsync(int entryId);
        Task<List<MaterialEntry>> GetByMaterialIdAsync(int materialId);
    }
}