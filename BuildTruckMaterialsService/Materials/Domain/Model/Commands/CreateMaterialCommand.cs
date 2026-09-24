using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Commands
{
    /// <summary>
    /// Command to create a new base material within a project.
    /// This command carries all required information to construct a Material aggregate.
    /// </summary>
    /// <param name="ProjectId">Unique identifier of the project where the material belongs.</param>
    /// <param name="Name">Name of the material (e.g., Cement Type I).</param>
    /// <param name="Type">Material type (e.g., CEMENTO, ACERO, PINTURA).</param>
    /// <param name="Unit">Measurement unit of the material (e.g., KG, M, UND).</param>
    /// <param name="MinimumStock">Minimum stock level required for alerts or tracking.</param>
    /// <param name="Provider">Primary supplier or vendor of the material.</param>
    public record CreateMaterialCommand(
        int ProjectId,
        string Name,
        string Type,
        string Unit,
        decimal MinimumStock,
        string Provider
    );
}
