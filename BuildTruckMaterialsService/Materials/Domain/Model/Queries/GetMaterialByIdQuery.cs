using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving a material by ID
    /// </summary>
    /// <param name="MaterialId">Material ID</param>
    public record GetMaterialByIdQuery(int MaterialId);
}