using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Commands
{
    public record CreateMaterialEntryCommand(
        int ProjectId,    // INT
        int MaterialId,   // INT
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
        string Observations
    );
}