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
    /// EF Core implementation of MaterialEntry repository
    /// </summary>
    public class MaterialEntryRepository : BaseRepository<MaterialEntry, MaterialsServiceDbContext>, IMaterialEntryRepository
    {
        public MaterialEntryRepository(MaterialsServiceDbContext context) : base(context) { }

        public async Task<List<MaterialEntry>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<MaterialEntry>()
                .Where(e => e.ProjectId == projectId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<MaterialEntry?> GetByIdAsync(int entryId)
        {
            return await Context.Set<MaterialEntry>()
                .FirstOrDefaultAsync(e => e.Id == entryId);
        }

        public async Task<List<MaterialEntry>> GetByMaterialIdAsync(int materialId)
        {
            return await Context.Set<MaterialEntry>()
                .Where(e => e.MaterialId == materialId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }
    }
}
