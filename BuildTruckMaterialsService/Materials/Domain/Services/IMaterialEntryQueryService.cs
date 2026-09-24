// Materials/Domain/Services/IMaterialEntryQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for querying material entries
    /// </summary>
    public interface IMaterialEntryQueryService
    {
        /// <summary>
        /// Gets all material entries for a given project
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of material entries</returns>
        Task<List<MaterialEntry>> Handle(GetMaterialEntriesByProjectQuery query);

        /// <summary>
        /// Gets a single material entry by its ID
        /// </summary>
        /// <param name="query">Query with entry ID</param>
        /// <returns>Material entry details</returns>
        Task<MaterialEntry?> Handle(GetMaterialEntryByIdQuery query);
    }
}