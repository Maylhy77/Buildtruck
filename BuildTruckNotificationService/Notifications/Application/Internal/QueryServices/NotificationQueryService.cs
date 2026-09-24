using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;

namespace BuildTruckNotificationService.Notifications.Application.Internal.QueryServices;

public class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationQueryService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> Handle(GetNotificationsByUserQuery query)
    {
        return await _notificationRepository.FindByUserIdWithFiltersAsync(
            query.UserId, query.Page, query.Size, query.IsRead,
            query.Context, query.MinimumPriority, query.RelatedProjectId);
    }

    public async Task<Dictionary<string, object>> Handle(GetNotificationSummaryQuery query)
    {
        var unreadCount = await _notificationRepository.CountUnreadByUserIdAsync(query.UserId);
        var summaryByContext = await _notificationRepository.GetSummaryByUserIdAsync(query.UserId);

        return new Dictionary<string, object>
        {
            ["unreadCount"] = unreadCount,
            ["byContext"] = summaryByContext,
            ["lastUpdated"] = DateTime.UtcNow
        };
    }

    public async Task<Notification?> Handle(GetNotificationByIdQuery query)
    {
        var notification = await _notificationRepository.FindByIdAsync(query.NotificationId);
        if (notification == null || notification.UserId != query.UserId) return null;
        return notification;
    }
}
