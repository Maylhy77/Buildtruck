using System;
using System.ComponentModel.DataAnnotations;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Resources
{
    public record CreateOrUpdateMaterialResource(
        int? Id, // Si viene con ID, es update; si es null, es create
        [Required] int ProjectId,
        [Required][StringLength(100)] string Name,
        [Required][StringLength(50)] string Type,
        [Required][StringLength(20)] string Unit,
        [Required][Range(0, 999999.99)] decimal MinimumStock,
        [Required][StringLength(100)] string Provider
    );
}