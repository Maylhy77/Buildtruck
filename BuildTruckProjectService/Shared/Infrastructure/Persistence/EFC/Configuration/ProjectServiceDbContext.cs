using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckProjectService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class ProjectServiceDbContext(DbContextOptions<ProjectServiceDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>().HasKey(p => p.Id);
        builder.Entity<Project>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();

        builder.Entity<Project>().OwnsOne(p => p.Name, n =>
        {
            n.WithOwner().HasForeignKey("Id");
            n.Property(pn => pn.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
        });

        builder.Entity<Project>().OwnsOne(p => p.Description, d =>
        {
            d.WithOwner().HasForeignKey("Id");
            d.Property(pd => pd.Description).HasColumnName("Description").IsRequired().HasMaxLength(1000);
        });

        builder.Entity<Project>().OwnsOne(p => p.Location, l =>
        {
            l.WithOwner().HasForeignKey("Id");
            l.Property(pl => pl.Location).HasColumnName("Location").IsRequired().HasMaxLength(200);
        });

        builder.Entity<Project>().OwnsOne(p => p.State, s =>
        {
            s.WithOwner().HasForeignKey("Id");
            s.Property(ps => ps.State).HasColumnName("State").IsRequired().HasMaxLength(20);
        });

        builder.Entity<Project>().OwnsOne(p => p.Coordinates, c =>
        {
            c.WithOwner().HasForeignKey("Id");
            c.Property(pc => pc.Latitude).HasColumnName("Latitude").HasPrecision(10, 8);
            c.Property(pc => pc.Longitude).HasColumnName("Longitude").HasPrecision(11, 8);
        });

        builder.Entity<Project>().Property(p => p.ManagerId).IsRequired();
        builder.Entity<Project>().Property(p => p.SupervisorId);
        builder.Entity<Project>().Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Entity<Project>().Property(p => p.StartDate);

        builder.Entity<Project>()
            .HasIndex(p => p.ManagerId)
            .HasDatabaseName("IX_Projects_ManagerId");

        builder.Entity<Project>()
            .HasIndex(p => p.SupervisorId)
            .HasDatabaseName("IX_Projects_SupervisorId");

        builder.Entity<Project>()
            .HasIndex(p => p.SupervisorId)
            .HasDatabaseName("IX_Projects_SupervisorId_Business")
            .HasFilter("supervisor_id IS NOT NULL");
    }
}
