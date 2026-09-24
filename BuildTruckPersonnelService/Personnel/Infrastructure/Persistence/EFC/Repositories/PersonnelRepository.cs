using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;
using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Repositories;

public class PersonnelRepository : BaseRepository<PersonnelEntity, PersonnelServiceDbContext>, IPersonnelRepository
{
    public PersonnelRepository(PersonnelServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PersonnelEntity>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<PersonnelEntity>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<PersonnelEntity>> FindByProjectIdWithAttendanceAsync(int projectId, int year, int month)
    {
        return await Context.Set<PersonnelEntity>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<PersonnelEntity?> FindByDocumentNumberAsync(string documentNumber, int projectId)
    {
        return await Context.Set<PersonnelEntity>()
            .FirstOrDefaultAsync(p =>
                p.DocumentNumber == documentNumber &&
                p.ProjectId == projectId &&
                !p.IsDeleted);
    }

    public async Task<PersonnelEntity?> FindByEmailAsync(string email, int projectId)
    {
        return await Context.Set<PersonnelEntity>()
            .FirstOrDefaultAsync(p =>
                p.Email == email &&
                p.ProjectId == projectId &&
                !p.IsDeleted);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null)
    {
        var query = Context.Set<PersonnelEntity>()
            .Where(p =>
                p.DocumentNumber == documentNumber &&
                p.ProjectId == projectId &&
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
            query = query.Where(p => p.Id != excludePersonnelId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int projectId, int? excludePersonnelId = null)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var query = Context.Set<PersonnelEntity>()
            .Where(p =>
                p.Email == email &&
                p.ProjectId == projectId &&
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
            query = query.Where(p => p.Id != excludePersonnelId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<PersonnelEntity>> FindActiveByProjectIdAsync(int projectId)
    {
        return await Context.Set<PersonnelEntity>()
            .Where(p =>
                p.ProjectId == projectId &&
                !p.IsDeleted &&
                p.Status == PersonnelStatus.ACTIVE)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId)
    {
        return await Context.Set<PersonnelEntity>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .Select(p => p.Department)
            .Distinct()
            .Where(d => !string.IsNullOrEmpty(d))
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<bool> UpdateAttendanceBatchAsync(IEnumerable<PersonnelEntity> personnelList)
    {
        try
        {
            foreach (var personnel in personnelList)
                Context.Set<PersonnelEntity>().Update(personnel);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public new async Task<PersonnelEntity?> FindByIdAsync(int id)
    {
        return await Context.Set<PersonnelEntity>()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public new async Task<IEnumerable<PersonnelEntity>> ListAsync()
    {
        return await Context.Set<PersonnelEntity>()
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }
}
