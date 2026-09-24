using System;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Aggregates
{
    public partial class MaterialUsage
    {
        public int Id { get; } // Solo lectura como en Projects
        public int ProjectId { get; private set; }
        public int MaterialId { get; private set; }

        public DateTime Date { get; private set; }
        public MaterialQuantity Quantity { get; private set; } = null!;
        public UsageType UsageType { get; private set; } = null!;
        public string Area { get; private set; } = string.Empty;
        public string Worker { get; private set; } = string.Empty;
        
        public string Observations { get; private set; } = string.Empty;

        // Constructor para EF Core
        protected MaterialUsage() 
        {
            Quantity = new MaterialQuantity(0);
            UsageType = UsageType.Construction;
            
        }

        public MaterialUsage(int projectId, int materialId, DateTime date,
            MaterialQuantity quantity, UsageType usageType,
            string area, string worker, string observations)
        {
            ProjectId = projectId > 0 ? projectId : throw new ArgumentException("ProjectId must be greater than 0", nameof(projectId));
            MaterialId = materialId > 0 ? materialId : throw new ArgumentException("MaterialId must be greater than 0", nameof(materialId));
            Date = date;
            Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            UsageType = usageType ?? throw new ArgumentNullException(nameof(usageType));
            Area = area ?? string.Empty;
            Worker = worker ?? string.Empty;
            Observations = observations ?? string.Empty;
            
        }

       

        public void UpdateDetails(DateTime date, MaterialQuantity quantity, UsageType usageType,
            string area, string worker, string observations)
        {
            Date = date;
            Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            UsageType = usageType ?? throw new ArgumentNullException(nameof(usageType));
            Area = area ?? string.Empty;
            Worker = worker ?? string.Empty;
            Observations = observations ?? string.Empty;
        }
        

        
    }
}