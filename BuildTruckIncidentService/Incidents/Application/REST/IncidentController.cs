using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BuildTruckIncidentService.Incidents.Application.Internal;
using BuildTruckIncidentService.Incidents.Application.REST.Resources;
using BuildTruckIncidentService.Incidents.Application.REST.Transform;
using System.IO;
using System.Linq;
using BuildTruckIncidentService.Incidents.Domain.Model.Commands;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckIncidentService.Incidents.Application.REST
{
    /// <summary>
    /// Incident Controller
    /// </summary>
    /// <remarks>
    /// REST API controller for managing incidents in BuildTruck platform
    /// </remarks>
    [ApiController]
    [Route("api/v1/incidents")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerTag("Available Incident Management endpoints")]
    [Authorize]
    public class IncidentController : ControllerBase
    {
        private readonly IIncidentFacade _incidentFacade;

        public IncidentController(IIncidentFacade incidentFacade)
        {
            _incidentFacade = incidentFacade;
        }

        /// <summary>
        /// Get incidents by project ID
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <returns>List of incidents for the specified project</returns>
        [HttpGet("project/{projectId}")]
        [Authorize(Roles = "Manager,Supervisor,Admin")] // ✅ Agregar Admin
        [SwaggerOperation(
            Summary = "Get incidents by project",
            Description = "Retrieves all incidents for a specific project",
            OperationId = "GetIncidentsByProject")]
        [SwaggerResponse(StatusCodes.Status200OK, "Incidents retrieved successfully", typeof(IEnumerable<IncidentResource>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Project not found")]
        public async Task<IActionResult> GetIncidentsByProjectId(int projectId)
        {
            try
            {
                var incidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                var resources = incidents.Select(IncidentResourceAssembler.ToResource);
                return Ok(resources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get incident by ID
        /// </summary>
        /// <param name="id">The incident ID</param>
        /// <returns>The incident</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager,Supervisor,Admin")] // ✅ Agregar Admin
        [SwaggerOperation(
            Summary = "Get incident by ID",
            Description = "Retrieves an incident by its ID",
            OperationId = "GetIncidentById")]
        [SwaggerResponse(StatusCodes.Status200OK, "Incident found", typeof(IncidentResource))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
        public async Task<IActionResult> GetIncidentById(int id)
        {
            try
            {
                var incident = await _incidentFacade.GetIncidentByIdAsync(id);
                if (incident == null) 
                    return NotFound($"Incident with ID {id} not found");
                
                return Ok(IncidentResourceAssembler.ToResource(incident));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new incident
        /// </summary>
        /// <param name="dto">The incident creation data</param>
        /// <returns>The created incident</returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Supervisor,Admin")] // ✅ Permitir a Manager crear también
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Create a new incident",
            Description = "Creates a new incident in the BuildTruck platform with optional image upload",
            OperationId = "CreateIncident")]
        [SwaggerResponse(StatusCodes.Status201Created, "Incident created successfully", typeof(IncidentResource))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid incident data")]
        public async Task<IActionResult> CreateIncident([FromForm] CreateIncidentDto dto)
        {
            try
            {
                string? tempFilePath = null;
                string? imageName = null;
                
                // ✅ Manejar imagen de forma más robusta
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var ext = Path.GetExtension(dto.Image.FileName);
                    if (string.IsNullOrEmpty(ext))
                    {
                        return BadRequest("Invalid image file format");
                    }
                    
                    tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
                    using (var stream = System.IO.File.Create(tempFilePath))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    imageName = dto.Image.FileName;
                }

                var command = new CreateIncidentCommand(
                    dto.ProjectId,
                    dto.Title,
                    dto.Description,
                    dto.IncidentType,
                    dto.Severity,
                    dto.Status,
                    dto.Location,
                    dto.ReportedBy,
                    dto.AssignedTo,
                    dto.OccurredAt,
                    imageName,      // Nombre del archivo
                    dto.Notes,
                    tempFilePath    // Ruta temporal
                );
                
                var incidentId = await _incidentFacade.CreateIncidentAsync(command);

                // ✅ Limpiar archivo temporal
                if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                // ✅ Devolver el incident creado, no solo el ID
                var createdIncident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
                if (createdIncident != null)
                {
                    var resource = IncidentResourceAssembler.ToResource(createdIncident);
                    return CreatedAtAction(nameof(GetIncidentById), new { id = incidentId }, resource);
                }

                return CreatedAtAction(nameof(GetIncidentById), new { id = incidentId }, new { id = incidentId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update incident information
        /// </summary>
        /// <param name="id">The incident ID</param>
        /// <param name="dto">Update incident data</param>
        /// <returns>Updated incident</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Supervisor,Admin")] // ✅ Permitir a Manager editar también
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Update incident information",
            Description = "Update incident's information with optional image upload",
            OperationId = "UpdateIncident")]
        [SwaggerResponse(StatusCodes.Status200OK, "Incident updated successfully", typeof(IncidentResource))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data provided")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
        public async Task<IActionResult> UpdateIncident(int id, [FromForm] UpdateIncidentDto dto)
        {
            try
            {

                string? tempFilePath = null;
                string? imageName = null;
                
                // ✅ Manejar imagen de forma más robusta
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var ext = Path.GetExtension(dto.Image.FileName);
                    if (string.IsNullOrEmpty(ext))
                    {
                        return BadRequest("Invalid image file format");
                    }
                    
                    tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
                    using (var stream = System.IO.File.Create(tempFilePath))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    imageName = dto.Image.FileName;
                }

                var command = new UpdateIncidentCommand(
                    id,
                    dto.ProjectId,
                    dto.Title,
                    dto.Description,
                    dto.IncidentType,
                    dto.Severity,
                    dto.Status,
                    dto.Location,
                    dto.ReportedBy,
                    dto.AssignedTo,
                    dto.OccurredAt,
                    dto.ResolvedAt,
                    imageName,
                    dto.Notes,
                    tempFilePath
                );
                
                await _incidentFacade.UpdateIncidentAsync(command);

                // ✅ Limpiar archivo temporal
                if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                // ✅ Devolver el incident actualizado
                var updatedIncident = await _incidentFacade.GetIncidentByIdAsync(id);
                if (updatedIncident != null)
                {
                    var resource = IncidentResourceAssembler.ToResource(updatedIncident);
                    return Ok(resource);
                }

                return Ok(new { message = "Incident updated successfully" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound($"Incident with ID {id} not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete incident
        /// </summary>
        /// <param name="id">The incident ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Supervisor,Admin")] // ✅ Solo Supervisor y Admin pueden eliminar
        [SwaggerOperation(
            Summary = "Delete incident",
            Description = "Permanently delete an incident from the system. This action cannot be undone.",
            OperationId = "DeleteIncident")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Incident deleted successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Incident not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            try
            {
                await _incidentFacade.DeleteIncidentAsync(id);
                return NoContent(); // 204 - Success with no content
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound($"Incident with ID {id} not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}