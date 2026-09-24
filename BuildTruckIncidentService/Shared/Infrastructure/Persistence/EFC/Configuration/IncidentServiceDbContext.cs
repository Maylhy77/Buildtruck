using BuildTruckIncidentService.Incidents.Domain.Aggregates;
using BuildTruckIncidentService.Incidents.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class IncidentServiceDbContext(DbContextOptions<IncidentServiceDbContext> options) : DbContext(options)
{
    public DbSet<Incident> Incidents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new IncidentConfiguration());
    }
}
