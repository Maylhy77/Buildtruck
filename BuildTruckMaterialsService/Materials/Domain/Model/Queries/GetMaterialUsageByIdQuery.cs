using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving a material usage by ID
    /// </summary>
    /// <param name="UsageId">Material usage ID</param>
    public record GetMaterialUsageByIdQuery(int UsageId);
}