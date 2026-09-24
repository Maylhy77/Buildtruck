using System;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for handling material entry commands
    /// </summary>
    public interface IMaterialEntryCommandService
    {
        /// <summary>
        /// Handles the creation of a new material entry
        /// </summary>
        /// <param name="command">Material entry creation data</param>
        /// <returns>Created entry or null if failed</returns>
        Task<MaterialEntry?> Handle(CreateMaterialEntryCommand command);

        /// <summary>
        /// Handles the update of an existing material entry
        /// </summary>
        /// <param name="command">Material entry update data</param>
        /// <returns>Updated entry or null if not found</returns>
        Task<MaterialEntry?> Handle(UpdateMaterialEntryCommand command);

        /// <summary>
        /// Handles the deletion of a material entry
        /// </summary>
        /// <param name="command">Material entry deletion data</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> Handle(DeleteMaterialEntryCommand command);
    }
}