using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Queries;

namespace BuildTruckNotificationService.Notifications.Domain.Services;

public interface INotificationQueryService
{
    Task<Notification?> Handle(GetNotificationByIdQuery query);
    Task<IEnumerable<Notification>> Handle(GetNotificationsByUserQuery query);
    Task<Dictionary<string, object>> Handle(GetNotificationSummaryQuery query);
}
