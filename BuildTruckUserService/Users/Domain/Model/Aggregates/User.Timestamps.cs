using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckUserService.Users.Domain.Model.Aggregates;

/// <summary>
/// User partial class for timestamps
/// </summary>
/// <remarks>
/// Separates audit concerns like CreatedAt and UpdatedAt
/// </remarks>
public partial class User : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] 
    public DateTimeOffset? CreatedDate { get; set; }
    
    [Column("UpdatedAt")] 
    public DateTimeOffset? UpdatedDate { get; set; }
}