using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;

public partial class Documentation : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}