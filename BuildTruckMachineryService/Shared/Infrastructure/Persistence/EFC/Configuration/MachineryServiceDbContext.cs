using BuildTruckMachineryService.Machinery.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

using MachineryDocument = BuildTruckMachineryService.Machinery.Domain.Model.Aggregates.Machinery;

namespace BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class MachineryServiceDbContext(DbContextOptions<MachineryServiceDbContext> options) : DbContext(options)
{
    public DbSet<MachineryDocument> Machinery { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new MachineryConfiguration());
    }
}
