using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for querying consolidated inventory by project
    /// </summary>
    public interface IInventoryQueryService
    {
        /// <summary>
        /// Gets the current inventory summary for a given project
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of materials with updated stock and prices</returns>
        Task<List<Material>> Handle(GetInventoryByProjectQuery query);
        
        /// <summary>
        /// Gets the detailed inventory summary with entries and usages totals
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of inventory items with complete information</returns>
        Task<List<InventoryItemResource>> HandleDetailed(GetInventoryByProjectQuery query);
    }
}