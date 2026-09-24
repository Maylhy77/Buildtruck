using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Application.ACL.Services;
using BuildTruckNotificationService.Notifications.Application.Internal.CommandServices;
using Microsoft.Extensions.Logging.Abstractions;
using BuildTruckNotificationService.Notifications.Application.Internal.QueryServices;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Commands;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using BuildTruckShared.Domain.Repositories;
using Xunit;

namespace BuildTruckBackend.Tests;

public class NotificationTests
{
    [Fact]
    public void NotificationValueObjects_ParseValidValues_AndRejectInvalidValues()
    {
        var priority = NotificationPriority.FromString("HIGH");
        var context = NotificationContext.FromString("MATERIALS");
        var content = new NotificationContent("", "", "/materials/10");

        Assert.True(priority.RequiresEmailByDefault);
        Assert.True(context.IsMaterials());
        Assert.Equal("Notification", content.Title);
        Assert.Equal("No message", content.Message);
        Assert.True(content.HasAction());
        Assert.Equal("Ver detalles", content.GetDisplayActionText());
        Assert.Throws<ArgumentException>(() => NotificationPriority.FromString("URGENT"));
        Assert.Throws<ArgumentException>(() => NotificationChannel.FromString("SMS"));
    }

    [Fact]
    public void NotificationAggregate_MarksAsRead_AndKeepsCriticalPriorityRules()
    {
        var notification = new Notification(
            userId: 7,
            type: NotificationType.CriticalStock,
            context: NotificationContext.Materials,
            priority: NotificationPriority.Critical,
            content: new NotificationContent("Stock critico", "Cemento agotado"),
            targetRole: UserRole.Manager,
            scope: NotificationScope.Project,
            relatedProjectId: 3,
            relatedEntityId: 11,
            relatedEntityType: "Material");

        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        Assert.Equal(NotificationStatus.Read, notification.Status);
        Assert.True(notification.IsCritical());
        Assert.True(notification.ShouldSendEmailImmediate());
        Assert.True(notification.BelongsToProject(3));
    }

    [Fact]
    public async Task NotificationCommandService_CreatesCriticalNotification_AndDispatchesAllRequiredChannels()
    {
        var repository = new FakeNotificationRepository();
        var deliveryService = new FakeNotificationDeliveryService();
        var webSocketService = new FakeWebSocketService();
        var unitOfWork = new FakeUnitOfWork();
        var queue = new FakeQueuePublisher();
        var service = new NotificationCommandService(
            repository,
            deliveryService,
            webSocketService,
            unitOfWork,
            queue,
            NullLogger<NotificationCommandService>.Instance);

        var id = await service.Handle(new CreateNotificationCommand(
            UserId: 9,
            Type: NotificationType.CriticalStock,
            Context: NotificationContext.Materials,
            Priority: NotificationPriority.Critical,
            Title: "Stock critico",
            Message: "El cemento Portland esta agotado",
            TargetRole: UserRole.Manager,
            Scope: NotificationScope.Project,
            RelatedProjectId: 15,
            RelatedEntityId: 4,
            RelatedEntityType: "Material",
            ActionUrl: "/projects/15/materials/4",
            ActionText: "Revisar inventario"));

        Assert.Equal(1, id);
        Assert.Single(repository.Notifications);
        Assert.Equal(1, unitOfWork.CompleteCalls);

        // IN_APP se resuelve en linea (no hace I/O externo).
        Assert.Contains(deliveryService.DeliveredChannels, channel => channel == NotificationChannel.InApp);

        // Los canales lentos se encolan: el SMTP ya no bloquea la peticion.
        Assert.DoesNotContain(deliveryService.DeliveredChannels, channel => channel == NotificationChannel.Email);
        Assert.Equal(
            new[] { "WEBSOCKET", "EMAIL" },
            queue.Published.Select(m => m.Channel).ToArray());
        Assert.All(queue.Published, m => Assert.Equal(1, m.NotificationId));
    }

    [Fact]
    public async Task NotificationCommandService_DeliversInline_WhenQueueIsUnavailable()
    {
        var repository = new FakeNotificationRepository();
        var deliveryService = new FakeNotificationDeliveryService();
        var unitOfWork = new FakeUnitOfWork();
        var queue = new FakeQueuePublisher(available: false);
        var service = new NotificationCommandService(
            repository,
            deliveryService,
            new FakeWebSocketService(),
            unitOfWork,
            queue,
            NullLogger<NotificationCommandService>.Instance);

        await service.Handle(new CreateNotificationCommand(
            UserId: 9,
            Type: NotificationType.CriticalStock,
            Context: NotificationContext.Materials,
            Priority: NotificationPriority.Critical,
            Title: "Stock critico",
            Message: "El cemento Portland esta agotado",
            TargetRole: UserRole.Manager,
            Scope: NotificationScope.Project,
            RelatedProjectId: 15,
            RelatedEntityId: 4,
            RelatedEntityType: "Material",
            ActionUrl: "/projects/15/materials/4",
            ActionText: "Revisar inventario"));

        // Sin broker se degrada a la entrega en linea: ninguna notificacion se pierde.
        Assert.Empty(queue.Published);
        Assert.Contains(deliveryService.DeliveredChannels, channel => channel == NotificationChannel.WebSocket);
        Assert.Contains(deliveryService.DeliveredChannels, channel => channel == NotificationChannel.Email);
    }

    [Fact]
    public async Task NotificationCommandService_MarksNotificationAsRead_ForOwnerOnly()
    {
        var repository = new FakeNotificationRepository();
        await repository.AddAsync(new Notification(
            userId: 5,
            type: NotificationType.PersonnelAdded,
            context: NotificationContext.Personnel,
            priority: NotificationPriority.Normal,
            content: new NotificationContent("Nuevo personal", "Se agrego un trabajador"),
            targetRole: UserRole.Manager,
            scope: NotificationScope.Project,
            relatedProjectId: 2));
        var unitOfWork = new FakeUnitOfWork();
        var service = new NotificationCommandService(
            repository,
            new FakeNotificationDeliveryService(),
            new FakeWebSocketService(),
            unitOfWork,
            new FakeQueuePublisher(),
            NullLogger<NotificationCommandService>.Instance);

        await service.Handle(new MarkAsReadCommand(NotificationId: 1, UserId: 5));

        var notification = await repository.FindByIdAsync(1);

        Assert.NotNull(notification);
        Assert.True(notification.IsRead);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, unitOfWork.CompleteCalls);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new MarkAsReadCommand(NotificationId: 1, UserId: 99)));
    }

    [Fact]
    public async Task NotificationQueryService_FiltersUserNotifications_AndBuildsSummary()
    {
        var repository = new FakeNotificationRepository();
        var readPersonnelNotification = new Notification(
            userId: 3,
            type: NotificationType.PersonnelAdded,
            context: NotificationContext.Personnel,
            priority: NotificationPriority.Normal,
            content: new NotificationContent("Personal", "Nuevo personal"),
            targetRole: UserRole.Manager,
            scope: NotificationScope.Project,
            relatedProjectId: 7);
        readPersonnelNotification.MarkAsRead();

        await repository.AddAsync(new Notification(
            userId: 3,
            type: NotificationType.LowStock,
            context: NotificationContext.Materials,
            priority: NotificationPriority.High,
            content: new NotificationContent("Stock bajo", "Quedan pocas bolsas"),
            targetRole: UserRole.Manager,
            scope: NotificationScope.Project,
            relatedProjectId: 7));
        await repository.AddAsync(readPersonnelNotification);
        await repository.AddAsync(new Notification(
            userId: 8,
            type: NotificationType.LowStock,
            context: NotificationContext.Materials,
            priority: NotificationPriority.High,
            content: new NotificationContent("Otro usuario", "No debe aparecer"),
            targetRole: UserRole.Manager,
            scope: NotificationScope.Project,
            relatedProjectId: 7));

        var service = new NotificationQueryService(repository);

        var notifications = (await service.Handle(new GetNotificationsByUserQuery(
            UserId: 3,
            IsRead: false,
            Context: NotificationContext.Materials,
            MinimumPriority: NotificationPriority.High,
            RelatedProjectId: 7))).ToList();
        var summary = await service.Handle(new GetNotificationSummaryQuery(UserId: 3));

        var item = Assert.Single(notifications);
        Assert.Equal(NotificationType.LowStock, item.Type);
        Assert.Equal(1, summary["unreadCount"]);
        var byContext = Assert.IsType<Dictionary<string, object>>(summary["byContext"]);
        Assert.Equal(1, byContext["MATERIALS"]);
        Assert.Equal(1, byContext["PERSONNEL"]);
    }

    [Fact]
    public async Task NotificationPreferenceCommandService_CreatesDefaults_AndUpdatesExistingPreference()
    {
        var repository = new FakeNotificationPreferenceRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new NotificationPreferenceCommandService(repository, unitOfWork);

        await service.CreateDefaultPreferencesAsync(userId: 12);
        await service.Handle(new UpdatePreferenceCommand(
            UserId: 12,
            Context: NotificationContext.Materials,
            InAppEnabled: true,
            EmailEnabled: true,
            MinimumPriority: NotificationPriority.Critical));

        var preferences = (await repository.FindByUserIdAsync(12)).ToList();
        var materialsPreference = await repository.FindByUserIdAndContextAsync(12, NotificationContext.Materials);

        Assert.Equal(NotificationContext.GetAllContexts().Count(), preferences.Count);
        Assert.NotNull(materialsPreference);
        Assert.True(materialsPreference.EmailEnabled);
        Assert.Equal(NotificationPriority.Critical, materialsPreference.MinimumPriority);
        Assert.Equal(2, unitOfWork.CompleteCalls);
    }

    private static void SetEntityId(object entity, int id)
    {
        var backingField = entity.GetType().GetField(
            "<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CompleteCalls { get; private set; }

        public Task CompleteAsync()
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationDeliveryService : INotificationDeliveryService
    {
        public List<NotificationChannel> DeliveredChannels { get; } = [];

        public Task DeliverAsync(Notification notification, NotificationChannel channel)
        {
            DeliveredChannels.Add(channel);
            return Task.CompletedTask;
        }

        public Task RetryFailedDeliveriesAsync() => Task.CompletedTask;

        public Task<bool> CanDeliverAsync(Notification notification, NotificationChannel channel) =>
            Task.FromResult(true);
    }

    /// <summary>Cola de mentira: registra lo publicado y simula que el broker responde.</summary>
    private sealed class FakeQueuePublisher : INotificationQueuePublisher
    {
        private readonly bool _available;

        public FakeQueuePublisher(bool available = true) => _available = available;

        public List<NotificationDeliveryMessage> Published { get; } = new();

        public Task<bool> PublishAsync(NotificationDeliveryMessage message, CancellationToken ct = default)
        {
            if (!_available) return Task.FromResult(false);
            Published.Add(message);
            return Task.FromResult(true);
        }
    }

    private sealed class FakeWebSocketService : IWebSocketService
    {
        public int SendToUserCalls { get; private set; }
        public int LastUserId { get; private set; }

        public Task SendToUserAsync(int userId, Notification notification)
        {
            SendToUserCalls++;
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task SendToGroupAsync(string groupName, Notification notification) => Task.CompletedTask;

        public Task SendUnreadCountUpdateAsync(int userId, int unreadCount) => Task.CompletedTask;

        public Task SendNotificationReadAsync(int userId, int notificationId) => Task.CompletedTask;
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private int _nextId = 1;

        public List<Notification> Notifications { get; } = [];
        public int UpdateCalls { get; private set; }

        public Task AddAsync(Notification entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);

            Notifications.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Notification?> FindByIdAsync(int id) =>
            Task.FromResult(Notifications.FirstOrDefault(notification => notification.Id == id));

        public void Update(Notification entity)
        {
            UpdateCalls++;
        }

        public void Remove(Notification entity) => Notifications.Remove(entity);

        public Task<IEnumerable<Notification>> ListAsync() =>
            Task.FromResult<IEnumerable<Notification>>(Notifications);

        public Task<IEnumerable<Notification>> FindByUserIdWithFiltersAsync(
            int userId,
            int page,
            int size,
            bool? isRead,
            NotificationContext? context,
            NotificationPriority? minimumPriority,
            int? relatedProjectId)
        {
            var query = Notifications.Where(notification => notification.UserId == userId);

            if (isRead.HasValue)
                query = query.Where(notification => notification.IsRead == isRead.Value);

            if (context != null)
                query = query.Where(notification => notification.Context == context);

            if (minimumPriority != null)
                query = query.Where(notification => notification.Priority.Level >= minimumPriority.Level);

            if (relatedProjectId.HasValue)
                query = query.Where(notification => notification.RelatedProjectId == relatedProjectId.Value);

            return Task.FromResult<IEnumerable<Notification>>(
                query.Skip((page - 1) * size).Take(size).ToList());
        }

        public Task<int> CountUnreadByUserIdAsync(int userId) =>
            Task.FromResult(Notifications.Count(notification =>
                notification.UserId == userId &&
                !notification.IsRead));

        public Task<Dictionary<string, object>> GetSummaryByUserIdAsync(int userId) =>
            Task.FromResult(
                Notifications
                    .Where(notification => notification.UserId == userId)
                    .GroupBy(notification => notification.Context.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => (object)group.Count()));

        public Task<IEnumerable<Notification>> FindByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<Notification>>(
                Notifications.Where(notification => notification.RelatedProjectId == projectId));

        public Task BulkMarkAsReadAsync(List<int> notificationIds, int userId)
        {
            foreach (var notification in Notifications.Where(notification =>
                         notification.UserId == userId &&
                         notificationIds.Contains(notification.Id)))
            {
                notification.MarkAsRead();
            }

            return Task.CompletedTask;
        }

        public Task DeleteOldNotificationsAsync(int userId, int daysOld)
        {
            Notifications.RemoveAll(notification => notification.UserId == userId && notification.CanBeDeleted());
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Notification>> FindByTypeAndContextAsync(
            NotificationType type,
            NotificationContext context,
            DateTime since) =>
            Task.FromResult<IEnumerable<Notification>>(
                Notifications.Where(notification =>
                    notification.Type == type &&
                    notification.Context == context));
    }

    private sealed class FakeNotificationPreferenceRepository : INotificationPreferenceRepository
    {
        private int _nextId = 1;

        public List<NotificationPreference> Preferences { get; } = [];

        public Task AddAsync(NotificationPreference entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);

            Preferences.Add(entity);
            return Task.CompletedTask;
        }

        public Task<NotificationPreference?> FindByIdAsync(int id) =>
            Task.FromResult(Preferences.FirstOrDefault(preference => preference.Id == id));

        public void Update(NotificationPreference entity)
        {
        }

        public void Remove(NotificationPreference entity) => Preferences.Remove(entity);

        public Task<IEnumerable<NotificationPreference>> ListAsync() =>
            Task.FromResult<IEnumerable<NotificationPreference>>(Preferences);

        public Task<IEnumerable<NotificationPreference>> FindByUserIdAsync(int userId) =>
            Task.FromResult<IEnumerable<NotificationPreference>>(
                Preferences.Where(preference => preference.UserId == userId));

        public Task<NotificationPreference?> FindByUserIdAndContextAsync(int userId, NotificationContext context) =>
            Task.FromResult(Preferences.FirstOrDefault(preference =>
                preference.UserId == userId &&
                preference.Context == context));

        public Task<bool> ExistsByUserIdAndContextAsync(int userId, NotificationContext context) =>
            Task.FromResult(Preferences.Any(preference =>
                preference.UserId == userId &&
                preference.Context == context));
    }
}
