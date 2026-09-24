// Materials/Interfaces/REST/Resources/MaterialResource.cs
using System;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Resources
{
    public record MaterialResource(
        int Id,
        int ProjectId,
        string Name,
        string Type,
        string Unit,
        decimal MinimumStock,
        string Provider,
        decimal Stock,
        decimal Price,
        decimal Total
    );
}