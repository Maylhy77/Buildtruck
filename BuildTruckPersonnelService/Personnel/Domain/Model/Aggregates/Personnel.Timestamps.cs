using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;

public partial class Personnel : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
