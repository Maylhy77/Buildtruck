using System;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Resources
{
    public record MaterialEntryResource(
        int Id,
        int ProjectId,
        int MaterialId,
        DateTime Date,
        decimal Quantity,
        string Unit,
        string Provider,
        string Ruc,
        string Payment,
        string DocumentType,
        string DocumentNumber,
        decimal UnitCost,
        decimal TotalCost,
        string Status,
        string? Observations
    );
}