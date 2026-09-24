using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckProjectService.Projects.Domain.Model.Aggregates;

/// <summary>
/// Project partial class for timestamps
/// </summary>
/// <remarks>
/// Separates audit concerns like CreatedAt and UpdatedAt
/// </remarks>
public partial class Project : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] 
    public DateTimeOffset? CreatedDate { get; set; }
    
    [Column("UpdatedAt")] 
    public DateTimeOffset? UpdatedDate { get; set; }
}