using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Commands
{
    public record CreateMaterialUsageCommand(
        int ProjectId,   // INT
        int MaterialId,  // INT
        DateTime Date,
        decimal Quantity,
        string Area,
        string UsageType,
        string Worker,
        string Observations
    );
}