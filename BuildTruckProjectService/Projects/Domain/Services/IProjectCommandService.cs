using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Projects.Domain.Model.Commands;

namespace BuildTruckProjectService.Projects.Domain.Services;

/// <summary>
/// Interface for Project Command Service
/// </summary>
/// <remarks>
/// Defines contract for project command operations following DDD patterns
/// </remarks>
public interface IProjectCommandService
{
    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="command">Command with project creation data</param>
    /// <returns>Created project or null if creation failed</returns>
    Task<Project?> Handle(CreateProjectCommand command);

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="command">Command with project update data</param>
    /// <returns>Updated project or null if update failed</returns>
    Task<Project?> Handle(UpdateProjectCommand command);

    /// <summary>
    /// Deletes a project
    /// </summary>
    /// <param name="command">Command with project deletion data</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> Handle(DeleteProjectCommand command);
}