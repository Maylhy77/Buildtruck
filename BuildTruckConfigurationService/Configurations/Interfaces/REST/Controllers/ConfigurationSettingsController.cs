using Microsoft.AspNetCore.Mvc;
using BuildTruckConfigurationService.Configurations.Application.ACL;
using BuildTruckConfigurationService.Configurations.Interfaces.ACL;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckConfigurationService.Configurations.Interfaces.REST.Controllers;

/// <summary>
/// Controller for ConfigurationSettings operations
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
[ApiController]
[Route("api/v1/configuration-settings")]
[Produces("application/json")]
public class ConfigurationSettingsController : ControllerBase
{
    private readonly IConfigurationSettingsFacade _facade;

    public ConfigurationSettingsController(IConfigurationSettingsFacade facade)
    {
        _facade = facade;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new configuration settings",
        Description = "Creates a new configuration settings for a user.",
        OperationId = "CreateConfigurationSettings",
        Tags = new[] { "Configurations" })]
    [ProducesResponseType(typeof(ConfigurationSettingsResource), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationSettingsResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Validate the resource first
            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            var command = CreateConfigurationSettingsCommandFromResourceAssembler.ToCommandFromResource(resource);
            var configurationSettings = await _facade.CreateConfigurationSettingsAsync(command);
            var configurationSettingsResource = ConfigurationSettingsResourceFromEntityAssembler.ToResourceFromEntity(configurationSettings);
            return StatusCode(201, configurationSettingsResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update a configuration settings",
        Description = "Updates an existing configuration settings for a user.",
        OperationId = "UpdateConfigurationSettings",
        Tags = new[] { "Configurations" })]
    [ProducesResponseType(typeof(ConfigurationSettingsResource), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConfigurationSettingsResource resource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var command = UpdateConfigurationSettingsCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var configurationSettings = await _facade.UpdateConfigurationSettingsAsync(command);
            var configurationSettingsResource = ConfigurationSettingsResourceFromEntityAssembler.ToResourceFromEntity(configurationSettings);
            return Ok(configurationSettingsResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [SwaggerOperation(
        Summary = "Get configuration settings by user ID",
        Description = "Retrieves the configuration settings for a specific user.",
        OperationId = "GetConfigurationSettingsByUserId",
        Tags = new[] { "Configurations" })]
    [ProducesResponseType(typeof(ConfigurationSettingsResource), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        try
        {
            var configurationSettings = await _facade.GetConfigurationSettingsByUserIdAsync(userId);
            if (configurationSettings == null)
                return NotFound(new { error = $"Configuration settings for UserId {userId} not found." });

            var configurationSettingsResource = ConfigurationSettingsResourceFromEntityAssembler.ToResourceFromEntity(configurationSettings);
            return Ok(configurationSettingsResource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
}
