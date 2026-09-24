using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving material usages by project
    /// </summary>
    /// <param name="ProjectId">Project ID</param>
    public record GetMaterialUsagesByProjectQuery(int ProjectId);
}