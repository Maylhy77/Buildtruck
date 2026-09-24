using System.Security.Claims;
using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationFacade _notificationFacade;

    public NotificationsController(INotificationFacade notificationFacade)
    {
        _notificationFacade = notificationFacade;
    }

    private int GetCurrentUserId()
    {
        var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResource>>> GetNotifications(
        [FromQuery] int page = 1, [FromQuery] int size = 20,
        [FromQuery] bool? unread = null, [FromQuery] string? context = null,
        [FromQuery] int? projectId = null)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var contextFilter = !string.IsNullOrWhiteSpace(context)
                ? NotificationContext.FromString(context) : null;

            var notifications = await _notificationFacade.GetUserNotificationsAsync(
                userId, page, size, unread, contextFilter, projectId);

            return Ok(NotificationResourceAssembler.ToResourceFromEntity(notifications));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { message = "Error retrieving notifications" }); }
    }

    [HttpPost("mark-read")]
    public async Task<ActionResult> MarkAsRead([FromBody] MarkAsReadResource resource)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();
        if (!resource.IsValid) return BadRequest(new { message = "Must provide NotificationId or NotificationIds" });

        try
        {
            if (resource.IsSingle && resource.NotificationId.HasValue)
                await _notificationFacade.MarkAsReadAsync(resource.NotificationId.Value, userId);
            else if (resource.IsBulk && resource.NotificationIds != null)
                await _notificationFacade.MarkAllAsReadAsync(resource.NotificationIds, userId);

            return Ok(new { message = "Notifications marked as read successfully" });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { message = "Error marking notifications as read" }); }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<NotificationSummaryResource>> GetSummary()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var summary = await _notificationFacade.GetNotificationSummaryAsync(userId);
            return Ok(NotificationResourceAssembler.ToSummaryResource(summary));
        }
        catch (Exception) { return StatusCode(500, new { message = "Error retrieving notification summary" }); }
    }

    [HttpDelete]
    public async Task<ActionResult> CleanOldNotifications([FromBody] CleanNotificationsResource? resource = null)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var daysOld = resource?.DaysOld ?? 30;
            await _notificationFacade.CleanOldNotificationsAsync(userId, daysOld);
            return Ok(new { message = $"Old notifications (older than {daysOld} days) cleaned successfully" });
        }
        catch (Exception) { return StatusCode(500, new { message = "Error cleaning old notifications" }); }
    }
}
