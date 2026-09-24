// Materials/Application/ACL/Services/IUserContextService.cs (Updated)
using System;
using System.Threading.Tasks;

namespace BuildTruckMaterialsService.Materials.Application.ACL.Services
{
    /// <summary>
    /// Service interface for accessing user context information
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>
        /// Gets the current authenticated user ID
        /// </summary>
        /// <returns>Current user ID</returns>
        int GetCurrentUserId();

        /// <summary>
        /// Gets the current authenticated user email
        /// </summary>
        /// <returns>Current user email</returns>
        string GetCurrentUserEmail();

        /// <summary>
        /// Validates if the current user has permission to access a project
        /// </summary>
        /// <param name="projectId">Project identifier (Guid - deprecated)</param>
        /// <returns>True if user has access</returns>
        [Obsolete("Use HasProjectAccessAsync(int projectId) instead")]
        Task<bool> HasProjectAccessAsync(Guid projectId);
        
        /// <summary>
        /// Validates if the current user has permission to access a project
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>True if user has access</returns>
        Task<bool> HasProjectAccessAsync(int projectId);
    }
}