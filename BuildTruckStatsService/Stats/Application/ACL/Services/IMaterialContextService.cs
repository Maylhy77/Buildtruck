namespace BuildTruckStatsService.Stats.Application.ACL.Services;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public interface IMaterialContextService
{
    Task<MaterialMetrics> GetMaterialMetricsAsync(List<int> projectIds, StatsPeriod period);
    Task<Dictionary<string, int>> GetMaterialsByCategoryAsync(List<int> projectIds);
    Task<decimal> GetTotalMaterialCostAsync(List<int> projectIds, StatsPeriod period);
    Task<decimal> GetTotalUsageCostAsync(List<int> projectIds, StatsPeriod period);
    Task<int> GetMaterialsInStockCountAsync(List<int> projectIds);
    Task<int> GetMaterialsLowStockCountAsync(List<int> projectIds);
    Task<int> GetMaterialsOutOfStockCountAsync(List<int> projectIds);
    Task<Dictionary<string, decimal>> GetCostsByCategoryAsync(List<int> projectIds, StatsPeriod period);
    Task<int> GetTotalMaterialsCountAsync(List<int> projectIds);
}
