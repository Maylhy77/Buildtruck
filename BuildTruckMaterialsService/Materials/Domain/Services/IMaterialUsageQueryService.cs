// Materials/Domain/Services/IMaterialUsageQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for querying material usage records
    /// </summary>
    public interface IMaterialUsageQueryService
    {
        /// <summary>
        /// Gets all material usage records for a specific project
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of material usage records</returns>
        Task<List<MaterialUsage>> Handle(GetMaterialUsagesByProjectQuery query);

        /// <summary>
        /// Gets a material usage record by its ID
        /// </summary>
        /// <param name="query">Query with usage ID</param>
        /// <returns>Material usage details</returns>
        Task<MaterialUsage?> Handle(GetMaterialUsageByIdQuery query);
    }
}