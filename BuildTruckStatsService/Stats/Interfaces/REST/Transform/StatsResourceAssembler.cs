namespace BuildTruckStatsService.Stats.Interfaces.REST.Transform;

using BuildTruckStatsService.Stats.Domain.Model.Aggregates;
using BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public static class StatsResourceAssembler
{
    public static ManagerStatsResource ToResourceFromEntity(ManagerStats entity)
    {
        return new ManagerStatsResource(
            Id: entity.Id,
            ManagerId: entity.ManagerId,
            Period: ToResourceFromEntity(entity.Period),
            ProjectMetrics: ToResourceFromEntity(entity.ProjectMetrics),
            PersonnelMetrics: ToResourceFromEntity(entity.PersonnelMetrics),
            IncidentMetrics: ToResourceFromEntity(entity.IncidentMetrics),
            MaterialMetrics: ToResourceFromEntity(entity.MaterialMetrics),
            MachineryMetrics: ToResourceFromEntity(entity.MachineryMetrics),
            OverallPerformanceScore: entity.OverallPerformanceScore,
            PerformanceGrade: entity.PerformanceGrade,
            Alerts: entity.Alerts.ToList(),
            Recommendations: entity.Recommendations.ToList(),
            CalculatedAt: entity.CalculatedAt,
            IsCurrentPeriod: entity.IsCurrentPeriod,
            CalculationSource: entity.CalculationSource,
            HasCriticalAlerts: entity.HasCriticalAlerts(),
            OverallStatus: entity.GetOverallStatus(),
            ScoreBreakdown: entity.GetScoreBreakdown()
        );
    }

    public static StatsHistoryResource ToResourceFromEntity(StatsHistory entity)
    {
        return new StatsHistoryResource(
            Id: entity.Id,
            ManagerId: entity.ManagerId,
            ManagerStatsId: entity.ManagerStatsId,
            Period: ToResourceFromEntity(entity.Period),
            SnapshotDate: entity.SnapshotDate,
            PeriodType: entity.PeriodType,
            OverallPerformanceScore: entity.OverallPerformanceScore,
            PerformanceGrade: entity.PerformanceGrade,
            ProjectMetrics: new ProjectHistoryMetricsResource(
                TotalProjects: entity.TotalProjects,
                ActiveProjects: entity.ActiveProjects,
                CompletedProjects: entity.CompletedProjects,
                ProjectCompletionRate: entity.ProjectCompletionRate
            ),
            PersonnelMetrics: new PersonnelHistoryMetricsResource(
                TotalPersonnel: entity.TotalPersonnel,
                ActivePersonnel: entity.ActivePersonnel,
                PersonnelActiveRate: entity.PersonnelActiveRate,
                PersonnelEfficiencyScore: entity.PersonnelEfficiencyScore
            ),
            IncidentMetrics: new IncidentHistoryMetricsResource(
                TotalIncidents: entity.TotalIncidents,
                CriticalIncidents: entity.CriticalIncidents,
                SafetyScore: entity.SafetyScore
            ),
            MaterialMetrics: new MaterialHistoryMetricsResource(
                TotalMaterials: entity.TotalMaterials,
                MaterialsOutOfStock: entity.MaterialsOutOfStock,
                TotalMaterialCost: entity.TotalMaterialCost,
                InventoryHealthScore: entity.InventoryHealthScore
            ),
            MachineryMetrics: new MachineryHistoryMetricsResource(
                TotalMachinery: entity.TotalMachinery,
                ActiveMachinery: entity.ActiveMachinery,
                MachineryAvailabilityRate: entity.MachineryAvailabilityRate,
                MachineryEfficiencyScore: entity.MachineryEfficiencyScore
            ),
            DataSource: entity.DataSource,
            Notes: entity.Notes,
            IsManualSnapshot: entity.IsManualSnapshot,
            PeriodDescription: entity.GetPeriodDescription(),
            SummaryData: entity.GetSummaryData(),
            SnapshotSummary: entity.GetSnapshotSummary(),
            IsFromToday: entity.IsFromToday(),
            IsFromThisWeek: entity.IsFromThisWeek(),
            AgeInDays: entity.GetAgeInDays(),
            FormattedSnapshotDate: entity.GetSnapshotDateFormatted(),
            RelativeSnapshotTime: entity.GetRelativeSnapshotTime(),
            ShouldBeArchived: entity.ShouldBeArchived()
        );
    }

    private static PeriodResource ToResourceFromEntity(Domain.Model.ValueObjects.StatsPeriod period)
    {
        return new PeriodResource(
            StartDate: period.StartDate,
            EndDate: period.EndDate,
            PeriodType: period.PeriodType,
            DisplayName: period.DisplayName,
            TotalDays: period.GetTotalDays(),
            IsCurrentPeriod: period.IsCurrentPeriod()
        );
    }

    private static ProjectMetricsResource ToResourceFromEntity(Domain.Model.ValueObjects.ProjectMetrics metrics)
    {
        return new ProjectMetricsResource(
            TotalProjects: metrics.TotalProjects,
            ActiveProjects: metrics.ActiveProjects,
            CompletedProjects: metrics.CompletedProjects,
            PlannedProjects: metrics.PlannedProjects,
            OverdueProjects: metrics.OverdueProjects,
            ProjectsByStatus: metrics.ProjectsByStatus.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CompletionRate: metrics.GetCompletionRate(),
            ActiveRate: metrics.GetActiveRate(),
            HasOverdueProjects: metrics.HasOverdueProjects(),
            StatusSummary: metrics.GetStatusSummary(),
            DominantStatus: metrics.GetDominantStatus()
        );
    }

    private static PersonnelMetricsResource ToResourceFromEntity(Domain.Model.ValueObjects.PersonnelMetrics metrics)
    {
        return new PersonnelMetricsResource(
            TotalPersonnel: metrics.TotalPersonnel,
            ActivePersonnel: metrics.ActivePersonnel,
            InactivePersonnel: metrics.InactivePersonnel,
            PersonnelByType: metrics.PersonnelByType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            TotalSalaryAmount: metrics.TotalSalaryAmount,
            AverageAttendanceRate: metrics.AverageAttendanceRate,
            ActiveRate: metrics.GetActiveRate(),
            InactiveRate: metrics.GetInactiveRate(),
            AverageSalary: metrics.GetAverageSalary(),
            DominantPersonnelType: metrics.GetDominantPersonnelType(),
            HasGoodAttendance: metrics.HasGoodAttendance(),
            AttendanceStatus: metrics.GetAttendanceStatus(),
            PersonnelSummary: metrics.GetPersonnelSummary(),
            EfficiencyScore: metrics.GetEfficiencyScore()
        );
    }

    private static IncidentMetricsResource ToResourceFromEntity(Domain.Model.ValueObjects.IncidentMetrics metrics)
    {
        return new IncidentMetricsResource(
            TotalIncidents: metrics.TotalIncidents,
            CriticalIncidents: metrics.CriticalIncidents,
            OpenIncidents: metrics.OpenIncidents,
            ResolvedIncidents: metrics.ResolvedIncidents,
            IncidentsBySeverity: metrics.IncidentsBySeverity.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            IncidentsByType: metrics.IncidentsByType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            IncidentsByStatus: metrics.IncidentsByStatus.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            AverageResolutionTimeHours: metrics.AverageResolutionTimeHours,
            CriticalRate: metrics.GetCriticalRate(),
            ResolutionRate: metrics.GetResolutionRate(),
            OpenRate: metrics.GetOpenRate(),
            SafetyStatus: metrics.GetSafetyStatus(),
            HasCriticalIncidents: metrics.HasCriticalIncidents(),
            NeedsAttention: metrics.NeedsAttention(),
            MostCommonSeverity: metrics.GetMostCommonSeverity(),
            MostCommonType: metrics.GetMostCommonType(),
            SafetyScore: metrics.GetSafetyScore(),
            IncidentSummary: metrics.GetIncidentSummary()
        );
    }

    private static MaterialMetricsResource ToResourceFromEntity(Domain.Model.ValueObjects.MaterialMetrics metrics)
    {
        return new MaterialMetricsResource(
            TotalMaterials: metrics.TotalMaterials,
            MaterialsInStock: metrics.MaterialsInStock,
            MaterialsLowStock: metrics.MaterialsLowStock,
            MaterialsOutOfStock: metrics.MaterialsOutOfStock,
            TotalMaterialCost: metrics.TotalMaterialCost,
            TotalUsageCost: metrics.TotalUsageCost,
            MaterialsByCategory: metrics.MaterialsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CostsByCategory: metrics.CostsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            AverageUsageRate: metrics.AverageUsageRate,
            StockRate: metrics.GetStockRate(),
            LowStockRate: metrics.GetLowStockRate(),
            OutOfStockRate: metrics.GetOutOfStockRate(),
            CostEfficiencyRate: metrics.GetCostEfficiencyRate(),
            AverageMaterialCost: metrics.GetAverageMaterialCost(),
            StockStatus: metrics.GetStockStatus(),
            NeedsRestocking: metrics.NeedsRestocking(),
            MostUsedCategory: metrics.GetMostUsedCategory(),
            LargestCategory: metrics.GetLargestCategory(),
            InventoryHealthScore: metrics.GetInventoryHealthScore(),
            MaterialSummary: metrics.GetMaterialSummary(),
            CostSummary: metrics.GetCostSummary(),
            StockAlerts: metrics.GetStockAlerts()
        );
    }

    private static MachineryMetricsResource ToResourceFromEntity(Domain.Model.ValueObjects.MachineryMetrics metrics)
    {
        return new MachineryMetricsResource(
            TotalMachinery: metrics.TotalMachinery,
            ActiveMachinery: metrics.ActiveMachinery,
            InMaintenanceMachinery: metrics.InMaintenanceMachinery,
            InactiveMachinery: metrics.InactiveMachinery,
            MachineryByStatus: metrics.MachineryByStatus.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            MachineryByType: metrics.MachineryByType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            MachineryByProject: metrics.MachineryByProject.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            OverallAvailabilityRate: metrics.OverallAvailabilityRate,
            AverageMaintenanceTimeHours: metrics.AverageMaintenanceTimeHours,
            ActiveRate: metrics.GetActiveRate(),
            MaintenanceRate: metrics.GetMaintenanceRate(),
            InactiveRate: metrics.GetInactiveRate(),
            OperationalRate: metrics.GetOperationalRate(),
            AvailabilityStatus: metrics.GetAvailabilityStatus(),
            HasHighAvailability: metrics.HasHighAvailability(),
            NeedsMaintenance: metrics.NeedsMaintenance(),
            MostCommonStatus: metrics.GetMostCommonStatus(),
            MostCommonType: metrics.GetMostCommonType(),
            ProjectWithMostMachinery: metrics.GetProjectWithMostMachinery(),
            EfficiencyScore: metrics.GetEfficiencyScore(),
            MachinerySummary: metrics.GetMachinerySummary(),
            MaintenanceSummary: metrics.GetMaintenanceSummary(),
            MaintenanceAlerts: metrics.GetMaintenanceAlerts()
        );
    }

    public static IEnumerable<ManagerStatsResource> ToResourceFromEntityCollection(IEnumerable<ManagerStats> entities)
        => entities.Select(ToResourceFromEntity);

    public static IEnumerable<StatsHistoryResource> ToResourceFromEntityCollection(IEnumerable<StatsHistory> entities)
        => entities.Select(ToResourceFromEntity);
}
