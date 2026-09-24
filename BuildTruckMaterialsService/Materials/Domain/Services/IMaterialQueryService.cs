// Materials/Domain/Services/IMaterialQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for querying materials
    /// </summary>
    public interface IMaterialQueryService
    {
        /// <summary>
        /// Gets all materials for a specific project
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of materials</returns>
        Task<List<Material>> Handle(GetMaterialsByProjectQuery query);

        /// <summary>
        /// Gets a material by its ID
        /// </summary>
        /// <param name="query">Query with material ID</param>
        /// <returns>Material details</returns>
        Task<Material?> Handle(GetMaterialByIdQuery query);
    }
}