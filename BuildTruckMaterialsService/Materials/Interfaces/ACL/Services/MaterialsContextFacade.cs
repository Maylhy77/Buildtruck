using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Application.Internal.OutboundServices;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Interfaces.ACL.Services
{
    /// <summary>
    /// Implementation of Materials context facade for external bounded contexts
    /// </summary>
    public class MaterialsContextFacade : IMaterialsContextFacade
    {
        private readonly IMaterialFacade _materialFacade;
        private readonly IMaterialQueryService _materialQueryService;

        public MaterialsContextFacade(
            IMaterialFacade materialFacade,
            IMaterialQueryService materialQueryService)
        {
            _materialFacade = materialFacade;
            _materialQueryService = materialQueryService;
        }

        public async Task<decimal> GetProjectMaterialCostAsync(int projectId)
        {
            return await _materialFacade.GetProjectMaterialCostAsync(projectId);
        }

        public async Task<decimal> GetMaterialCurrentStockAsync(int materialId)
        {
            return await _materialFacade.GetMaterialStockAsync(materialId);
        }

        public async Task<bool> ValidateMaterialExistsInProjectAsync(int materialId, int projectId)
        {
            return await _materialFacade.ValidateMaterialExistsInProjectAsync(materialId, projectId);
        }

        public async Task<Dictionary<int, string>> GetProjectMaterialsLookupAsync(int projectId)
        {
            var query = new GetMaterialsByProjectQuery(projectId);
            var materials = await _materialQueryService.Handle(query);
            
            return materials.ToDictionary(
                m => m.Id,
                m => m.Name.Value
            );
        }

        public async Task<bool> ProjectHasMaterialsAsync(int projectId)
        {
            var query = new GetMaterialsByProjectQuery(projectId);
            var materials = await _materialQueryService.Handle(query);
            
            return materials.Any();
        }
    }
}