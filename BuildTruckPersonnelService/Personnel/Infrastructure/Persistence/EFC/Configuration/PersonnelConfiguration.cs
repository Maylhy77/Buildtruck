using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Configuration;

public class PersonnelConfiguration : IEntityTypeConfiguration<PersonnelEntity>
{
    public void Configure(EntityTypeBuilder<PersonnelEntity> builder)
    {
        builder.ToTable("Personnel");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(p => p.ProjectId).HasColumnName("ProjectId").IsRequired();
        builder.Property(p => p.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Lastname).HasColumnName("Lastname").HasMaxLength(100).IsRequired();
        builder.Property(p => p.DocumentNumber).HasColumnName("DocumentNumber").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Position).HasColumnName("Position").HasMaxLength(150).IsRequired();
        builder.Property(p => p.Department).HasColumnName("Department").HasMaxLength(100).IsRequired();

        builder.Property(p => p.PersonnelType)
            .HasColumnName("PersonnelType")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.MonthlyAmount).HasColumnName("MonthlyAmount").HasColumnType("decimal(10,2)").HasDefaultValue(0);
        builder.Property(p => p.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(10,2)").HasDefaultValue(0);
        builder.Property(p => p.Discount).HasColumnName("Discount").HasColumnType("decimal(10,2)").HasDefaultValue(0);
        builder.Property(p => p.Bank).HasColumnName("Bank").HasMaxLength(50);
        builder.Property(p => p.AccountNumber).HasColumnName("AccountNumber").HasMaxLength(50);

        builder.Property(p => p.StartDate).HasColumnName("StartDate").HasColumnType("date");
        builder.Property(p => p.EndDate).HasColumnName("EndDate").HasColumnType("date");

        builder.Property(p => p.WorkedDays).HasColumnName("WorkedDays").HasDefaultValue(0);
        builder.Property(p => p.CompensatoryDays).HasColumnName("CompensatoryDays").HasDefaultValue(0);
        builder.Property(p => p.UnpaidLeave).HasColumnName("UnpaidLeave").HasDefaultValue(0);
        builder.Property(p => p.Absences).HasColumnName("Absences").HasDefaultValue(0);
        builder.Property(p => p.Sundays).HasColumnName("Sundays").HasDefaultValue(0);
        builder.Property(p => p.TotalDays).HasColumnName("TotalDays").HasDefaultValue(0);

        builder.Property(p => p.MonthlyAttendanceJson)
            .HasColumnName("MonthlyAttendanceJson")
            .HasColumnType("json")
            .HasDefaultValueSql("('{}')");

        builder.Property(p => p.Phone).HasColumnName("Phone").HasMaxLength(20);
        builder.Property(p => p.Email).HasColumnName("Email").HasMaxLength(150);
        builder.Property(p => p.AvatarUrl).HasColumnName("AvatarUrl").HasColumnType("text");

        builder.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

        builder.Property(p => p.CreatedDate).HasColumnName("CreatedAt");
        builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedAt");

        builder.HasIndex(p => p.ProjectId).HasDatabaseName("IX_Personnel_ProjectId");
        builder.HasIndex(p => p.Status).HasDatabaseName("IX_Personnel_Status");
        builder.HasIndex(p => p.PersonnelType).HasDatabaseName("IX_Personnel_PersonnelType");
        builder.HasIndex(p => p.IsDeleted).HasDatabaseName("IX_Personnel_IsDeleted");

        builder.Ignore(p => p.MonthlyAttendance);
    }
}
