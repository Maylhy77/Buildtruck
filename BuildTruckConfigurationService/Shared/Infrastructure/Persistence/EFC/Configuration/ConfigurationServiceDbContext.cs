using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckConfigurationService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class ConfigurationServiceDbContext(DbContextOptions<ConfigurationServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<ConfigurationSettings> ConfigurationSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ConfigurationSettingsConfiguration());
    }
}
