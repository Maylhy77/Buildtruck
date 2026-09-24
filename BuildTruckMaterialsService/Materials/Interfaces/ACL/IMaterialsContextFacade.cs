// Materials/Interfaces/ACL/IMaterialsContextFacade.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildTruckMaterialsService.Materials.Interfaces.ACL
{
    /// <summary>
    /// Facade interface for external bounded contexts to access Materials functionality
    /// </summary>
    public interface IMaterialsContextFacade
    {
        /// <summary>
        /// Gets the total material cost for a specific project
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>Total material cost in PEN</returns>
        Task<decimal> GetProjectMaterialCostAsync(int projectId);

        /// <summary>
        /// Gets current stock for a specific material
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <returns>Current stock quantity</returns>
        Task<decimal> GetMaterialCurrentStockAsync(int materialId);

        /// <summary>
        /// Validates if a material exists and belongs to the specified project
        /// </summary>
        /// <param name="materialId">Material identifier</param>
        /// <param name="projectId">Project identifier</param>
        /// <returns>True if material exists and belongs to project</returns>
        Task<bool> ValidateMaterialExistsInProjectAsync(int materialId, int projectId);

        /// <summary>
        /// Gets material names and IDs for a specific project (for dropdowns, etc.)
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>Dictionary with material ID as key and material name as value</returns>
        Task<Dictionary<int, string>> GetProjectMaterialsLookupAsync(int projectId);

        /// <summary>
        /// Checks if a project has any material entries
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>True if project has materials</returns>
        Task<bool> ProjectHasMaterialsAsync(int projectId);
    }
}
