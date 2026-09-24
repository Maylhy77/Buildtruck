using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving material entries by project
    /// </summary>
    /// <param name="ProjectId">Project ID</param>
    public record GetMaterialEntriesByProjectQuery(int ProjectId);
}