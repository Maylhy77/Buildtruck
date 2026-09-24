using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Repositories
{
    /// <summary>
    /// EF Core implementation of MaterialUsage repository
    /// </summary>
    public class MaterialUsageRepository : BaseRepository<MaterialUsage, MaterialsServiceDbContext>, IMaterialUsageRepository
    {
        public MaterialUsageRepository(MaterialsServiceDbContext context) : base(context) { }

        public async Task<List<MaterialUsage>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<MaterialUsage>()
                .Where(u => u.ProjectId == projectId)
                .OrderByDescending(u => u.Date)
                .ThenByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<MaterialUsage?> GetByIdAsync(int usageId)
        {
            return await Context.Set<MaterialUsage>()
                .FirstOrDefaultAsync(u => u.Id == usageId);
        }

        public async Task<List<MaterialUsage>> GetByMaterialIdAsync(int materialId)
        {
            return await Context.Set<MaterialUsage>()
                .Where(u => u.MaterialId == materialId)
                .OrderByDescending(u => u.Date)
                .ThenByDescending(u => u.CreatedDate)
                .ToListAsync();
        }
    }
}
