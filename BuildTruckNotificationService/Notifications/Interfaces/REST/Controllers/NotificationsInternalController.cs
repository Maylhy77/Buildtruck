using BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Controllers;

[ApiController]
[Route("internal/notifications")]
[Produces("application/json")]
public class NotificationsInternalController : ControllerBase
{
    private readonly INotificationFacade _notificationFacade;

    public NotificationsInternalController(INotificationFacade notificationFacade)
    {
        _notificationFacade = notificationFacade;
    }

    public record CreateInternalNotificationRequest(
        int UserId,
        string Type,
        string Context,
        string Title,
        string Message,
        string? Priority = null,
        string? ActionUrl = null,
        int? RelatedProjectId = null,
        int? RelatedEntityId = null,
        string? RelatedEntityType = null
    );

    [HttpPost]
    public async Task<ActionResult<int>> CreateNotification([FromBody] CreateInternalNotificationRequest req)
    {
        try
        {
            var id = await _notificationFacade.CreateNotificationAsync(
                req.UserId,
                NotificationType.FromString(req.Type),
                NotificationContext.FromString(req.Context),
                req.Title,
                req.Message,
                req.Priority != null ? NotificationPriority.FromString(req.Priority) : null,
                actionUrl: req.ActionUrl,
                relatedProjectId: req.RelatedProjectId,
                relatedEntityId: req.RelatedEntityId,
                relatedEntityType: req.RelatedEntityType
            );

            return Ok(id);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { message = "Error creating notification" }); }
    }

    public record CreateBulkNotificationRequest(
        List<int> UserIds,
        string Type,
        string Context,
        string Title,
        string Message,
        string? Priority = null,
        string? ActionUrl = null,
        int? RelatedProjectId = null
    );

    [HttpPost("bulk")]
    public async Task<ActionResult<int>> CreateBulkNotification([FromBody] CreateBulkNotificationRequest req)
    {
        try
        {
            var id = await _notificationFacade.CreateBulkNotificationAsync(
                req.UserIds,
                NotificationType.FromString(req.Type),
                NotificationContext.FromString(req.Context),
                req.Title,
                req.Message,
                req.Priority != null ? NotificationPriority.FromString(req.Priority) : null,
                actionUrl: req.ActionUrl,
                relatedProjectId: req.RelatedProjectId
            );

            return Ok(id);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { message = "Error creating bulk notification" }); }
    }
}
