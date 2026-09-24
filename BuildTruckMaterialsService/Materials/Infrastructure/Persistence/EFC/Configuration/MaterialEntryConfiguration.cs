using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckMaterialsService.Materials.Infrastructure.Persistence.EFC.Configuration;

public class MaterialEntryConfiguration : IEntityTypeConfiguration<MaterialEntry>
{
    public void Configure(EntityTypeBuilder<MaterialEntry> builder)
    {
        builder.ToTable("MaterialEntries");
        builder.HasKey(me => me.Id);

        builder.Property(me => me.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(me => me.ProjectId)
            .IsRequired();

        builder.Property(me => me.MaterialId)
            .IsRequired();

        builder.Property(me => me.Date)
            .IsRequired();

        builder.Property(me => me.Provider)
            .HasMaxLength(100);

        builder.Property(me => me.Ruc)
            .HasMaxLength(11);

        builder.Property(me => me.DocumentNumber)
            .HasMaxLength(50);

        builder.Property(me => me.Observations)
            .HasMaxLength(500);

        builder.OwnsOne(me => me.Quantity, quantity =>
        {
            quantity.WithOwner().HasForeignKey("Id");
            quantity.Property(mq => mq.Value)
                .HasColumnName("Quantity")
                .IsRequired()
                .HasColumnType("decimal(10,2)");
        });

        builder.OwnsOne(me => me.Unit, unit =>
        {
            unit.WithOwner().HasForeignKey("Id");
            unit.Property(mu => mu.Value)
                .HasColumnName("Unit")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsOne(me => me.Payment, payment =>
        {
            payment.WithOwner().HasForeignKey("Id");
            payment.Property(mp => mp.Value)
                .HasColumnName("Payment")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsOne(me => me.DocumentType, documentType =>
        {
            documentType.WithOwner().HasForeignKey("Id");
            documentType.Property(mdt => mdt.Value)
                .HasColumnName("DocumentType")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsOne(me => me.UnitCost, unitCost =>
        {
            unitCost.WithOwner().HasForeignKey("Id");
            unitCost.Property(muc => muc.Value)
                .HasColumnName("UnitCost")
                .IsRequired()
                .HasColumnType("decimal(10,2)");
        });

        builder.Ignore(me => me.TotalCost);

        builder.OwnsOne(me => me.Status, status =>
        {
            status.WithOwner().HasForeignKey("Id");
            status.Property(ms => ms.Value)
                .HasColumnName("Status")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(me => me.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_MaterialEntries_Materials_MaterialId");

        builder.HasIndex(me => me.ProjectId)
            .HasDatabaseName("IX_MaterialEntries_ProjectId");

        builder.HasIndex(me => me.MaterialId)
            .HasDatabaseName("IX_MaterialEntries_MaterialId");

        builder.HasIndex(me => me.Date)
            .HasDatabaseName("IX_MaterialEntries_Date");
    }
}
