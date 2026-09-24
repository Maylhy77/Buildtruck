using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Commands;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Services;

namespace BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;

public class NotificationFacade : INotificationFacade
{
    private readonly INotificationCommandService _commandService;
    private readonly INotificationQueryService _queryService;
    private readonly INotificationPreferenceCommandService _preferenceCommandService;
    private readonly INotificationPreferenceQueryService _preferenceQueryService;
    private readonly IWebSocketService _webSocketService;

    public NotificationFacade(
        INotificationCommandService commandService,
        INotificationQueryService queryService,
        INotificationPreferenceCommandService preferenceCommandService,
        INotificationPreferenceQueryService preferenceQueryService,
        IWebSocketService webSocketService)
    {
        _commandService = commandService;
        _queryService = queryService;
        _preferenceCommandService = preferenceCommandService;
        _preferenceQueryService = preferenceQueryService;
        _webSocketService = webSocketService;
    }

    public async Task<int> CreateNotificationAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, string? actionText = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null)
    {
        var command = new CreateNotificationCommand(
            userId, type, context, priority ?? NotificationPriority.Normal, title, message,
            targetRole ?? UserRole.Manager, scope ?? NotificationScope.User,
            relatedProjectId, relatedEntityId, relatedEntityType, actionUrl, actionText);

        var id = await _commandService.Handle(command);
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
        return id;
    }

    public async Task<int> CreateBulkNotificationAsync(List<int> userIds, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, int? relatedProjectId = null)
    {
        int lastId = 0;
        foreach (var userId in userIds)
            lastId = await CreateNotificationAsync(userId, type, context, title, message,
                priority, targetRole, scope, actionUrl, relatedProjectId: relatedProjectId);
        return lastId;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int page = 1, int size = 20,
        bool? isRead = null, NotificationContext? context = null, int? relatedProjectId = null)
    {
        var query = new GetNotificationsByUserQuery(userId, page, size, isRead, context, null, relatedProjectId);
        var result = await _queryService.Handle(query);
        return result.ToList();
    }

    public async Task<Dictionary<string, object>> GetNotificationSummaryAsync(int userId)
    {
        return await _queryService.Handle(new GetNotificationSummaryQuery(userId));
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId, int userId)
    {
        return await _queryService.Handle(new GetNotificationByIdQuery(notificationId, userId));
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        await _commandService.Handle(new MarkAsReadCommand(notificationId, userId));
        await _webSocketService.SendNotificationReadAsync(userId, notificationId);
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
    }

    public async Task MarkAllAsReadAsync(List<int> notificationIds, int userId)
    {
        await _commandService.Handle(new BulkMarkAsReadCommand(notificationIds, userId));
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
    }

    public async Task CleanOldNotificationsAsync(int userId, int daysOld = 30)
    {
        await _commandService.Handle(new CleanOldNotificationsCommand(userId, daysOld));
    }

    public async Task<List<NotificationPreference>> GetUserPreferencesAsync(int userId)
    {
        var result = await _preferenceQueryService.Handle(new GetUserPreferencesQuery(userId));
        return result.ToList();
    }

    public async Task UpdatePreferenceAsync(int userId, NotificationContext context, bool inAppEnabled,
        bool emailEnabled, NotificationPriority minimumPriority)
    {
        await _preferenceCommandService.Handle(new UpdatePreferenceCommand(userId, context, inAppEnabled, emailEnabled, minimumPriority));
    }

    public async Task CreateDefaultPreferencesAsync(int userId)
    {
        await _preferenceCommandService.CreateDefaultPreferencesAsync(userId);
    }

    private async Task<int> GetUnreadCountAsync(int userId)
    {
        var summary = await GetNotificationSummaryAsync(userId);
        return summary.TryGetValue("unreadCount", out var count) ? (int)count : 0;
    }
}
