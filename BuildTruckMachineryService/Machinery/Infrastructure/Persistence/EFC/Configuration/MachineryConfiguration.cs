using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MachineryEntity = BuildTruckMachineryService.Machinery.Domain.Model.Aggregates.Machinery;

namespace BuildTruckMachineryService.Machinery.Infrastructure.Persistence.EFC.Configuration;

public class MachineryConfiguration : IEntityTypeConfiguration<MachineryEntity>
{
    public void Configure(EntityTypeBuilder<MachineryEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ProjectId).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LicensePlate).IsRequired().HasMaxLength(20);
        builder.Property(m => m.MachineryType).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Provider).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.PersonnelId);
        builder.Property(m => m.ImageUrl).HasMaxLength(500);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();
        builder.Property(m => m.RegisterDate).IsRequired();

        builder.HasIndex(m => new { m.ProjectId, m.LicensePlate }).IsUnique();
        builder.HasIndex(m => m.Status).HasDatabaseName("IX_Machinery_Status");
    }
}