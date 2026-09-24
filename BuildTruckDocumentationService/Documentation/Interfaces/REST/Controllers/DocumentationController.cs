using BuildTruckDocumentationService.Documentation.Application.ACL.Services;
using BuildTruckDocumentationService.Documentation.Domain.Model.Commands;
using BuildTruckDocumentationService.Documentation.Domain.Services;
using BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;
using BuildTruckDocumentationService.Documentation.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BuildTruckDocumentationService.Documentation.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/documentation")]
[Produces("application/json")]
public class DocumentationController : ControllerBase
{
    private readonly IDocumentationCommandService _documentationCommandService;
    private readonly IDocumentationQueryService _documentationQueryService;
    private readonly ICloudinaryService _cloudinaryService;

    public DocumentationController(
        IDocumentationCommandService documentationCommandService,
        IDocumentationQueryService documentationQueryService,
        ICloudinaryService cloudinaryService)
    {
        _documentationCommandService = documentationCommandService;
        _documentationQueryService = documentationQueryService;
        _cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// 1. POST /api/documentation - Crear Y actualizar (si incluye ID actualiza)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Supervisor,Manager")]
    [ProducesResponseType(typeof(DocumentationResource), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(DocumentationResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateOrUpdateDocumentation([FromForm] CreateOrUpdateDocumentationResource resource)
    {
        try
        {
            // Validate the resource first
            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Get current user ID from JWT
            var userIdClaim = User.FindFirst("user_id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user information");
            }

            string imagePath;

            if (resource.IsUpdate())
            {
                // UPDATE existing documentation
                var existingDoc = await _documentationQueryService.GetDocumentationByIdAsync(resource.Id!.Value);
                if (existingDoc == null)
                    return NotFound($"Documentation with ID {resource.Id} not found");

                // Handle image update
                if (resource.ImageFile != null)
                {
                    // Upload new image
                    using var memoryStream = new MemoryStream();
                    await resource.ImageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    
                    imagePath = await _cloudinaryService.UploadDocumentationImageAsync(
                        imageBytes, 
                        resource.ImageFile.FileName, 
                        resource.Id.Value);

                    // Delete old image
                    if (!string.IsNullOrEmpty(existingDoc.ImagePath))
                    {
                        _ = Task.Run(async () => await _cloudinaryService.DeleteDocumentationImageAsync(existingDoc.ImagePath));
                    }
                }
                else
                {
                    // Keep existing image
                    imagePath = existingDoc.ImagePath;
                }

                // Create command for update
                var updateCommand = CreateOrUpdateDocumentationCommandFromResourceAssembler
                    .ToCommandFromResourceForUpdate(resource, imagePath, currentUserId, existingDoc.ImagePath);
                
                var updatedDocumentation = await _documentationCommandService.Handle(updateCommand);
                if (updatedDocumentation == null)
                    return BadRequest("Failed to update documentation");

                var updatedResource = DocumentationResourceFromEntityAssembler.ToResourceFromEntity(updatedDocumentation);
                return Ok(updatedResource);
            }
            else
            {
                // CREATE new documentation
                if (resource.ImageFile == null)
                    return BadRequest("Image file is required for new documentation");

                // Upload image
                using var memoryStream = new MemoryStream();
                await resource.ImageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                // Generate temporary ID for filename (will be replaced after creation)
                var tempId = DateTime.UtcNow.Ticks;
                imagePath = await _cloudinaryService.UploadDocumentationImageAsync(
                    imageBytes, 
                    resource.ImageFile.FileName, 
                    (int)(tempId % int.MaxValue));

                // Create command
                var createCommand = CreateOrUpdateDocumentationCommandFromResourceAssembler
                    .ToCommandFromResource(resource, imagePath, currentUserId);
                
                var createdDocumentation = await _documentationCommandService.Handle(createCommand);
                if (createdDocumentation == null)
                    return BadRequest("Failed to create documentation");

                var createdResource = DocumentationResourceFromEntityAssembler.ToResourceFromEntity(createdDocumentation);
                return CreatedAtAction(nameof(GetDocumentationById), new { id = createdDocumentation.Id }, createdResource);
            }
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
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 2. GET /api/documentation?projectId=X - Listar por proyecto
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Supervisor,Manager")]
    [ProducesResponseType(typeof(IEnumerable<DocumentationResource>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetDocumentationByProject([FromQuery] int projectId)
    {
        try
        {
            if (projectId <= 0)
                return BadRequest("Invalid project ID");

            var documentation = await _documentationQueryService.GetDocumentationByProjectAsync(projectId);
            var resources = DocumentationResourceFromEntityAssembler.ToResourceFromEntity(documentation);
            
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 3. GET /api/documentation/{id} - Obtener específica
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor,Manager")]
    [ProducesResponseType(typeof(DocumentationResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetDocumentationById(int id)
    {
        try
        {
            var documentation = await _documentationQueryService.GetDocumentationByIdAsync(id);
            if (documentation == null)
                return NotFound($"Documentation with ID {id} not found");

            var resource = DocumentationResourceFromEntityAssembler.ToResourceFromEntity(documentation);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 4. DELETE /api/documentation/{id} - Eliminar (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Supervisor,Manager")]
    [ProducesResponseType(typeof(DeleteDocumentationResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DeleteDocumentation(int id)
    {
        try
        {
            var command = new DeleteDocumentationCommand(id);
            var success = await _documentationCommandService.Handle(command);

            if (success)
            {
                return Ok(new DeleteDocumentationResource(true, "Documentation deleted successfully"));
            }

            return NotFound(new DeleteDocumentationResource(false, $"Documentation with ID {id} not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DeleteDocumentationResource(false, $"Internal server error: {ex.Message}"));
        }
    }
}