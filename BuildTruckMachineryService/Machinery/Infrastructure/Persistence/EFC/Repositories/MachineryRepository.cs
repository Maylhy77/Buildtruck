using BuildTruckMachineryService.Machinery.Domain.Repositories;
using BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using MachineryEntity = BuildTruckMachineryService.Machinery.Domain.Model.Aggregates.Machinery;

namespace BuildTruckMachineryService.Machinery.Infrastructure.Persistence.EFC.Repositories;

public class MachineryRepository : BaseRepository<MachineryEntity, MachineryServiceDbContext>, IMachineryRepository
{
    private readonly MachineryServiceDbContext _context;

    public MachineryRepository(MachineryServiceDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MachineryEntity>> FindByProjectIdAsync(int projectId)
    {
        return await _context.Set<MachineryEntity>()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<MachineryEntity?> FindByLicensePlateAsync(string licensePlate, int projectId)
    {
        return await _context.Set<MachineryEntity>()
            .FirstOrDefaultAsync(m =>
                m.ProjectId == projectId &&
                m.LicensePlate == licensePlate);
    }

    public async Task UpdateAsync(MachineryEntity machinery)
    {
        _context.Machinery.Update(machinery);
        await _context.SaveChangesAsync();
    }
}