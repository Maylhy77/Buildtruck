using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Configuration;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(m => m.ProjectId)
            .IsRequired();

        builder.Property(m => m.Provider)
            .HasMaxLength(100);

        builder.OwnsOne(m => m.Name, name =>
        {
            name.WithOwner().HasForeignKey("Id");
            name.Property(mn => mn.Value)
                .HasColumnName("Name")
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.OwnsOne(m => m.Type, type =>
        {
            type.WithOwner().HasForeignKey("Id");
            type.Property(mt => mt.Value)
                .HasColumnName("Type")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsOne(m => m.Unit, unit =>
        {
            unit.WithOwner().HasForeignKey("Id");
            unit.Property(mu => mu.Value)
                .HasColumnName("Unit")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsOne(m => m.MinimumStock, minimumStock =>
        {
            minimumStock.WithOwner().HasForeignKey("Id");
            minimumStock.Property(mq => mq.Value)
                .HasColumnName("MinimumStock")
                .IsRequired()
                .HasColumnType("decimal(10,2)");
        });

        builder.OwnsOne(m => m.Stock, stock =>
        {
            stock.WithOwner().HasForeignKey("Id");
            stock.Property(mq => mq.Value)
                .HasColumnName("Stock")
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);
        });

        builder.OwnsOne(m => m.Price, price =>
        {
            price.WithOwner().HasForeignKey("Id");
            price.Property(mc => mc.Value)
                .HasColumnName("Price")
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);
        });

        builder.HasIndex(m => m.ProjectId)
            .HasDatabaseName("IX_Materials_ProjectId");
    }
}
