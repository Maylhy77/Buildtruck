using System;
using System.ComponentModel.DataAnnotations;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Resources
{
    public record CreateOrUpdateMaterialEntryResource(
        int? Id, // Si viene con ID, es update; si es null, es create
        [Required] int ProjectId,
        [Required] int MaterialId,
        [Required] DateTime Date,
        [Required][Range(0.01, 999999.99)] decimal Quantity,
        [Required][StringLength(100)] string Provider,
        [Required][StringLength(11)] string Ruc,
        [Required][StringLength(50)] string Payment,
        [Required][StringLength(50)] string DocumentType,
        [Required][StringLength(50)] string DocumentNumber,
        [Required][Range(0.01, 9999999.99)] decimal UnitCost,
        [StringLength(500)] string? Observations,
        // ? AGREGAR: Campo para el status
        [StringLength(50)] string? Status = "PENDING"
    );
}