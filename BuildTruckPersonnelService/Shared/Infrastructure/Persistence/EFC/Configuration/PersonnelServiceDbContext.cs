using BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class PersonnelServiceDbContext(DbContextOptions<PersonnelServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<PersonnelEntity> Personnel { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new PersonnelConfiguration());
    }
}
