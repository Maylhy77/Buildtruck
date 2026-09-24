namespace BuildTruckStatsService.Stats.Domain.Model.Aggregates;

using BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public partial class StatsHistory
{
    public int Id { get; private set; }
    public int ManagerId { get; private set; }
    public int ManagerStatsId { get; private set; }

    public StatsPeriod Period { get; private set; } = null!;
    public DateTime SnapshotDate { get; private set; }
    public string PeriodType { get; private set; } = string.Empty;

    public decimal OverallPerformanceScore { get; private set; }
    public string PerformanceGrade { get; private set; } = string.Empty;

    public int TotalProjects { get; private set; }
    public int ActiveProjects { get; private set; }
    public int CompletedProjects { get; private set; }
    public decimal ProjectCompletionRate { get; private set; }

    public int TotalPersonnel { get; private set; }
    public int ActivePersonnel { get; private set; }
    public decimal PersonnelActiveRate { get; private set; }
    public decimal PersonnelEfficiencyScore { get; private set; }

    public int TotalIncidents { get; private set; }
    public int CriticalIncidents { get; private set; }
    public decimal SafetyScore { get; private set; }

    public int TotalMaterials { get; private set; }
    public int MaterialsOutOfStock { get; private set; }
    public decimal TotalMaterialCost { get; private set; }
    public decimal InventoryHealthScore { get; private set; }

    public int TotalMachinery { get; private set; }
    public int ActiveMachinery { get; private set; }
    public decimal MachineryAvailabilityRate { get; private set; }
    public decimal MachineryEfficiencyScore { get; private set; }

    public string DataSource { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public bool IsManualSnapshot { get; private set; }

    protected StatsHistory() { }

    public StatsHistory(ManagerStats managerStats, string notes = "", bool isManualSnapshot = false)
    {
        if (managerStats == null) throw new ArgumentNullException(nameof(managerStats));

        ManagerId = managerStats.ManagerId;
        ManagerStatsId = managerStats.Id;
        Period = managerStats.Period;
        SnapshotDate = DateTime.UtcNow.AddHours(-5);
        PeriodType = managerStats.Period.PeriodType;

        OverallPerformanceScore = managerStats.OverallPerformanceScore;
        PerformanceGrade = managerStats.PerformanceGrade;

        TotalProjects = managerStats.ProjectMetrics.TotalProjects;
        ActiveProjects = managerStats.ProjectMetrics.ActiveProjects;
        CompletedProjects = managerStats.ProjectMetrics.CompletedProjects;
        ProjectCompletionRate = managerStats.ProjectMetrics.GetCompletionRate();

        TotalPersonnel = managerStats.PersonnelMetrics.TotalPersonnel;
        ActivePersonnel = managerStats.PersonnelMetrics.ActivePersonnel;
        PersonnelActiveRate = managerStats.PersonnelMetrics.GetActiveRate();
        PersonnelEfficiencyScore = managerStats.PersonnelMetrics.GetEfficiencyScore();

        TotalIncidents = managerStats.IncidentMetrics.TotalIncidents;
        CriticalIncidents = managerStats.IncidentMetrics.CriticalIncidents;
        SafetyScore = managerStats.IncidentMetrics.GetSafetyScore();

        TotalMaterials = managerStats.MaterialMetrics.TotalMaterials;
        MaterialsOutOfStock = managerStats.MaterialMetrics.MaterialsOutOfStock;
        TotalMaterialCost = managerStats.MaterialMetrics.TotalMaterialCost;
        InventoryHealthScore = managerStats.MaterialMetrics.GetInventoryHealthScore();

        TotalMachinery = managerStats.MachineryMetrics.TotalMachinery;
        ActiveMachinery = managerStats.MachineryMetrics.ActiveMachinery;
        MachineryAvailabilityRate = managerStats.MachineryMetrics.GetActiveRate();
        MachineryEfficiencyScore = managerStats.MachineryMetrics.GetEfficiencyScore();

        DataSource = managerStats.CalculationSource;
        Notes = notes;
        IsManualSnapshot = isManualSnapshot;
    }

    public void UpdateNotes(string newNotes) { Notes = newNotes ?? string.Empty; }
    public void MarkAsManual() { IsManualSnapshot = true; }

    public StatsComparison CompareTo(StatsHistory previousHistory)
    {
        if (previousHistory == null) throw new ArgumentNullException(nameof(previousHistory));

        return new StatsComparison
        {
            CurrentSnapshot = this,
            PreviousSnapshot = previousHistory,
            PerformanceChange = OverallPerformanceScore - previousHistory.OverallPerformanceScore,
            ProjectsChange = TotalProjects - previousHistory.TotalProjects,
            PersonnelChange = TotalPersonnel - previousHistory.TotalPersonnel,
            IncidentsChange = TotalIncidents - previousHistory.TotalIncidents,
            SafetyScoreChange = SafetyScore - previousHistory.SafetyScore,
            CompletionRateChange = ProjectCompletionRate - previousHistory.ProjectCompletionRate
        };
    }

    public bool ShowsImprovement(StatsHistory previousHistory)
    {
        if (previousHistory == null) return true;
        var comparison = CompareTo(previousHistory);
        return comparison.PerformanceChange > 0 && comparison.SafetyScoreChange >= 0;
    }

    public bool IsFromCurrentMonth()
    {
        var now = DateTime.UtcNow.AddHours(-5);
        return SnapshotDate.Year == now.Year && SnapshotDate.Month == now.Month;
    }

    public bool IsFromCurrentQuarter()
    {
        var now = DateTime.UtcNow.AddHours(-5);
        var currentQuarter = (now.Month - 1) / 3 + 1;
        var snapshotQuarter = (SnapshotDate.Month - 1) / 3 + 1;
        return SnapshotDate.Year == now.Year && currentQuarter == snapshotQuarter;
    }

    public string GetPeriodDescription()
    {
        return PeriodType switch
        {
            "CURRENT_MONTH" => $"Mes de {SnapshotDate:MMMM yyyy}",
            "CURRENT_QUARTER" => $"Q{(SnapshotDate.Month - 1) / 3 + 1} {SnapshotDate.Year}",
            "CURRENT_YEAR" => $"Año {SnapshotDate.Year}",
            "CUSTOM" => Period.DisplayName,
            _ => Period.DisplayName
        };
    }

    public Dictionary<string, object> GetSummaryData()
    {
        return new Dictionary<string, object>
        {
            ["SnapshotDate"] = SnapshotDate,
            ["Period"] = GetPeriodDescription(),
            ["OverallScore"] = OverallPerformanceScore,
            ["Grade"] = PerformanceGrade,
            ["Projects"] = new { Total = TotalProjects, Active = ActiveProjects, Completed = CompletedProjects },
            ["Personnel"] = new { Total = TotalPersonnel, Active = ActivePersonnel },
            ["Safety"] = new { Incidents = TotalIncidents, Critical = CriticalIncidents, Score = SafetyScore },
            ["Materials"] = new { Total = TotalMaterials, OutOfStock = MaterialsOutOfStock },
            ["Machinery"] = new { Total = TotalMachinery, Active = ActiveMachinery }
        };
    }

    public string GetSnapshotSummary()
    {
        var summary = $"Snapshot del {SnapshotDate:dd/MM/yyyy} - ";
        summary += $"Rendimiento: {OverallPerformanceScore:F1}% ({PerformanceGrade})\n";
        summary += $"Proyectos: {CompletedProjects}/{TotalProjects} completados\n";
        summary += $"Seguridad: {SafetyScore:F1}% ({CriticalIncidents} críticos)\n";
        summary += $"Personal: {ActivePersonnel}/{TotalPersonnel} activo";
        if (!string.IsNullOrEmpty(Notes)) summary += $"\nNotas: {Notes}";
        return summary;
    }

    public bool IsValid() => ManagerId > 0 && ManagerStatsId > 0 && Period != null && SnapshotDate != default;

    public override string ToString() => $"StatsHistory[{ManagerId}] {SnapshotDate:dd/MM/yyyy} - {OverallPerformanceScore:F1}% ({PerformanceGrade})";

    public override bool Equals(object? obj)
    {
        if (obj is not StatsHistory other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id > 0 && other.Id > 0 && Id == other.Id;
    }

    public override int GetHashCode() => Id > 0 ? Id.GetHashCode() : base.GetHashCode();
}

public class StatsComparison
{
    public StatsHistory CurrentSnapshot { get; set; } = null!;
    public StatsHistory PreviousSnapshot { get; set; } = null!;
    public decimal PerformanceChange { get; set; }
    public int ProjectsChange { get; set; }
    public int PersonnelChange { get; set; }
    public int IncidentsChange { get; set; }
    public decimal SafetyScoreChange { get; set; }
    public decimal CompletionRateChange { get; set; }

    public bool IsImprovement => PerformanceChange > 0;
    public bool IsSafer => SafetyScoreChange > 0 || (SafetyScoreChange == 0 && IncidentsChange <= 0);
    public bool IsMoreProductive => CompletionRateChange > 0;
}
