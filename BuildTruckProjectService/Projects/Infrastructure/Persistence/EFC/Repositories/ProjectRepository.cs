using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckProjectService.Projects.Infrastructure.Persistence.EFC.Repositories;

public class ProjectRepository : BaseRepository<Project, ProjectServiceDbContext>
{
    public ProjectRepository(ProjectServiceDbContext context) : base(context) { }

    /// <summary>
    /// Find project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project or null if not found</returns>
    public async Task<Project?> FindByIdAsync(int id)
    {
        return await Context.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Find all projects by manager ID
    /// </summary>
    /// <param name="managerId">Manager ID</param>
    /// <returns>List of projects managed by the user</returns>
    public async Task<List<Project>> FindByManagerIdAsync(int managerId)
    {
        return await Context.Set<Project>()
            .Where(p => p.ManagerId == managerId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Find all projects by supervisor ID
    /// </summary>
    /// <param name="supervisorId">Supervisor ID</param>
    /// <returns>List of projects supervised by the user</returns>
    public async Task<List<Project>> FindBySupervisorIdAsync(int supervisorId)
    {
        return await Context.Set<Project>()
            .Where(p => p.SupervisorId == supervisorId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Find projects by state
    /// </summary>
    /// <param name="state">Project state</param>
    /// <returns>List of projects in the specified state</returns>
    public async Task<List<Project>> FindByStateAsync(string state)
    {
        return await Context.Set<Project>()
            .Where(p => p.ProjectState == state)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Find active projects that need supervisors
    /// </summary>
    /// <returns>List of active projects without supervisors</returns>
    public async Task<List<Project>> FindActiveProjectsWithoutSupervisorAsync()
    {
        return await Context.Set<Project>()
            .Where(p => p.ProjectState == "Activo" && p.SupervisorId == null)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Find projects by location (contains search)
    /// </summary>
    /// <param name="location">Location to search for</param>
    /// <returns>List of projects in similar locations</returns>
    public async Task<List<Project>> FindByLocationAsync(string location)
    {
        return await Context.Set<Project>()
            .Where(p => p.ProjectLocation.Contains(location))
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Check if project exists by name and manager
    /// </summary>
    /// <param name="name">Project name</param>
    /// <param name="managerId">Manager ID</param>
    /// <param name="excludeProjectId">Project ID to exclude from check (for updates)</param>
    /// <returns>True if project with same name exists for the manager</returns>
    public async Task<bool> ExistsByNameAndManagerAsync(string name, int managerId, int? excludeProjectId = null)
    {
        var query = Context.Set<Project>()
            .Where(p => p.ProjectName == name && p.ManagerId == managerId);

        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Get projects count by manager
    /// </summary>
    /// <param name="managerId">Manager ID</param>
    /// <returns>Number of projects managed by the user</returns>
    public async Task<int> CountByManagerIdAsync(int managerId)
    {
        return await Context.Set<Project>()
            .CountAsync(p => p.ManagerId == managerId);
    }

    /// <summary>
    /// Get projects count by supervisor
    /// </summary>
    /// <param name="supervisorId">Supervisor ID</param>
    /// <returns>Number of projects supervised by the user</returns>
    public async Task<int> CountBySupervisorIdAsync(int supervisorId)
    {
        return await Context.Set<Project>()
            .CountAsync(p => p.SupervisorId == supervisorId);
    }

    /// <summary>
    /// Find projects with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of projects</returns>
    public async Task<(List<Project> Projects, int TotalCount)> FindPaginatedAsync(int pageNumber, int pageSize)
    {
        var query = Context.Set<Project>()
            .OrderByDescending(p => p.CreatedDate);

        var totalCount = await query.CountAsync();
        var projects = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (projects, totalCount);
    }

    /// <summary>
    /// Search projects by multiple criteria
    /// </summary>
    /// <param name="searchTerm">Term to search in name, description, or location</param>
    /// <param name="managerId">Optional manager ID filter</param>
    /// <param name="supervisorId">Optional supervisor ID filter</param>
    /// <param name="state">Optional state filter</param>
    /// <returns>List of matching projects</returns>
    public async Task<List<Project>> SearchAsync(
        string? searchTerm = null,
        int? managerId = null,
        int? supervisorId = null,
        string? state = null)
    {
        var query = Context.Set<Project>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(p => 
                p.ProjectName.ToLower().Contains(term) ||
                p.ProjectDescription.ToLower().Contains(term) ||
                p.ProjectLocation.ToLower().Contains(term));
        }

        if (managerId.HasValue)
        {
            query = query.Where(p => p.ManagerId == managerId.Value);
        }

        if (supervisorId.HasValue)
        {
            query = query.Where(p => p.SupervisorId == supervisorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(p => p.ProjectState == state);
        }

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }
}