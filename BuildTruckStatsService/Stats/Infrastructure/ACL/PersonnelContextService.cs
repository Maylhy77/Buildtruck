namespace BuildTruckStatsService.Stats.Infrastructure.ACL;

using System.Net.Http.Json;
using BuildTruckStatsService.Stats.Application.ACL.Services;
using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class PersonnelContextService : IPersonnelContextService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PersonnelContextService> _logger;
    private HttpClient Client => _httpClientFactory.CreateClient("PersonnelService");

    public PersonnelContextService(IHttpClientFactory httpClientFactory, ILogger<PersonnelContextService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PersonnelMetrics> GetPersonnelMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return PersonnelMetrics.FromCounts(0, 0);

            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);

            var totalPersonnel = allPersonnel.Count;
            var activePersonnel = allPersonnel.Count(p => IsActiveStatus(p.Status));
            var personnelByType = allPersonnel
                .Where(p => !string.IsNullOrEmpty(p.Type))
                .GroupBy(p => p.Type!)
                .ToDictionary(g => g.Key, g => g.Count());
            var totalSalary = allPersonnel.Sum(p => p.Salary);

            return new PersonnelMetrics(totalPersonnel, activePersonnel, totalPersonnel - activePersonnel,
                personnelByType, null, totalSalary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel metrics for projects");
            return PersonnelMetrics.FromCounts(0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();
            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);
            return allPersonnel.Where(p => !string.IsNullOrEmpty(p.Type))
                .GroupBy(p => p.Type!)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel by type");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetActivePersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);
            return allPersonnel.Count(p => IsActiveStatus(p.Status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active personnel count");
            return 0;
        }
    }

    public async Task<int> GetTotalPersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;
            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);
            return allPersonnel.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total personnel count");
            return 0;
        }
    }

    public async Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0m;
            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);
            if (!allPersonnel.Any()) return 0m;
            return allPersonnel.Average(p => p.AttendanceRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average attendance rate");
            return 0m;
        }
    }

    public async Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0m;
            var allPersonnel = await GetPersonnelForProjectsAsync(projectIds);
            return allPersonnel.Sum(p => p.Salary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total salary amount");
            return 0m;
        }
    }

    private async Task<List<PersonnelDto>> GetPersonnelForProjectsAsync(List<int> projectIds)
    {
        var allPersonnel = new List<PersonnelDto>();
        foreach (var projectId in projectIds)
        {
            try
            {
                var result = await Client.GetFromJsonAsync<List<PersonnelDto>>($"/api/v1/personnel?projectId={projectId}");
                if (result != null) allPersonnel.AddRange(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting personnel for project {ProjectId}", projectId);
            }
        }
        return allPersonnel;
    }

    private static bool IsActiveStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var n = status.ToLowerInvariant();
        return n is "activo" or "active" or "working" or "trabajando";
    }

    private record PersonnelDto(int Id, string? Status, string? Type, decimal Salary, decimal AttendanceRate);
}
