using System;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for handling material commands
    /// </summary>
    public interface IMaterialCommandService
    {
        /// <summary>
        /// Handles the creation of a new material
        /// </summary>
        /// <param name="command">Material creation data</param>
        /// <returns>Created material or null if failed</returns>
        Task<Material?> Handle(CreateMaterialCommand command);

        /// <summary>
        /// Handles the update of an existing material
        /// </summary>
        /// <param name="command">Material update data</param>
        /// <returns>Updated material or null if not found</returns>
        Task<Material?> Handle(UpdateMaterialCommand command);

        /// <summary>
        /// Handles the deletion of a material
        /// </summary>
        /// <param name="command">Material deletion data</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> Handle(DeleteMaterialCommand command);
    }
}