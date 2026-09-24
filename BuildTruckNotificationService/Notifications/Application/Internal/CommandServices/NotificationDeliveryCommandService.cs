using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Application.Internal.CommandServices;

public class NotificationDeliveryCommandService : INotificationDeliveryService
{
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly ISharedEmailService _emailService;
    private readonly IWebSocketService _webSocketService;
    private readonly IUserContextService _userContextService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationDeliveryCommandService(
        INotificationDeliveryRepository deliveryRepository,
        ISharedEmailService emailService,
        IWebSocketService webSocketService,
        IUserContextService userContextService,
        IUnitOfWork unitOfWork)
    {
        _deliveryRepository = deliveryRepository;
        _emailService = emailService;
        _webSocketService = webSocketService;
        _userContextService = userContextService;
        _unitOfWork = unitOfWork;
    }

    public async Task DeliverAsync(Notification notification, NotificationChannel channel)
    {
        var delivery = new NotificationDelivery(notification.Id, channel);

        try
        {
            switch (channel.Value)
            {
                case "WEBSOCKET":
                    await _webSocketService.SendToUserAsync(notification.UserId, notification);
                    break;

                case "EMAIL":
                    var userName = await _userContextService.GetUserNameAsync(notification.UserId);
                    var userEmail = await _userContextService.GetUserEmailAsync(notification.UserId);
                    if (!string.IsNullOrEmpty(userEmail))
                        await _emailService.SendNotificationEmailAsync(userEmail, userName,
                            notification.Type, notification.Content.Title,
                            notification.Content.Message, notification.Content.ActionUrl);
                    break;

                case "IN_APP":
                    break;
            }

            delivery.MarkAsSent();
        }
        catch (Exception ex)
        {
            delivery.MarkAsFailed(ex.Message);
        }

        await _deliveryRepository.AddAsync(delivery);
        await _unitOfWork.CompleteAsync();
    }

    public async Task RetryFailedDeliveriesAsync()
    {
        var retryable = await _deliveryRepository.FindRetryableDeliveriesAsync();
        foreach (var delivery in retryable)
        {
            if (delivery.ShouldRetryNow())
            {
                delivery.MarkAsRetrying();
                _deliveryRepository.Update(delivery);
            }
        }
        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> CanDeliverAsync(Notification notification, NotificationChannel channel)
    {
        var existing = await _deliveryRepository.FindByNotificationIdAndChannelAsync(notification.Id, channel);
        return existing == null || !existing.IsSuccessful();
    }
}
