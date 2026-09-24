namespace BuildTruckStatsService.Shared.Infrastructure.Persistence.EFC.Configuration;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

public class StatsServiceDbContext(DbContextOptions<StatsServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<ManagerStats> ManagerStats { get; set; }
    public DbSet<StatsHistory> StatsHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ManagerStatsConfiguration());
        builder.ApplyConfiguration(new StatsHistoryConfiguration());
    }
}
