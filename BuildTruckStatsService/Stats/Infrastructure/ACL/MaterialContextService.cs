namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

/// <summary>
/// DTO representing a material from the MaterialsService HTTP response.
/// Uses flat properties (Stock, MinimumStock, Type, Price) instead of value object accessors
/// to avoid depending on the Materials domain aggregate from the other bounded context.
/// </summary>
public record MaterialDto(
    int Id,
    int ProjectId,
    string? Name,
    string? Type,
    decimal Stock,
    decimal MinimumStock,
    decimal Price,
    decimal TotalCost);

public class MaterialContextService : IMaterialContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MaterialContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("MaterialsService");

    public MaterialContextService(IHttpClientFactory httpClientFactory, ILogger<MaterialContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MaterialMetrics> GetMaterialMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return MaterialMetrics.FromCounts(0, 0, 0, 0m);

            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            decimal totalCost = allMaterials.Sum(m => m.TotalCost);

            var totalMaterials = allMaterials.Count;
            var materialsInStock = allMaterials.Count(m => m.Stock > 0);
            var materialsLowStock = allMaterials.Count(m => m.Stock > 0 && m.Stock <= m.MinimumStock);
            var materialsOutOfStock = allMaterials.Count(m => m.Stock <= 0);

            var materialsByCategory = allMaterials
                .Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!)
                .ToDictionary(g => g.Key, g => g.Count());

            var costsByCategory = allMaterials
                .Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!)
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Price));

            _logger.LogDebug("Material metrics calculated: {Total} total, {InStock} in stock, cost: {Cost}",
                totalMaterials, materialsInStock, totalCost);

            return new MaterialMetrics(totalMaterials, materialsInStock, materialsLowStock, materialsOutOfStock,
                totalCost, 0m, materialsByCategory, costsByCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material metrics for projects");
            return MaterialMetrics.FromCounts(0, 0, 0, 0m);
        }
    }

    public async Task<Dictionary<string, int>> GetMaterialsByCategoryAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!).ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials by category for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<decimal> GetTotalMaterialCostAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0m;
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Sum(m => m.TotalCost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total material cost for projects");
            return 0m;
        }
    }

    public async Task<decimal> GetTotalUsageCostAsync(List<int> projectIds, StatsPeriod period)
    {
        _logger.LogDebug("Getting total usage cost for {Count} projects (placeholder)", projectIds.Count);
        return 0m;
    }

    public async Task<int> GetMaterialsInStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Count(m => m.Stock > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials in stock count for projects");
            return 0;
        }
    }

    public async Task<int> GetMaterialsLowStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Count(m => m.Stock > 0 && m.Stock <= m.MinimumStock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials low stock count for projects");
            return 0;
        }
    }

    public async Task<int> GetMaterialsOutOfStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Count(m => m.Stock <= 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials out of stock count for projects");
            return 0;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostsByCategoryAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, decimal>();
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Where(m => !string.IsNullOrEmpty(m.Type))
                .GroupBy(m => m.Type!).ToDictionary(g => g.Key, g => g.Sum(m => m.Price));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting costs by category for projects");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<int> GetTotalMaterialsCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allMaterials = await GetMaterialsForProjectsAsync(projectIds);
            return allMaterials.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total materials count for projects");
            return 0;
        }
    }

    private async Task<List<MaterialDto>> GetMaterialsForProjectsAsync(List<int> projectIds)
    {
        var allMaterials = new List<MaterialDto>();
        foreach (var projectId in projectIds)
        {
            try
            {
                var result = await Client.GetFromJsonAsync<List<MaterialDto>>($"/api/v1/materials?projectId={projectId}");
                if (result != null) allMaterials.AddRange(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting materials for project {ProjectId}", projectId);
            }
        }
        return allMaterials;
    }
}
