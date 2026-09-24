using BuildTruckMaterialsService.Materials.Application.ACL.Services;

namespace BuildTruckMaterialsService.Materials.Infrastructure.ACL;

public class NotificationContextService : INotificationContextService
{
    public Task NotifyMaterialAddedAsync(int projectId, int materialId, string materialName) =>
        Task.CompletedTask;

    public Task NotifyLowStockAsync(int projectId, string materialName, decimal stock) =>
        Task.CompletedTask;

    public Task NotifyCriticalStockAsync(int projectId, string materialName) =>
        Task.CompletedTask;

    public Task NotifyMaterialUsedAsync(int projectId, decimal quantity, string usageType, string? observations) =>
        Task.CompletedTask;
}
