using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckMaterialsService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class MaterialsServiceDbContext(DbContextOptions<MaterialsServiceDbContext> options) : DbContext(options)
{
    public DbSet<Material> Materials { get; set; }
    public DbSet<MaterialEntry> MaterialEntries { get; set; }
    public DbSet<MaterialUsage> MaterialUsages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new MaterialConfiguration());
        builder.ApplyConfiguration(new MaterialEntryConfiguration());
        builder.ApplyConfiguration(new MaterialUsageConfiguration());
    }
}
