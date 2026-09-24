using BuildTruckProjectService.Projects.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Extensions.Logging;

namespace BuildTruckProjectService.Projects.Application.Internal.OutboundServices;

/// <summary>
/// Project Facade Implementation
/// Provides project information and access control for other bounded contexts
/// </summary>
public class ProjectFacade : IProjectFacade
{
    private readonly ProjectRepository _projectRepository;
    private readonly ILogger<ProjectFacade> _logger;

    public ProjectFacade(ProjectRepository projectRepository, ILogger<ProjectFacade> logger)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ExistsByIdAsync(int projectId)
    {
        try
        {
            _logger.LogDebug("Checking if project exists: {ProjectId}", projectId);
            
            var project = await _projectRepository.FindByIdAsync(projectId);
            var exists = project != null;
            
            _logger.LogDebug("Project {ProjectId} exists: {Exists}", projectId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project exists: {ProjectId}", projectId);
            return false;
        }
    }

    public async Task<ProjectInfo?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            _logger.LogDebug("Getting project by ID: {ProjectId}", projectId);
            
            var project = await _projectRepository.FindByIdAsync(projectId);
            
            if (project == null)
            {
                _logger.LogDebug("Project not found: {ProjectId}", projectId);
                return null;
            }

            var projectInfo = new ProjectInfo
            {
                Id = project.Id,
                Name = project.ProjectName,
                Description = project.ProjectDescription,
                Location = project.ProjectLocation,
                State = project.ProjectState,
                ManagerId = project.ManagerId,
                SupervisorId = project.SupervisorId,
                StartDate = project.StartDate,
                ImageUrl = project.ImageUrl,
                CreatedAt = project.CreatedDate,
                IsActive = project.State.IsActive,
                HasSupervisor = project.HasSupervisor,
                IsReadyToStart = project.IsReadyToStart()
            };

            _logger.LogDebug("Retrieved project: {ProjectName} (ID: {ProjectId})", 
                project.ProjectName, projectId);
            
            return projectInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project by ID: {ProjectId}", projectId);
            return null;
        }
    }

    public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
    {
        try
        {
            _logger.LogDebug("Checking user access to project: User {UserId}, Project {ProjectId}", 
                userId, projectId);
            
            var project = await _projectRepository.FindByIdAsync(projectId);
            
            if (project == null)
            {
                _logger.LogDebug("Project not found for access check: {ProjectId}", projectId);
                return false;
            }

            var hasAccess = project.ManagerId == userId || project.SupervisorId == userId;
            
            _logger.LogDebug("User {UserId} has access to project {ProjectId}: {HasAccess}", 
                userId, projectId, hasAccess);
            
            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to project: User {UserId}, Project {ProjectId}", 
                userId, projectId);
            return false;
        }
    }

    public async Task<List<ProjectInfo>> GetProjectsByUserAsync(int userId)
    {
        try
        {
            _logger.LogDebug("Getting projects for user: {UserId}", userId);
            
            var managedProjects = await _projectRepository.FindByManagerIdAsync(userId);
            var supervisedProjects = await _projectRepository.FindBySupervisorIdAsync(userId);
            
            // Combine and remove duplicates (in case user is both manager and supervisor of same project)
            var allProjects = managedProjects
                .Concat(supervisedProjects)
                .DistinctBy(p => p.Id)
                .Select(p => new ProjectInfo
                {
                    Id = p.Id,
                    Name = p.ProjectName,
                    Description = p.ProjectDescription,
                    Location = p.ProjectLocation,
                    State = p.ProjectState,
                    ManagerId = p.ManagerId,
                    SupervisorId = p.SupervisorId,
                    StartDate = p.StartDate,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedDate,
                    IsActive = p.State.IsActive,
                    HasSupervisor = p.HasSupervisor,
                    IsReadyToStart = p.IsReadyToStart()
                })
                .ToList();

            _logger.LogDebug("Found {Count} projects for user {UserId}", allProjects.Count, userId);
            return allProjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for user: {UserId}", userId);
            return new List<ProjectInfo>();
        }
    }

    public async Task<List<ProjectInfo>> GetProjectsByManagerAsync(int managerId)
    {
        try
        {
            _logger.LogDebug("Getting projects managed by: {ManagerId}", managerId);
            
            var projects = await _projectRepository.FindByManagerIdAsync(managerId);
            
            var projectInfos = projects.Select(p => new ProjectInfo
            {
                Id = p.Id,
                Name = p.ProjectName,
                Description = p.ProjectDescription,
                Location = p.ProjectLocation,
                State = p.ProjectState,
                ManagerId = p.ManagerId,
                SupervisorId = p.SupervisorId,
                StartDate = p.StartDate,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedDate,
                IsActive = p.State.IsActive,
                HasSupervisor = p.HasSupervisor,
                IsReadyToStart = p.IsReadyToStart()
            }).ToList();

            _logger.LogDebug("Found {Count} projects managed by {ManagerId}", projectInfos.Count, managerId);
            return projectInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by manager: {ManagerId}", managerId);
            return new List<ProjectInfo>();
        }
    }

    public async Task<List<ProjectInfo>> GetProjectsBySupervisorAsync(int supervisorId)
    {
        try
        {
            _logger.LogDebug("Getting projects supervised by: {SupervisorId}", supervisorId);
            
            var projects = await _projectRepository.FindBySupervisorIdAsync(supervisorId);
            
            var projectInfos = projects.Select(p => new ProjectInfo
            {
                Id = p.Id,
                Name = p.ProjectName,
                Description = p.ProjectDescription,
                Location = p.ProjectLocation,
                State = p.ProjectState,
                ManagerId = p.ManagerId,
                SupervisorId = p.SupervisorId,
                StartDate = p.StartDate,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedDate,
                IsActive = p.State.IsActive,
                HasSupervisor = p.HasSupervisor,
                IsReadyToStart = p.IsReadyToStart()
            }).ToList();

            _logger.LogDebug("Found {Count} projects supervised by {SupervisorId}", projectInfos.Count, supervisorId);
            return projectInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by supervisor: {SupervisorId}", supervisorId);
            return new List<ProjectInfo>();
        }
    }

    public async Task<int> GetProjectCountByStateAsync(string state)
    {
        try
        {
            _logger.LogDebug("Getting project count by state: {State}", state);
            
            var projects = await _projectRepository.FindByStateAsync(state);
            var count = projects.Count;
            
            _logger.LogDebug("Found {Count} projects in state {State}", count, state);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project count by state: {State}", state);
            return 0;
        }
    }

    public async Task<int> GetActiveProjectsCountAsync()
    {
        try
        {
            _logger.LogDebug("Getting active projects count");
            
            return await GetProjectCountByStateAsync("Activo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active projects count");
            return 0;
        }
    }

    public async Task<List<ProjectInfo>> GetProjectsByStateAsync(string state)
    {
        try
        {
            _logger.LogDebug("Getting projects by state: {State}", state);
            
            var projects = await _projectRepository.FindByStateAsync(state);
            
            var projectInfos = projects.Select(p => new ProjectInfo
            {
                Id = p.Id,
                Name = p.ProjectName,
                Description = p.ProjectDescription,
                Location = p.ProjectLocation,
                State = p.ProjectState,
                ManagerId = p.ManagerId,
                SupervisorId = p.SupervisorId,
                StartDate = p.StartDate,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedDate,
                IsActive = p.State.IsActive,
                HasSupervisor = p.HasSupervisor,
                IsReadyToStart = p.IsReadyToStart()
            }).ToList();

            _logger.LogDebug("Found {Count} projects in state {State}", projectInfos.Count, state);
            return projectInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by state: {State}", state);
            return new List<ProjectInfo>();
        }
    }

    public async Task<List<ProjectInfo>> GetAllProjectsAsync()
    {
        try
        {
            _logger.LogDebug("Getting all projects");
            
            var projects = await _projectRepository.ListAsync();
            
            var projectInfos = projects.Select(p => new ProjectInfo
            {
                Id = p.Id,
                Name = p.ProjectName,
                Description = p.ProjectDescription,
                Location = p.ProjectLocation,
                State = p.ProjectState,
                ManagerId = p.ManagerId,
                SupervisorId = p.SupervisorId,
                StartDate = p.StartDate,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedDate,
                IsActive = p.State.IsActive,
                HasSupervisor = p.HasSupervisor,
                IsReadyToStart = p.IsReadyToStart()
            }).ToList();

            _logger.LogDebug("Retrieved {Count} total projects", projectInfos.Count);
            return projectInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all projects");
            return new List<ProjectInfo>();
        }
    }
}