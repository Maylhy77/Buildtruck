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
    /// EF Core implementation of Material repository
    /// </summary>
    public class MaterialRepository : BaseRepository<Material, MaterialsServiceDbContext>, IMaterialRepository
    {
        public MaterialRepository(MaterialsServiceDbContext context) : base(context) { }

        public async Task<List<Material>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<Material>()
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.Name.Value)
                .ToListAsync();
        }

        public async Task<Material?> GetByIdAsync(int materialId)
        {
            return await Context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Id == materialId);
        }
    }
}
