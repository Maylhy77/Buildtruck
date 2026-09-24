namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record MaterialHistoryMetricsResource(
    int TotalMaterials,
    int MaterialsOutOfStock,
    decimal TotalMaterialCost,
    decimal InventoryHealthScore
);
