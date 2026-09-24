using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Services;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin,Supervisor,Manager")]
    [SwaggerTag("Material Entries Management")]
    public class MaterialEntriesController : ControllerBase
    {
        private readonly IMaterialEntryCommandService _entryCommandService;
        private readonly IMaterialEntryQueryService _entryQueryService;

        public MaterialEntriesController(
            IMaterialEntryCommandService entryCommandService,
            IMaterialEntryQueryService entryQueryService)
        {
            _entryCommandService = entryCommandService;
            _entryQueryService = entryQueryService;
        }

        /// <summary>
        /// Endpoint 3: GET /api/material-entries/{projectId} - Listar entradas del proyecto
        /// </summary>
        [HttpGet("{projectId}")]
        [SwaggerOperation(Summary = "Get material entries by project", Description = "Retrieves all material entries for a specific project")]
        [SwaggerResponse(200, "Material entries retrieved successfully", typeof(IEnumerable<MaterialEntryResource>))]
        [SwaggerResponse(404, "Project not found")]
        public async Task<ActionResult<IEnumerable<MaterialEntryResource>>> GetMaterialEntriesByProject(int projectId)
        {
            try
            {
                var query = new GetMaterialEntriesByProjectQuery(projectId);
                var entries = await _entryQueryService.Handle(query);
                var resources = entries.Select(MaterialEntryResourceAssembler.ToResourceFromEntity);
                return Ok(resources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint 4: POST /api/material-entries - Crear o editar una entrada de material
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create or update material entry", Description = "Creates a new material entry or updates existing one if ID is provided")]
        [SwaggerResponse(200, "Material entry created/updated successfully", typeof(MaterialEntryResource))]
        [SwaggerResponse(400, "Invalid request data")]
        public async Task<ActionResult<MaterialEntryResource>> CreateOrUpdateMaterialEntry([FromBody] CreateOrUpdateMaterialEntryResource resource)
        {
            try
            {
                MaterialEntry? entry;

                if (resource.Id.HasValue && resource.Id.Value > 0)
                {
                    var updateCommand = MaterialEntryResourceAssembler.ToUpdateCommandFromResource(resource.Id.Value, resource);
                    entry = await _entryCommandService.Handle(updateCommand);

                    if (entry == null)
                        return NotFound("Material entry not found");
                }
                else
                {
                    var createCommand = MaterialEntryResourceAssembler.ToCreateCommandFromResource(resource);
                    entry = await _entryCommandService.Handle(createCommand);

                    if (entry == null)
                        return BadRequest("Failed to create material entry");
                }

                var entryResource = MaterialEntryResourceAssembler.ToResourceFromEntity(entry);
                return Ok(entryResource);
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
    }
}
