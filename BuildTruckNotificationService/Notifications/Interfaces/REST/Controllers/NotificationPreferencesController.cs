using System.Security.Claims;
using BuildTruckNotificationService.Notifications.Application.Internal.OutboundServices;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Resources;
using BuildTruckNotificationService.Notifications.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckNotificationService.Notifications.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/notification-preferences")]
[Authorize]
[Produces("application/json")]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationFacade _notificationFacade;

    public NotificationPreferencesController(INotificationFacade notificationFacade)
    {
        _notificationFacade = notificationFacade;
    }

    private int GetCurrentUserId()
    {
        var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationPreferenceResource>>> GetPreferences()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        try
        {
            var preferences = await _notificationFacade.GetUserPreferencesAsync(userId);

            if (!preferences.Any())
            {
                await _notificationFacade.CreateDefaultPreferencesAsync(userId);
                preferences = await _notificationFacade.GetUserPreferencesAsync(userId);
            }

            return Ok(NotificationPreferenceResourceAssembler.ToResourceFromEntity(preferences));
        }
        catch (Exception) { return StatusCode(500, new { message = "Error retrieving notification preferences" }); }
    }

    [HttpPut]
    public async Task<ActionResult> UpdatePreferences([FromBody] List<UpdatePreferenceResource> resources)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();
        if (!resources.Any()) return BadRequest(new { message = "At least one preference must be provided" });

        try
        {
            foreach (var resource in resources)
            {
                var command = NotificationPreferenceResourceAssembler.ToCommandFromResource(userId, resource);
                await _notificationFacade.UpdatePreferenceAsync(
                    command.UserId, command.Context,
                    command.InAppEnabled, command.EmailEnabled, command.MinimumPriority);
            }

            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { message = "Error updating notification preferences" }); }
    }
}
