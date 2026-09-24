using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Commands
{
    public record UpdateMaterialUsageCommand(
        int UsageId,     // INT
        DateTime Date,
        decimal Quantity,
        string Area,
        string UsageType,
        string Worker,
        string Observations
    );
}