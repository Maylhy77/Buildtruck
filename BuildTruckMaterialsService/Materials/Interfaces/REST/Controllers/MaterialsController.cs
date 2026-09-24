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
    [SwaggerTag("Materials Management")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialCommandService _materialCommandService;
        private readonly IMaterialQueryService _materialQueryService;
        private readonly IInventoryQueryService _inventoryQueryService;

        public MaterialsController(
            IMaterialCommandService materialCommandService,
            IMaterialQueryService materialQueryService,
            IInventoryQueryService inventoryQueryService)
        {
            _materialCommandService = materialCommandService;
            _materialQueryService = materialQueryService;
            _inventoryQueryService = inventoryQueryService;
        }

        /// <summary>
        /// Endpoint 1: GET /api/materials/project/{projectId} - Listar materiales de un proyecto
        /// </summary>
        [HttpGet("project/{projectId}")]
        [SwaggerOperation(Summary = "Get materials by project", Description = "Retrieves all materials for a specific project")]
        [SwaggerResponse(200, "Materials retrieved successfully", typeof(IEnumerable<MaterialResource>))]
        [SwaggerResponse(404, "Project not found")]
        public async Task<ActionResult<IEnumerable<MaterialResource>>> GetMaterialsByProject(int projectId)
        {
            var query = new GetMaterialsByProjectQuery(projectId);
            var materials = await _materialQueryService.Handle(query);
            
            var resources = materials.Select(MaterialResourceAssembler.ToResourceFromEntity);
            return Ok(resources);
        }

        /// <summary>
        /// Endpoint 2: POST /api/materials - Crear o editar un material base
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create or update material", Description = "Creates a new material or updates existing one if ID is provided")]
        [SwaggerResponse(200, "Material created/updated successfully", typeof(MaterialResource))]
        [SwaggerResponse(400, "Invalid request data")]
        public async Task<ActionResult<MaterialResource>> CreateOrUpdateMaterial([FromBody] CreateOrUpdateMaterialResource resource)
        {
            Material? material;
            
            if (resource.Id.HasValue && resource.Id.Value > 0)
            {
                // Actualizar material existente
                var updateCommand = MaterialResourceAssembler.ToUpdateCommandFromResource(resource.Id.Value, resource);
                material = await _materialCommandService.Handle(updateCommand);
                
                if (material == null)
                    return NotFound("Material not found");
            }
            else
            {
                // Crear nuevo material
                var createCommand = MaterialResourceAssembler.ToCreateCommandFromResource(resource);
                material = await _materialCommandService.Handle(createCommand);
                
                if (material == null)
                    return BadRequest("Failed to create material");
            }
            
            var materialResource = MaterialResourceAssembler.ToResourceFromEntity(material);
            return Ok(materialResource);
        }

        /// <summary>
        /// Endpoint 7: GET /api/materials/inventory/{projectId} - Obtener resumen dinámico del inventario
        /// </summary>
        [HttpGet("inventory/{projectId}")]
        [SwaggerOperation(Summary = "Get inventory summary", Description = "Retrieves dynamic inventory summary with current stock and prices for a project")]
        [SwaggerResponse(200, "Inventory retrieved successfully", typeof(IEnumerable<InventoryItemResource>))]
        [SwaggerResponse(404, "Project not found")]
        public async Task<ActionResult<IEnumerable<InventoryItemResource>>> GetInventoryByProject(int projectId)
        {
            var query = new GetInventoryByProjectQuery(projectId);
            var inventoryItems = await _inventoryQueryService.HandleDetailed(query);  // ✅ Cambio aquí
    
            return Ok(inventoryItems);  // ✅ Retornar directamente los recursos
        }
    }
}
