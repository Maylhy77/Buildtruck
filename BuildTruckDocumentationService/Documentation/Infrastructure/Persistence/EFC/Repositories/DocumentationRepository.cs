using BuildTruckDocumentationService.Documentation.Domain.Repositories;
using BuildTruckDocumentationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckDocumentationService.Documentation.Infrastructure.Persistence.EFC.Repositories;

public class DocumentationRepository :
    BaseRepository<Domain.Model.Aggregates.Documentation, DocumentationServiceDbContext>,
    IDocumentationRepository
{
    public DocumentationRepository(DocumentationServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<Domain.Model.Aggregates.Documentation?> FindByIdAndProjectAsync(int id, int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .FirstOrDefaultAsync(d => 
                d.Id == id && 
                d.ProjectId == projectId && 
                !d.IsDeleted);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> FindByProjectIdOrderedByDateAsync(int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> FindRecentByProjectIdAsync(int projectId, int days = 7)
    {
        var cutoffDate = DateTime.Now.Date.AddDays(-days);
        
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .Where(d => 
                d.ProjectId == projectId && 
                !d.IsDeleted &&
                d.Date >= cutoffDate)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsByTitleAndProjectAsync(string title, int projectId, int? excludeId = null)
    {
        var query = Context.Set<Domain.Model.Aggregates.Documentation>()
            .Where(d => 
                d.Title == title && 
                d.ProjectId == projectId && 
                !d.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    // Override base methods to include IsDeleted filter
    public new async Task<Domain.Model.Aggregates.Documentation?> FindByIdAsync(int id)
    {
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public new async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> ListAsync()
    {
        return await Context.Set<Domain.Model.Aggregates.Documentation>()
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }
}
