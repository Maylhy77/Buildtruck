namespace BuildTruckStatsService.Stats.Infrastructure.Persistence.EFC.Configuration;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

public class ManagerStatsConfiguration : IEntityTypeConfiguration<ManagerStats>
{
    public void Configure(EntityTypeBuilder<ManagerStats> builder)
    {
        builder.ToTable("ManagerStats");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.ManagerId)
            .IsRequired()
            .HasComment("References Users.Id");

        // StatsPeriod value object
        builder.OwnsOne(s => s.Period, period =>
        {
            period.WithOwner().HasForeignKey("Id");
            period.Property(p => p.StartDate).HasColumnName("PeriodStartDate").IsRequired();
            period.Property(p => p.EndDate).HasColumnName("PeriodEndDate").IsRequired();
            period.Property(p => p.PeriodType).HasColumnName("PeriodType").HasMaxLength(50).IsRequired();
            period.Property(p => p.DisplayName).HasColumnName("PeriodDisplayName").HasMaxLength(200).IsRequired();
        });

        // ProjectMetrics value object
        builder.OwnsOne(s => s.ProjectMetrics, projects =>
        {
            projects.WithOwner().HasForeignKey("Id");
            projects.Property(p => p.TotalProjects).HasColumnName("TotalProjects").IsRequired();
            projects.Property(p => p.ActiveProjects).HasColumnName("ActiveProjects").IsRequired();
            projects.Property(p => p.CompletedProjects).HasColumnName("CompletedProjects").IsRequired();
            projects.Property(p => p.PlannedProjects).HasColumnName("PlannedProjects").IsRequired();
            projects.Property(p => p.OverdueProjects).HasColumnName("OverdueProjects").IsRequired();
            projects.Property(p => p.ProjectsByStatus)
                .HasColumnName("ProjectsByStatus").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
        });

        // PersonnelMetrics value object
        builder.OwnsOne(s => s.PersonnelMetrics, personnel =>
        {
            personnel.WithOwner().HasForeignKey("Id");
            personnel.Property(p => p.TotalPersonnel).HasColumnName("TotalPersonnel").IsRequired();
            personnel.Property(p => p.ActivePersonnel).HasColumnName("ActivePersonnel").IsRequired();
            personnel.Property(p => p.InactivePersonnel).HasColumnName("InactivePersonnel").IsRequired();
            personnel.Property(p => p.TotalSalaryAmount).HasColumnName("TotalSalaryAmount").HasColumnType("decimal(18,2)").IsRequired();
            personnel.Property(p => p.AverageAttendanceRate).HasColumnName("AverageAttendanceRate").HasColumnType("decimal(5,2)").IsRequired();
            personnel.Property(p => p.PersonnelByType)
                .HasColumnName("PersonnelByType").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            personnel.Property(p => p.PersonnelByProject)
                .HasColumnName("PersonnelByProject").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
        });

        // IncidentMetrics value object
        builder.OwnsOne(s => s.IncidentMetrics, incidents =>
        {
            incidents.WithOwner().HasForeignKey("Id");
            incidents.Property(i => i.TotalIncidents).HasColumnName("TotalIncidents").IsRequired();
            incidents.Property(i => i.CriticalIncidents).HasColumnName("CriticalIncidents").IsRequired();
            incidents.Property(i => i.OpenIncidents).HasColumnName("OpenIncidents").IsRequired();
            incidents.Property(i => i.ResolvedIncidents).HasColumnName("ResolvedIncidents").IsRequired();
            incidents.Property(i => i.AverageResolutionTimeHours).HasColumnName("AverageResolutionTimeHours").HasColumnType("decimal(8,2)").IsRequired();
            incidents.Property(i => i.IncidentsBySeverity)
                .HasColumnName("IncidentsBySeverity").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            incidents.Property(i => i.IncidentsByType)
                .HasColumnName("IncidentsByType").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            incidents.Property(i => i.IncidentsByStatus)
                .HasColumnName("IncidentsByStatus").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
        });

        // MaterialMetrics value object
        builder.OwnsOne(s => s.MaterialMetrics, materials =>
        {
            materials.WithOwner().HasForeignKey("Id");
            materials.Property(m => m.TotalMaterials).HasColumnName("TotalMaterials").IsRequired();
            materials.Property(m => m.MaterialsInStock).HasColumnName("MaterialsInStock").IsRequired();
            materials.Property(m => m.MaterialsLowStock).HasColumnName("MaterialsLowStock").IsRequired();
            materials.Property(m => m.MaterialsOutOfStock).HasColumnName("MaterialsOutOfStock").IsRequired();
            materials.Property(m => m.TotalMaterialCost).HasColumnName("TotalMaterialCost").HasColumnType("decimal(18,2)").IsRequired();
            materials.Property(m => m.TotalUsageCost).HasColumnName("TotalUsageCost").HasColumnType("decimal(18,2)").IsRequired();
            materials.Property(m => m.AverageUsageRate).HasColumnName("AverageUsageRate").HasColumnType("decimal(5,2)").IsRequired();
            materials.Property(m => m.MaterialsByCategory)
                .HasColumnName("MaterialsByCategory").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            materials.Property(m => m.CostsByCategory)
                .HasColumnName("CostsByCategory").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, decimal>());
        });

        // MachineryMetrics value object
        builder.OwnsOne(s => s.MachineryMetrics, machinery =>
        {
            machinery.WithOwner().HasForeignKey("Id");
            machinery.Property(m => m.TotalMachinery).HasColumnName("TotalMachinery").IsRequired();
            machinery.Property(m => m.ActiveMachinery).HasColumnName("ActiveMachinery").IsRequired();
            machinery.Property(m => m.InMaintenanceMachinery).HasColumnName("InMaintenanceMachinery").IsRequired();
            machinery.Property(m => m.InactiveMachinery).HasColumnName("InactiveMachinery").IsRequired();
            machinery.Property(m => m.OverallAvailabilityRate).HasColumnName("OverallAvailabilityRate").HasColumnType("decimal(5,2)").IsRequired();
            machinery.Property(m => m.AverageMaintenanceTimeHours).HasColumnName("AverageMaintenanceTimeHours").HasColumnType("decimal(8,2)").IsRequired();
            machinery.Property(m => m.MachineryByStatus)
                .HasColumnName("MachineryByStatus").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            machinery.Property(m => m.MachineryByType)
                .HasColumnName("MachineryByType").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
            machinery.Property(m => m.MachineryByProject)
                .HasColumnName("MachineryByProject").HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, int>());
        });

        // Performance and alerts
        builder.Property(s => s.OverallPerformanceScore).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(s => s.PerformanceGrade).HasMaxLength(2).IsRequired();
        builder.Property(s => s.Alerts)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default) ?? new List<string>());
        builder.Property(s => s.Recommendations)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default) ?? new List<string>());

        // Metadata
        builder.Property(s => s.CalculatedAt).IsRequired();
        builder.Property(s => s.IsCurrentPeriod).IsRequired();
        builder.Property(s => s.CalculationSource).HasMaxLength(100).IsRequired();

        // Indexes
        builder.HasIndex(s => s.ManagerId).HasDatabaseName("IX_ManagerStats_ManagerId");
        builder.HasIndex(s => s.CalculatedAt).HasDatabaseName("IX_ManagerStats_CalculatedAt");
        builder.HasIndex(s => s.IsCurrentPeriod).HasDatabaseName("IX_ManagerStats_IsCurrentPeriod");
        builder.HasIndex(s => s.OverallPerformanceScore).HasDatabaseName("IX_ManagerStats_PerformanceScore");
        builder.HasIndex(s => s.PerformanceGrade).HasDatabaseName("IX_ManagerStats_PerformanceGrade");
        builder.HasIndex(s => new { s.ManagerId, s.IsCurrentPeriod }).HasDatabaseName("IX_ManagerStats_Manager_Current");
    }
}
