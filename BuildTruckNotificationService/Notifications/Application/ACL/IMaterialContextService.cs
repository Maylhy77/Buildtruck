namespace BuildTruckNotificationService.Notifications.Application.ACL;

public interface IMaterialContextService
{
    Task<IEnumerable<int>> GetLowStockMaterialsAsync(int projectId);
    Task<string> GetMaterialNameAsync(int materialId);
    Task<decimal> GetMaterialStockAsync(int materialId);
    Task<decimal> GetMaterialMinimumStockAsync(int materialId);
}
