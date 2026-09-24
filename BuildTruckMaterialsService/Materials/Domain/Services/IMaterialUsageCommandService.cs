using System;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;

namespace BuildTruckMaterialsService.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for handling material usage commands
    /// </summary>
    public interface IMaterialUsageCommandService
    {
        /// <summary>
        /// Handles the creation of a new material usage record
        /// </summary>
        /// <param name="command">Material usage creation data</param>
        /// <returns>Created usage or null if failed</returns>
        Task<MaterialUsage?> Handle(CreateMaterialUsageCommand command);

        /// <summary>
        /// Handles the update of an existing material usage record
        /// </summary>
        /// <param name="command">Material usage update data</param>
        /// <returns>Updated usage or null if not found</returns>
        Task<MaterialUsage?> Handle(UpdateMaterialUsageCommand command);

        /// <summary>
        /// Handles the deletion of a material usage record
        /// </summary>
        /// <param name="command">Material usage deletion data</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> Handle(DeleteMaterialUsageCommand command);
    }
}