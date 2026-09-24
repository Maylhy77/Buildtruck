using System;
using System.Threading.Tasks;

namespace BuildTruckMaterialsService.Materials.Application.ACL.Services
{
    /// <summary>
    /// Service interface for accessing project context information
    /// </summary>
    public interface IProjectContextService
    {
        /// <summary>
        /// Validates if a project exists and the user has access to it
        /// </summary>
        /// <param name="projectId">Project identifier (Guid - deprecated)</param>
        /// <returns>True if project exists and user has access</returns>
        [Obsolete("Use ValidateProjectAccessAsync(int projectId) instead")]
        Task<bool> ValidateProjectAccessAsync(Guid projectId);
        
        /// <summary>
        /// Validates if a project exists and the user has access to it
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>True if project exists and user has access</returns>
        Task<bool> ValidateProjectAccessAsync(int projectId);

        /// <summary>
        /// Gets the project name by its ID
        /// </summary>
        /// <param name="projectId">Project identifier (Guid - deprecated)</param>
        /// <returns>Project name or null if not found</returns>
        [Obsolete("Use GetProjectNameAsync(int projectId) instead")]
        Task<string?> GetProjectNameAsync(Guid projectId);
        
        /// <summary>
        /// Gets the project name by its ID
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>Project name or null if not found</returns>
        Task<string?> GetProjectNameAsync(int projectId);
    }
}