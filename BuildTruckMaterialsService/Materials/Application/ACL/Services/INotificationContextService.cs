namespace BuildTruckMaterialsService.Materials.Application.ACL.Services;

public interface INotificationContextService
{
    Task NotifyMaterialAddedAsync(int projectId, int materialId, string materialName);
    Task NotifyLowStockAsync(int projectId, string materialName, decimal stock);
    Task NotifyCriticalStockAsync(int projectId, string materialName);
    Task NotifyMaterialUsedAsync(int projectId, decimal quantity, string usageType, string? observations);
}
