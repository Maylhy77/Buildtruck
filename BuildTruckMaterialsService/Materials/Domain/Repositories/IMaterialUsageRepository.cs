using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckMaterialsService.Materials.Domain.Repositories
{
    public interface IMaterialUsageRepository : IBaseRepository<MaterialUsage>
    {
        Task<List<MaterialUsage>> GetByProjectIdAsync(int projectId);
        Task<MaterialUsage?> GetByIdAsync(int usageId);
        Task<List<MaterialUsage>> GetByMaterialIdAsync(int materialId);
    }
}