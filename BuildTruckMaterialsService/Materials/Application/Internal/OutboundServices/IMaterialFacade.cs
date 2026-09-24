using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;

namespace BuildTruckMaterialsService.Materials.Application.Internal.OutboundServices
{
    /// <summary>
    /// Facade interface for external access to Materials bounded context
    /// </summary>
    public interface IMaterialFacade
    {
        /// <summary>
        /// Gets all materials for a specific project
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>List of materials</returns>
        Task<List<Material>> GetMaterialsByProjectAsync(int projectId);

        /// <summary>
        /// Gets current stock for a specific material
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <returns>Current stock quantity</returns>
        Task<decimal> GetMaterialStockAsync(int materialId);

        /// <summary>
        /// Validates if a material exists and belongs to the specified project
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <param name="projectId">Project identifier</param>
        /// <returns>True if material exists and belongs to project</returns>
        Task<bool> ValidateMaterialExistsInProjectAsync(int materialId, int projectId);

        /// <summary>
        /// Gets the total cost of materials used in a project
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>Total material cost</returns>
        Task<decimal> GetProjectMaterialCostAsync(int projectId);
    }
}