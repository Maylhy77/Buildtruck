namespace BuildTruckStatsService.Stats.Application.ACL.Services;

public interface IUserContextService
{
    Task<bool> IsValidManagerAsync(int userId);
    Task<Dictionary<string, object>?> GetManagerInfoAsync(int managerId);
}
