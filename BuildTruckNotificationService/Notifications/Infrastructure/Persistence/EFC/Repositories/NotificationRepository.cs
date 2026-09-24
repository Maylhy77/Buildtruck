using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationRepository
    : BaseRepository<Notification, NotificationServiceDbContext>, INotificationRepository
{
    public NotificationRepository(NotificationServiceDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> FindByUserIdWithFiltersAsync(int userId, int page, int size,
        bool? isRead, NotificationContext? context, NotificationPriority? minimumPriority, int? relatedProjectId)
    {
        var query = Context.Set<Notification>().Where(n => n.UserId == userId);

        if (isRead.HasValue) query = query.Where(n => n.IsRead == isRead.Value);
        if (context != null) query = query.Where(n => n.Context == context);
        if (minimumPriority != null) query = query.Where(n => n.Priority.Level >= minimumPriority.Level);
        if (relatedProjectId.HasValue) query = query.Where(n => n.RelatedProjectId == relatedProjectId.Value);

        return await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<int> CountUnreadByUserIdAsync(int userId)
    {
        return await Context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Dictionary<string, object>> GetSummaryByUserIdAsync(int userId)
    {
        var notifications = await Context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .Select(n => new { Context = n.Context.Value })
            .ToListAsync();

        return notifications
            .GroupBy(x => x.Context)
            .ToDictionary(g => g.Key, g => (object)g.Count());
    }

    public async Task<IEnumerable<Notification>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Notification>()
            .Where(n => n.RelatedProjectId == projectId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task BulkMarkAsReadAsync(List<int> notificationIds, int userId)
    {
        var notifications = await Context.Set<Notification>()
            .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications) n.MarkAsRead();
        await Context.SaveChangesAsync();
    }

    public async Task DeleteOldNotificationsAsync(int userId, int daysOld)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        var old = await Context.Set<Notification>()
            .Where(n => n.UserId == userId && n.IsRead && n.CreatedDate < cutoff)
            .ToListAsync();

        Context.Set<Notification>().RemoveRange(old);
        await Context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> FindByTypeAndContextAsync(
        NotificationType type, NotificationContext context, DateTime since)
    {
        return await Context.Set<Notification>()
            .Where(n => n.Type == type && n.Context == context && n.CreatedDate >= since)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }
}
