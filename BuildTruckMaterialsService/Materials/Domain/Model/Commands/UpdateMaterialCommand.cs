namespace BuildTruckMaterialsService.Materials.Domain.Model.Commands
{
    public record UpdateMaterialCommand(
        int MaterialId,  // INT
        string Name,
        string Type,
        string Unit,
        decimal MinimumStock,
        string Provider
    );
}