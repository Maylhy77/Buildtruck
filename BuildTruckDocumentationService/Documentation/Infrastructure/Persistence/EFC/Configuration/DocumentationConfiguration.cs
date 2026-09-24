using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckDocumentationService.Documentation.Infrastructure.Persistence.EFC.Configuration;

public class DocumentationConfiguration : IEntityTypeConfiguration<Domain.Model.Aggregates.Documentation>
{
    public void Configure(EntityTypeBuilder<Domain.Model.Aggregates.Documentation> builder)
    {
        // Table configuration
        builder.ToTable("Documentation");
        builder.HasKey(d => d.Id);

        // Primary key
        builder.Property(d => d.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        // Basic Information
        builder.Property(d => d.ProjectId)
            .HasColumnName("ProjectId")
            .IsRequired();

        builder.Property(d => d.Title)
            .HasColumnName("Title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(d => d.ImagePath)
            .HasColumnName("ImagePath")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(d => d.Date)
            .HasColumnName("Date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .HasColumnName("CreatedBy")
            .IsRequired();

        // Audit Fields
        builder.Property(d => d.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasDefaultValue(false);

        // Timestamps (handled by EntityFrameworkCore.CreatedUpdatedDate)
        builder.Property(d => d.CreatedDate)
            .HasColumnName("CreatedAt");

        builder.Property(d => d.UpdatedDate)
            .HasColumnName("UpdatedAt");

        // Indexes for performance
        builder.HasIndex(d => d.ProjectId)
            .HasDatabaseName("IX_Documentation_ProjectId");

        builder.HasIndex(d => new { d.ProjectId, d.Title })
            .HasDatabaseName("IX_Documentation_ProjectId_Title")
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        builder.HasIndex(d => d.Date)
            .HasDatabaseName("IX_Documentation_Date");

        builder.HasIndex(d => d.CreatedBy)
            .HasDatabaseName("IX_Documentation_CreatedBy");

        builder.HasIndex(d => d.IsDeleted)
            .HasDatabaseName("IX_Documentation_IsDeleted");

        builder.HasIndex(d => new { d.ProjectId, d.Date })
            .HasDatabaseName("IX_Documentation_ProjectId_Date");
    }
}
