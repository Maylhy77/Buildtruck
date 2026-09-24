using BuildTruckPersonnelService.Personnel.Application.ACL.Services;
using BuildTruckPersonnelService.Personnel.Domain.Model.Commands;
using BuildTruckPersonnelService.Personnel.Domain.Services;
using BuildTruckPersonnelService.Personnel.Interfaces.REST.Resources;
using BuildTruckPersonnelService.Personnel.Interfaces.REST.Transform;
using BuildTruckShared.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelCommandService _personnelCommandService;
    private readonly IPersonnelQueryService _personnelQueryService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersonnelController> _logger;

    public PersonnelController(
        IPersonnelCommandService personnelCommandService,
        IPersonnelQueryService personnelQueryService,
        ICloudinaryService cloudinaryService,
        IUnitOfWork unitOfWork,
        ILogger<PersonnelController> logger)
    {
        _personnelCommandService = personnelCommandService;
        _personnelQueryService = personnelQueryService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> CreatePersonnel([FromForm] CreatePersonnelResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? []).Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
                return BadRequest(new { message = "Validation failed", errors = resource.GetValidationErrors() });

            var command = CreatePersonnelCommandFromResourceAssembler.ToCommandFromResource(resource);
            var personnel = await _personnelCommandService.Handle(command);

            if (personnel == null)
                return BadRequest("Failed to create personnel");

            if (resource.ImageFile != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await resource.ImageFile.CopyToAsync(memoryStream);
                    var imageUrl = await _cloudinaryService.UploadPersonnelPhotoAsync(
                        memoryStream.ToArray(), resource.ImageFile.FileName, personnel.Id);
                    personnel.UpdateAvatar(imageUrl);
                    await _unitOfWork.CompleteAsync();
                }
                catch (Exception imageEx)
                {
                    _logger.LogWarning(imageEx, "Failed to upload avatar for personnel {PersonnelId}", personnel.Id);
                }
            }

            var personnelResource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            return CreatedAtAction(nameof(GetPersonnelById), new { id = personnel.Id }, personnelResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error creating personnel");
            return StatusCode(500, "An internal error occurred while creating the personnel");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<PersonnelResource>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetPersonnelByProject(
        [FromQuery] int projectId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] bool includeAttendance = false)
    {
        try
        {
            if (projectId <= 0)
                return BadRequest("Invalid project ID");

            IEnumerable<PersonnelEntity> personnel;

            if (includeAttendance)
            {
                var currentDate = DateTime.Now;
                var targetYear = year ?? currentDate.Year;
                var targetMonth = month ?? currentDate.Month;

                if (targetYear < 2020 || targetYear > 2100)
                    return BadRequest("Invalid year. Must be between 2020 and 2100");

                if (targetMonth < 1 || targetMonth > 12)
                    return BadRequest("Invalid month. Must be between 1 and 12");

                personnel = await _personnelQueryService.GetPersonnelWithAttendanceAsync(
                    projectId, targetYear, targetMonth, true);
            }
            else
            {
                personnel = await _personnelQueryService.GetPersonnelByProjectAsync(projectId);
            }

            var personnelList = personnel.ToList();
            var resources = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnelList, includeAttendance);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel for project {ProjectId}", projectId);
            return StatusCode(500, "An internal error occurred while retrieving personnel");
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetPersonnelById(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            var personnel = await _personnelQueryService.GetPersonnelByIdAsync(id);
            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            return Ok(PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel {PersonnelId}", id);
            return StatusCode(500, "An internal error occurred while retrieving personnel");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> UpdatePersonnel(int id, [FromForm] UpdatePersonnelResource resource)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? []).Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
                return BadRequest(new { message = "Validation failed", errors = resource.GetValidationErrors() });

            if (!resource.HasChanges())
                return BadRequest("No changes specified in update request");

            var command = UpdatePersonnelCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var personnel = await _personnelCommandService.Handle(command);

            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            bool imageUpdated = false;

            if (resource.ImageFile != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await resource.ImageFile.CopyToAsync(memoryStream);

                    var newImageUrl = await _cloudinaryService.UploadPersonnelPhotoAsync(
                        memoryStream.ToArray(), resource.ImageFile.FileName, personnel.Id);

                    if (!string.IsNullOrEmpty(personnel.AvatarUrl))
                    {
                        try { await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl); }
                        catch (Exception deleteEx) { _logger.LogWarning(deleteEx, "Failed to delete old avatar for {PersonnelId}", personnel.Id); }
                    }

                    personnel.UpdateAvatar(newImageUrl);
                    imageUpdated = true;
                }
                catch (Exception imageEx)
                {
                    _logger.LogWarning(imageEx, "Failed to update avatar for personnel {PersonnelId}", personnel.Id);
                }
            }
            else if (resource.RemoveImage && !string.IsNullOrEmpty(personnel.AvatarUrl))
            {
                try
                {
                    await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl);
                    personnel.UpdateAvatar(null);
                    imageUpdated = true;
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to remove avatar for personnel {PersonnelId}", personnel.Id);
                }
            }

            if (imageUpdated)
                await _unitOfWork.CompleteAsync();

            return Ok(PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error updating personnel {PersonnelId}", id);
            return StatusCode(500, "An internal error occurred while updating the personnel");
        }
    }

    [HttpPut("attendance")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateAttendance([FromBody] UpdateAttendanceResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? []).Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
                return BadRequest(new { message = "Validation failed", errors = resource.GetValidationErrors() });

            var command = AttendanceResourceFromEntityAssembler.ToCommandFromResource(resource);
            var success = await _personnelCommandService.Handle(command);

            if (success)
                return Ok(new
                {
                    success = true,
                    message = "Attendance updated successfully",
                    updatedPersonnel = resource.PersonnelAttendances.Count,
                    updatedAt = DateTimeOffset.UtcNow
                });

            return BadRequest("Failed to update attendance");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error updating attendance");
            return StatusCode(500, "An internal error occurred while updating attendance");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(DeletePersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            var personnel = await _personnelQueryService.GetPersonnelByIdAsync(id);
            if (personnel == null)
                return NotFound(new DeletePersonnelResource(false, $"Personnel with ID {id} not found"));

            var command = new DeletePersonnelCommand(id);
            var success = await _personnelCommandService.Handle(command);

            if (success)
            {
                if (!string.IsNullOrEmpty(personnel.AvatarUrl))
                {
                    try { await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl); }
                    catch (Exception imageEx) { _logger.LogWarning(imageEx, "Failed to delete avatar for {PersonnelId}", id); }
                }

                return Ok(new DeletePersonnelResource(true, "Personnel deleted successfully"));
            }

            return BadRequest(new DeletePersonnelResource(false, "Failed to delete personnel"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error deleting personnel {PersonnelId}", id);
            return StatusCode(500, new DeletePersonnelResource(false, $"Internal server error: {ex.Message}"));
        }
    }
}
