using System;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Resources
{
    public record InventoryItemResource(
        int MaterialId,
        string Name,
        string Type,
        string Unit,
        decimal MinimumStock,
        string Provider,
        decimal TotalEntries,    
        decimal TotalUsages,     
        decimal StockActual,
        decimal UnitPrice,
        decimal Total
    );
}