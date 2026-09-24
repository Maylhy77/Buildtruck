namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class ProjectMetrics
{
    public int TotalProjects { get; private set; }
    public int ActiveProjects { get; private set; }
    public int CompletedProjects { get; private set; }
    public int PlannedProjects { get; private set; }
    public int OverdueProjects { get; private set; }
    public Dictionary<string, int> ProjectsByStatus { get; private set; }

    protected ProjectMetrics() { ProjectsByStatus = new Dictionary<string, int>(); }

    public ProjectMetrics(int totalProjects, int activeProjects, int completedProjects, int plannedProjects, int overdueProjects, Dictionary<string, int>? projectsByStatus = null)
    {
        TotalProjects = Math.Max(0, totalProjects);
        ActiveProjects = Math.Max(0, activeProjects);
        CompletedProjects = Math.Max(0, completedProjects);
        PlannedProjects = Math.Max(0, plannedProjects);
        OverdueProjects = Math.Max(0, overdueProjects);
        ProjectsByStatus = projectsByStatus ?? new Dictionary<string, int>();
    }

    public static ProjectMetrics FromCounts(int total, int active, int completed, int planned = 0, int overdue = 0)
    {
        var projectsByStatus = new Dictionary<string, int>
        {
            ["Activo"] = active,
            ["Completado"] = completed,
            ["Planificado"] = planned
        };
        if (overdue > 0) projectsByStatus["Vencido"] = overdue;
        return new ProjectMetrics(total, active, completed, planned, overdue, projectsByStatus);
    }

    public decimal GetCompletionRate()
    {
        if (TotalProjects == 0) return 0m;
        return Math.Round((decimal)CompletedProjects / TotalProjects * 100, 2);
    }

    public decimal GetActiveRate()
    {
        if (TotalProjects == 0) return 0m;
        return Math.Round((decimal)ActiveProjects / TotalProjects * 100, 2);
    }

    public bool HasOverdueProjects() => OverdueProjects > 0;

    public string GetStatusSummary()
    {
        if (TotalProjects == 0) return "Sin proyectos";
        var summary = $"{ActiveProjects} activos, {CompletedProjects} completados";
        if (OverdueProjects > 0) summary += $", {OverdueProjects} vencidos";
        return summary;
    }

    public string GetDominantStatus()
    {
        if (!ProjectsByStatus.Any()) return "Sin datos";
        return ProjectsByStatus.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public ProjectMetrics AddProject(string status)
    {
        var newProjectsByStatus = new Dictionary<string, int>(ProjectsByStatus);
        if (newProjectsByStatus.ContainsKey(status)) newProjectsByStatus[status]++;
        else newProjectsByStatus[status] = 1;

        var newActive = status == "Activo" ? ActiveProjects + 1 : ActiveProjects;
        var newCompleted = status == "Completado" ? CompletedProjects + 1 : CompletedProjects;
        var newPlanned = status == "Planificado" ? PlannedProjects + 1 : PlannedProjects;

        return new ProjectMetrics(TotalProjects + 1, newActive, newCompleted, newPlanned, OverdueProjects, newProjectsByStatus);
    }

    public override string ToString() => $"Projects: {TotalProjects} total ({GetCompletionRate()}% completed)";

    public override bool Equals(object? obj)
    {
        if (obj is not ProjectMetrics other) return false;
        return TotalProjects == other.TotalProjects && ActiveProjects == other.ActiveProjects &&
               CompletedProjects == other.CompletedProjects && PlannedProjects == other.PlannedProjects &&
               OverdueProjects == other.OverdueProjects;
    }

    public override int GetHashCode() => HashCode.Combine(TotalProjects, ActiveProjects, CompletedProjects, PlannedProjects, OverdueProjects);
}
