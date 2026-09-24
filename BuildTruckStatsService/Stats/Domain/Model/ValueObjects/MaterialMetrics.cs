namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class MaterialMetrics
{
    public int TotalMaterials { get; private set; }
    public int MaterialsInStock { get; private set; }
    public int MaterialsLowStock { get; private set; }
    public int MaterialsOutOfStock { get; private set; }
    public decimal TotalMaterialCost { get; private set; }
    public decimal TotalUsageCost { get; private set; }
    public Dictionary<string, int> MaterialsByCategory { get; private set; }
    public Dictionary<string, decimal> CostsByCategory { get; private set; }
    public decimal AverageUsageRate { get; private set; }

    protected MaterialMetrics()
    {
        MaterialsByCategory = new Dictionary<string, int>();
        CostsByCategory = new Dictionary<string, decimal>();
    }

    public MaterialMetrics(int totalMaterials, int materialsInStock, int materialsLowStock, int materialsOutOfStock,
        decimal totalMaterialCost, decimal totalUsageCost, Dictionary<string, int>? materialsByCategory = null,
        Dictionary<string, decimal>? costsByCategory = null, decimal averageUsageRate = 0m)
    {
        TotalMaterials = Math.Max(0, totalMaterials);
        MaterialsInStock = Math.Max(0, materialsInStock);
        MaterialsLowStock = Math.Max(0, materialsLowStock);
        MaterialsOutOfStock = Math.Max(0, materialsOutOfStock);
        TotalMaterialCost = Math.Max(0m, totalMaterialCost);
        TotalUsageCost = Math.Max(0m, totalUsageCost);
        MaterialsByCategory = materialsByCategory ?? new Dictionary<string, int>();
        CostsByCategory = costsByCategory ?? new Dictionary<string, decimal>();
        AverageUsageRate = Math.Max(0m, Math.Min(100m, averageUsageRate));
    }

    public static MaterialMetrics FromCounts(int total, int inStock, int lowStock, decimal totalCost,
        decimal usageCost = 0m, Dictionary<string, int>? byCategory = null)
    {
        var outOfStock = Math.Max(0, total - inStock - lowStock);
        return new MaterialMetrics(total, inStock, lowStock, outOfStock, totalCost, usageCost, byCategory);
    }

    public decimal GetStockRate()
    {
        if (TotalMaterials == 0) return 0m;
        return Math.Round((decimal)MaterialsInStock / TotalMaterials * 100, 2);
    }

    public decimal GetLowStockRate()
    {
        if (TotalMaterials == 0) return 0m;
        return Math.Round((decimal)MaterialsLowStock / TotalMaterials * 100, 2);
    }

    public decimal GetOutOfStockRate()
    {
        if (TotalMaterials == 0) return 0m;
        return Math.Round((decimal)MaterialsOutOfStock / TotalMaterials * 100, 2);
    }

    public decimal GetCostEfficiencyRate()
    {
        if (TotalMaterialCost == 0m) return 0m;
        return Math.Round(TotalUsageCost / TotalMaterialCost * 100, 2);
    }

    public decimal GetAverageMaterialCost()
    {
        if (TotalMaterials == 0) return 0m;
        return Math.Round(TotalMaterialCost / TotalMaterials, 2);
    }

    public string GetStockStatus()
    {
        var outOfStockRate = GetOutOfStockRate();
        var lowStockRate = GetLowStockRate();
        return (outOfStockRate, lowStockRate) switch
        {
            (0m, 0m) => "Excelente",
            (0m, <= 10m) => "Bueno",
            (0m, <= 25m) => "Regular",
            (> 0m and <= 5m, _) => "Atención requerida",
            _ => "Crítico"
        };
    }

    public bool NeedsRestocking() => MaterialsOutOfStock > 0 || MaterialsLowStock > TotalMaterials * 0.3m;

    public string GetMostUsedCategory()
    {
        if (!CostsByCategory.Any()) return "Sin datos";
        return CostsByCategory.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public string GetLargestCategory()
    {
        if (!MaterialsByCategory.Any()) return "Sin datos";
        return MaterialsByCategory.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public decimal GetInventoryHealthScore()
    {
        if (TotalMaterials == 0) return 100m;
        var baseScore = 100m;
        baseScore -= GetOutOfStockRate() * 2m;
        baseScore -= GetLowStockRate() * 0.5m;
        if (GetCostEfficiencyRate() > 80m) baseScore += 10m;
        return Math.Max(0m, Math.Min(100m, baseScore));
    }

    public string GetMaterialSummary()
    {
        if (TotalMaterials == 0) return "Sin materiales";
        var summary = $"{TotalMaterials} materiales";
        if (MaterialsOutOfStock > 0) summary += $" ({MaterialsOutOfStock} agotados)";
        if (MaterialsLowStock > 0) summary += $" ({MaterialsLowStock} bajo stock)";
        return summary;
    }

    public string GetCostSummary()
    {
        if (TotalMaterialCost == 0m) return "Sin costos registrados";
        var summary = $"Costo total: S/. {TotalMaterialCost:N2}";
        if (TotalUsageCost > 0m) summary += $" (Usado: S/. {TotalUsageCost:N2})";
        return summary;
    }

    public List<string> GetStockAlerts()
    {
        var alerts = new List<string>();
        if (MaterialsOutOfStock > 0) alerts.Add($"{MaterialsOutOfStock} materiales agotados");
        if (MaterialsLowStock > 0) alerts.Add($"{MaterialsLowStock} materiales con bajo stock");
        if (GetOutOfStockRate() > 10m) alerts.Add("Alto porcentaje de materiales agotados");
        return alerts;
    }

    public MaterialMetrics AddMaterial(string category, decimal cost, bool inStock, bool lowStock = false)
    {
        var newByCategory = new Dictionary<string, int>(MaterialsByCategory);
        var newCostsByCategory = new Dictionary<string, decimal>(CostsByCategory);

        if (newByCategory.ContainsKey(category)) newByCategory[category]++; else newByCategory[category] = 1;
        if (newCostsByCategory.ContainsKey(category)) newCostsByCategory[category] += cost; else newCostsByCategory[category] = cost;

        var newInStock = inStock ? MaterialsInStock + 1 : MaterialsInStock;
        var newLowStock = lowStock ? MaterialsLowStock + 1 : MaterialsLowStock;
        var newOutOfStock = !inStock && !lowStock ? MaterialsOutOfStock + 1 : MaterialsOutOfStock;

        return new MaterialMetrics(TotalMaterials + 1, newInStock, newLowStock, newOutOfStock,
            TotalMaterialCost + cost, TotalUsageCost, newByCategory, newCostsByCategory, AverageUsageRate);
    }

    public override string ToString() => $"Materials: {TotalMaterials} total (S/. {TotalMaterialCost:N2}, {GetInventoryHealthScore():F1}% health)";

    public override bool Equals(object? obj)
    {
        if (obj is not MaterialMetrics other) return false;
        return TotalMaterials == other.TotalMaterials && MaterialsInStock == other.MaterialsInStock &&
               MaterialsLowStock == other.MaterialsLowStock && MaterialsOutOfStock == other.MaterialsOutOfStock &&
               TotalMaterialCost == other.TotalMaterialCost;
    }

    public override int GetHashCode() => HashCode.Combine(TotalMaterials, MaterialsInStock, MaterialsLowStock, MaterialsOutOfStock, TotalMaterialCost);
}
