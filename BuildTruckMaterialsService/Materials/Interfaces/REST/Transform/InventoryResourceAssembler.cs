using System.Collections.Generic;
using System.Linq;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Interfaces.REST.Resources;

namespace BuildTruckMaterialsService.Materials.Interfaces.REST.Transform
{
    /// <summary>
    /// Resource assembler for inventory operations
    /// Transforms between Material aggregates and InventoryItemResource DTOs
    /// </summary>
    public static class InventoryResourceAssembler
    {
        /// <summary>
        /// Converts a Material aggregate to an InventoryItemResource for API responses
        /// This is used specifically for inventory endpoint that shows current stock and pricing
        /// </summary>
        /// <param name="material">Material aggregate with updated stock and price information</param>
        /// <returns>InventoryItemResource with current inventory data</returns>
        public static InventoryItemResource ToResourceFromEntity(Material material)
        {
            return new InventoryItemResource(
                material.Id,
                material.Name.Value,
                material.Type.Value,
                material.Unit.Value,
                material.MinimumStock.Value,
                material.Provider,
                0, // TotalEntries - no disponible desde Material
                0, // TotalUsages - no disponible desde Material
                material.Stock.Value,
                material.Price.Value,
                material.Stock.Value * material.Price.Value
            );
        }

        /// <summary>
        /// Converts a list of Material aggregates to InventoryItemResource list
        /// </summary>
        /// <param name="materials">List of Material aggregates</param>
        /// <returns>List of InventoryItemResource for API response</returns>
        public static List<InventoryItemResource> ToResourceListFromEntityList(List<Material> materials)
        {
            return materials.Select(ToResourceFromEntity).ToList();
        }

        /// <summary>
        /// Alternative method name for consistency with other assemblers
        /// </summary>
        /// <param name="material">Material aggregate</param>
        /// <returns>InventoryItemResource</returns>
        public static InventoryItemResource ToInventoryResourceFromEntity(Material material)
        {
            return ToResourceFromEntity(material);
        }
    }
}