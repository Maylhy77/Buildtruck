using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationDeliveryRepository
    : BaseRepository<NotificationDelivery, NotificationServiceDbContext>, INotificationDeliveryRepository
{
    public NotificationDeliveryRepository(NotificationServiceDbContext context) : base(context) { }

    public async Task<IEnumerable<NotificationDelivery>> FindByNotificationIdAsync(int notificationId)
    {
        return await Context.Set<NotificationDelivery>()
            .Where(nd => nd.NotificationId == notificationId)
            .ToListAsync();
    }

    public async Task<NotificationDelivery?> FindByNotificationIdAndChannelAsync(
        int notificationId, NotificationChannel channel)
    {
        return await Context.Set<NotificationDelivery>()
            .FirstOrDefaultAsync(nd => nd.NotificationId == notificationId && nd.Channel == channel);
    }

    public async Task<IEnumerable<NotificationDelivery>> FindRetryableDeliveriesAsync()
    {
        return await Context.Set<NotificationDelivery>()
            .Where(nd => (nd.Status == DeliveryStatus.Failed || nd.Status == DeliveryStatus.Retrying)
                      && nd.AttemptCount < 3)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetDeliveryStatsAsync()
    {
        return await Context.Set<NotificationDelivery>()
            .GroupBy(nd => nd.Status.Value)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
