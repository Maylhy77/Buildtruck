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
    [SwaggerTag("Material Usage Management")]
    public class MaterialUsagesController : ControllerBase
    {
        private readonly IMaterialUsageCommandService _usageCommandService;
        private readonly IMaterialUsageQueryService _usageQueryService;

        public MaterialUsagesController(
            IMaterialUsageCommandService usageCommandService,
            IMaterialUsageQueryService usageQueryService)
        {
            _usageCommandService = usageCommandService;
            _usageQueryService = usageQueryService;
        }

        /// <summary>
        /// Endpoint 5: GET /api/material-usages/{projectId} - Listar usos registrados del proyecto
        /// </summary>
        [HttpGet("{projectId}")]
        [SwaggerOperation(Summary = "Get material usages by project", Description = "Retrieves all material usage records for a specific project")]
        [SwaggerResponse(200, "Material usages retrieved successfully", typeof(IEnumerable<MaterialUsageResource>))]
        [SwaggerResponse(404, "Project not found")]
        public async Task<ActionResult<IEnumerable<MaterialUsageResource>>> GetMaterialUsagesByProject(int projectId)
        {
            var query = new GetMaterialUsagesByProjectQuery(projectId);
            var usages = await _usageQueryService.Handle(query);
            
            var resources = usages.Select(MaterialUsageResourceAssembler.ToResourceFromEntity);
            return Ok(resources);
        }

        /// <summary>
        /// Endpoint 6: POST /api/material-usages - Crear o editar un uso de material
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create or update material usage", Description = "Creates a new material usage or updates existing one if ID is provided")]
        [SwaggerResponse(200, "Material usage created/updated successfully", typeof(MaterialUsageResource))]
        [SwaggerResponse(400, "Invalid request data")]
        public async Task<ActionResult<MaterialUsageResource>> CreateOrUpdateMaterialUsage([FromBody] CreateOrUpdateMaterialUsageResource resource)
        {
            MaterialUsage? usage;
            
            if (resource.Id.HasValue && resource.Id.Value > 0)
            {
                // Actualizar uso existente
                var updateCommand = MaterialUsageResourceAssembler.ToUpdateCommandFromResource(resource.Id.Value, resource);
                usage = await _usageCommandService.Handle(updateCommand);
                
                if (usage == null)
                    return NotFound("Material usage not found");
            }
            else
            {
                // Crear nuevo uso
                var createCommand = MaterialUsageResourceAssembler.ToCreateCommandFromResource(resource);
                usage = await _usageCommandService.Handle(createCommand);
                
                if (usage == null)
                    return BadRequest("Failed to create material usage");
            }
            
            var usageResource = MaterialUsageResourceAssembler.ToResourceFromEntity(usage);
            return Ok(usageResource);
        }
    }
}