using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckMaterialsService.Materials.Domain.Repositories
{
    public interface IMaterialRepository : IBaseRepository<Material>
    {
        Task<List<Material>> GetByProjectIdAsync(int projectId);
        Task<Material?> GetByIdAsync(int materialId);
    }
}