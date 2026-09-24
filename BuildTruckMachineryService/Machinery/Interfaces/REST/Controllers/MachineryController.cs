using System.Net.Mime;
using BuildTruckMachineryService.Machinery.Domain.Model.Queries;
using BuildTruckMachineryService.Machinery.Domain.Services;
using BuildTruckMachineryService.Machinery.Interfaces.REST.Resources;
using BuildTruckMachineryService.Machinery.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BuildTruckMachineryService.Machinery.Domain.Model.Commands;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BuildTruckMachineryService.Machinery.Interfaces.REST.Controllers;

/// <summary>
/// Machinery Controller
/// </summary>
/// <remarks>
/// REST API controller for managing machinery in BuildTruck platform
/// Only admins can access these endpoints
/// </remarks>
[ApiController]
[Route("api/v1/machinery")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Machinery Management endpoints")]
[Authorize]
public class MachineryController(
    IMachineryCommandService machineryCommandService,
    IMachineryQueryService machineryQueryService,
    ICloudinaryImageService cloudinaryImageService) : ControllerBase
{
    private readonly CloudinarySettings _cloudinarySettings = new CloudinarySettings(); // Assuming configured via DI or appsettings

   /// <summary>
/// Create a new machinery
/// </summary>
/// <param name="createMachineryResource">The machinery creation data</param>
/// <returns>The created machinery</returns>
[HttpPost]
[Authorize(Roles = "Supervisor,Admin")]
[SwaggerOperation(
    Summary = "Create a new machinery",
    Description = "Creates a new machinery in the BuildTruck platform with optional image upload.",
    OperationId = "CreateMachinery")]
[SwaggerResponse(StatusCodes.Status201Created, "The machinery was created successfully", typeof(MachineryResource))]
[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid machinery data")]
[SwaggerResponse(StatusCodes.Status409Conflict, "Machinery already exists")]
public async Task<IActionResult> CreateMachinery([FromForm] CreateMachineryResource createMachineryResource)
{
    try
    {
        
        // ✅ Convert DTO to Command
         var createMachineryCommand = MachineryResourceAssembler.ToCommandFromResource(createMachineryResource);
        /*
         
          string imageUrl = string.Empty;
                 if (createMachineryResource.ImageFile != null)
                 {
                     using var stream = new MemoryStream();
                     await createMachineryResource.ImageFile.CopyToAsync(stream);
                     imageUrl = await cloudinaryImageService.UploadImageAsync(
                         stream.ToArray(),
                         createMachineryResource.ImageFile.FileName,
                         _cloudinarySettings.MachineryImagesFolder);
                 }
         */
       

        // ✅ Handle image upload to Cloudinary if provided
       

        // ✅ Execute business logic
        var machinery = await machineryCommandService.Handle(createMachineryCommand, createMachineryResource.ImageFile);

        // ✅ Convert Entity to DTO
        var machineryResource = MachineryResourceAssembler.ToResource(machinery);

        return CreatedAtAction(nameof(GetMachineryById), new { id = machinery.Id }, machineryResource);
    }
    catch (ArgumentException ex)
    {
        return BadRequest($"Invalid data: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        return Conflict($"Conflict: {ex.Message}");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

    /// <summary>
    /// Get machinery by ID
    /// </summary>
    /// <param name="id">The machinery ID</param>
    /// <returns>The machinery</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [SwaggerOperation(
        Summary = "Get machinery by ID",
        Description = "Retrieves a machinery by its ID",
        OperationId = "GetMachineryById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The machinery was found", typeof(MachineryResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Machinery not found")]
    public async Task<IActionResult> GetMachineryById(int id)
    {
        try
        {
            var getMachineryByIdQuery = new GetMachineryByIdQuery(id);
            var machinery = await machineryQueryService.Handle(getMachineryByIdQuery);

            if (machinery == null)
                return NotFound($"Machinery with ID {id} not found");

            var machineryResource = MachineryResourceAssembler.ToResource(machinery);
            return Ok(machineryResource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get machinery by project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <returns>List of machinery for the specified project</returns>
    [HttpGet("project/{projectId}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [SwaggerOperation(
        Summary = "Get machinery by project",
        Description = "Retrieves all machinery for a specific project",
        OperationId = "GetMachineryByProject")]
    [SwaggerResponse(StatusCodes.Status200OK, "Machinery retrieved successfully", typeof(IEnumerable<MachineryResource>))]
    public async Task<IActionResult> GetMachineryByProject(int projectId)
    {
        try
        {
            var query = new GetMachineryByProjectQuery(projectId);
            var machinery = await machineryQueryService.Handle(query);

            var machineryResources = machinery.Select(MachineryResourceAssembler.ToResource);
            return Ok(machineryResources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update machinery information
    /// </summary>
    /// <param name="id">The machinery ID</param>
    /// <param name="request">Update machinery request</param>
    /// <returns>Updated machinery</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Supervisor,Admin")]
    [SwaggerOperation(
        Summary = "Update machinery information",
        Description = "Update machinery's basic information.",
        OperationId = "UpdateMachinery")]
    [SwaggerResponse(StatusCodes.Status200OK, "Machinery updated successfully", typeof(MachineryResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data provided")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Machinery not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Machinery license plate already exists")]

    public async Task<IActionResult> UpdateMachinery(int id, [FromForm] UpdateMachineryResource request)
    {
        try
        {
            
            // ✅ Create command
            var updateCommand = request.ToCommandFromResource(id); // Fixed: Pass id parameter

            // ✅ Execute command
            var machinery = await machineryCommandService.Handle(updateCommand, request.ImageFile);

            // ✅ Return updated machinery
            var machineryResource = MachineryResourceAssembler.ToResource(machinery);
            return Ok(machineryResource);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound($"Machinery with ID {id} not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Operation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete machinery
    /// </summary>
    /// <param name="id">The machinery ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Supervisor,Admin")]
    [SwaggerOperation(
        Summary = "Delete machinery",
        Description = "Permanently delete a machinery from the system. This action cannot be undone.",
        OperationId = "DeleteMachinery")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Machinery deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Machinery not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    public async Task<IActionResult> DeleteMachinery(int id)
    {
        try
        {
            // ✅ Convert to Command
            var deleteMachineryCommand = new DeleteMachineryCommand(id);

            // ✅ Execute business logic
            await machineryCommandService.Handle(deleteMachineryCommand);

            return NoContent(); // 204 - Success with no content
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound($"Machinery with ID {id} not found");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

  
    
    
}