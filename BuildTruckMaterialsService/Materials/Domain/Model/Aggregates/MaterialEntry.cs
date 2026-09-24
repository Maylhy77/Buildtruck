using System;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;

namespace BuildTruckMaterialsService.Materials.Domain.Model.Aggregates
{
    public partial class MaterialEntry
    {
        public int Id { get; } // Solo lectura como en Projects
        public int ProjectId { get; private set; }
        public int MaterialId { get; private set; }

        public DateTime Date { get; private set; }
        public MaterialQuantity Quantity { get; private set; } = null!;
        public MaterialUnit Unit { get; private set; } = null!;
        public MaterialCost UnitCost { get; private set; } = null!;

        public PaymentMethod Payment { get; private set; } = null!;
        public DocumentType DocumentType { get; private set; } = null!;
        public string DocumentNumber { get; private set; } = string.Empty;

        public string Provider { get; private set; } = string.Empty;
        public string Ruc { get; private set; } = string.Empty;

        public MaterialStatus Status { get; private set; } = null!;
        public string Observations { get; private set; } = string.Empty;

        // Constructor para EF Core
        protected MaterialEntry() 
        {
            Quantity = new MaterialQuantity(0);
            Unit = new MaterialUnit("UND");
            UnitCost = MaterialCost.InSoles(0);
            Payment = PaymentMethod.Cash;
            DocumentType = DocumentType.Receipt;
            Status = MaterialStatus.Pending;
        }

        public MaterialEntry(int projectId, int materialId, DateTime date,
            MaterialQuantity quantity, MaterialUnit unit, MaterialCost unitCost,
            PaymentMethod payment, DocumentType documentType, string documentNumber,
            string provider, string ruc, string observations)
        {
            ProjectId = projectId > 0 ? projectId : throw new ArgumentException("ProjectId must be greater than 0", nameof(projectId));
            MaterialId = materialId > 0 ? materialId : throw new ArgumentException("MaterialId must be greater than 0", nameof(materialId));
            Date = date;
            Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
            Payment = payment ?? throw new ArgumentNullException(nameof(payment));
            DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
            DocumentNumber = documentNumber ?? string.Empty;
            Provider = provider ?? string.Empty;
            Ruc = ruc ?? string.Empty;
            Observations = observations ?? string.Empty;
            Status = MaterialStatus.Pending;
        }

        public MaterialCost TotalCost => UnitCost.Multiply(Quantity);
        public void Confirm() => Status = MaterialStatus.Confirmed;
        public void Cancel() => Status = MaterialStatus.Cancelled;

        public void UpdateDetails(DateTime date, MaterialQuantity quantity, MaterialUnit unit, MaterialCost unitCost,
            PaymentMethod payment, DocumentType documentType, string documentNumber,
            string provider, string ruc, string observations)
        {
            Date = date;
            Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
            Payment = payment ?? throw new ArgumentNullException(nameof(payment));
            DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
            DocumentNumber = documentNumber ?? string.Empty;
            Provider = provider ?? string.Empty;
            Ruc = ruc ?? string.Empty;
            Observations = observations ?? string.Empty;
        }
       
        public void SetStatus(MaterialStatus status)
        {
            Status = status ?? throw new ArgumentNullException(nameof(status));
        }


        public void SetStatus(string status)
        {
            Status = new MaterialStatus(status);
        }
    }
}