using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Configuration;

public class MaterialUsageConfiguration : IEntityTypeConfiguration<MaterialUsage>
{
    public void Configure(EntityTypeBuilder<MaterialUsage> builder)
    {
        builder.ToTable("MaterialUsages");
        builder.HasKey(mu => mu.Id);

        builder.Property(mu => mu.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(mu => mu.ProjectId)
            .IsRequired();

        builder.Property(mu => mu.MaterialId)
            .IsRequired();

        builder.Property(mu => mu.Date)
            .IsRequired();

        builder.Property(mu => mu.Area)
            .HasMaxLength(100);

        builder.Property(mu => mu.Worker)
            .HasMaxLength(100);

        builder.Property(mu => mu.Observations)
            .HasMaxLength(500);

        builder.OwnsOne(mu => mu.Quantity, quantity =>
        {
            quantity.WithOwner().HasForeignKey("Id");
            quantity.Property(mq => mq.Value)
                .HasColumnName("Quantity")
                .IsRequired()
                .HasColumnType("decimal(10,2)");
        });

        builder.OwnsOne(mu => mu.UsageType, usageType =>
        {
            usageType.WithOwner().HasForeignKey("Id");
            usageType.Property(mut => mut.Value)
                .HasColumnName("UsageType")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(mu => mu.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialUsages_Materials_MaterialId");

        builder.HasIndex(mu => mu.ProjectId)
            .HasDatabaseName("IX_MaterialUsages_ProjectId");

        builder.HasIndex(mu => mu.MaterialId)
            .HasDatabaseName("IX_MaterialUsages_MaterialId");

        builder.HasIndex(mu => mu.Date)
            .HasDatabaseName("IX_MaterialUsages_Date");
    }
}
