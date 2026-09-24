using System;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Aggregates
{
    public partial class Material
    {
        public int Id { get; } // Solo lectura como en Projects
        public int ProjectId { get; private set; }

        public MaterialName Name { get; private set; } = null!;
        public MaterialType Type { get; private set; } = null!;
        public MaterialUnit Unit { get; private set; } = null!;
        public MaterialQuantity MinimumStock { get; private set; } = null!;
        public string Provider { get; private set; } = string.Empty;

        public MaterialCost Price { get; private set; } = null!;
        public MaterialQuantity Stock { get; private set; } = null!;

        // Constructor para EF Core
        protected Material() 
        {
            Name = new MaterialName("Default");
            Type = new MaterialType("OTRO");
            Unit = new MaterialUnit("UND");
            MinimumStock = new MaterialQuantity(0);
            Price = MaterialCost.InSoles(0);
            Stock = new MaterialQuantity(0);
        }

        public Material(int projectId, MaterialName name, MaterialType type, MaterialUnit unit,
            MaterialQuantity minimumStock, string provider)
        {
            ProjectId = projectId > 0 ? projectId : throw new ArgumentException("ProjectId must be greater than 0", nameof(projectId));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            MinimumStock = minimumStock ?? throw new ArgumentNullException(nameof(minimumStock));
            Provider = provider ?? string.Empty;

            Price = MaterialCost.InSoles(0);
            Stock = new MaterialQuantity(0);
        }

        public void UpdatePrice(MaterialCost price) => Price = price ?? throw new ArgumentNullException(nameof(price));
        public void AddStock(MaterialQuantity quantity) => Stock = Stock.Add(quantity);
        public void RemoveStock(MaterialQuantity quantity) => Stock = Stock.Subtract(quantity);
        public void UpdateStock(MaterialQuantity quantity) => Stock = quantity ?? throw new ArgumentNullException(nameof(quantity));

        public void UpdateBasicInfo(MaterialName name, MaterialType type, MaterialUnit unit,
            MaterialQuantity minimumStock, string provider)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            MinimumStock = minimumStock ?? throw new ArgumentNullException(nameof(minimumStock));
            Provider = provider ?? string.Empty;
        }
    }
}