namespace BuildTruckStatsService.Stats.Domain.Model.ValueObjects;

public class PersonnelMetrics
{
    public int TotalPersonnel { get; private set; }
    public int ActivePersonnel { get; private set; }
    public int InactivePersonnel { get; private set; }
    public Dictionary<string, int> PersonnelByType { get; private set; }
    public Dictionary<string, int> PersonnelByProject { get; private set; }
    public decimal TotalSalaryAmount { get; private set; }
    public decimal AverageAttendanceRate { get; private set; }

    protected PersonnelMetrics()
    {
        PersonnelByType = new Dictionary<string, int>();
        PersonnelByProject = new Dictionary<string, int>();
    }

    public PersonnelMetrics(int totalPersonnel, int activePersonnel, int inactivePersonnel,
        Dictionary<string, int>? personnelByType = null, Dictionary<string, int>? personnelByProject = null,
        decimal totalSalaryAmount = 0m, decimal averageAttendanceRate = 0m)
    {
        TotalPersonnel = Math.Max(0, totalPersonnel);
        ActivePersonnel = Math.Max(0, activePersonnel);
        InactivePersonnel = Math.Max(0, inactivePersonnel);
        PersonnelByType = personnelByType ?? new Dictionary<string, int>();
        PersonnelByProject = personnelByProject ?? new Dictionary<string, int>();
        TotalSalaryAmount = Math.Max(0m, totalSalaryAmount);
        AverageAttendanceRate = Math.Max(0m, Math.Min(100m, averageAttendanceRate));
    }

    public static PersonnelMetrics FromCounts(int total, int active, Dictionary<string, int>? byType = null, decimal totalSalary = 0m)
    {
        return new PersonnelMetrics(total, active, total - active, byType, null, totalSalary);
    }

    public decimal GetActiveRate()
    {
        if (TotalPersonnel == 0) return 0m;
        return Math.Round((decimal)ActivePersonnel / TotalPersonnel * 100, 2);
    }

    public decimal GetInactiveRate()
    {
        if (TotalPersonnel == 0) return 0m;
        return Math.Round((decimal)InactivePersonnel / TotalPersonnel * 100, 2);
    }

    public decimal GetAverageSalary()
    {
        if (ActivePersonnel == 0) return 0m;
        return Math.Round(TotalSalaryAmount / ActivePersonnel, 2);
    }

    public string GetDominantPersonnelType()
    {
        if (!PersonnelByType.Any()) return "Sin datos";
        return PersonnelByType.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    public bool HasGoodAttendance() => AverageAttendanceRate >= 85m;

    public string GetAttendanceStatus()
    {
        return AverageAttendanceRate switch
        {
            >= 95m => "Excelente",
            >= 85m => "Buena",
            >= 70m => "Regular",
            _ => "Necesita mejora"
        };
    }

    public string GetPersonnelSummary()
    {
        if (TotalPersonnel == 0) return "Sin personal";
        var summary = $"{ActivePersonnel} activos de {TotalPersonnel} total";
        if (AverageAttendanceRate > 0) summary += $" ({AverageAttendanceRate:F1}% asistencia)";
        return summary;
    }

    public decimal GetEfficiencyScore()
    {
        if (TotalPersonnel == 0) return 0m;
        var activeWeight = 0.6m;
        var attendanceWeight = 0.4m;
        return Math.Round((GetActiveRate() * activeWeight) + (AverageAttendanceRate * attendanceWeight), 2);
    }

    public PersonnelMetrics UpdateAttendance(decimal newAttendanceRate)
    {
        return new PersonnelMetrics(TotalPersonnel, ActivePersonnel, InactivePersonnel,
            PersonnelByType, PersonnelByProject, TotalSalaryAmount, Math.Max(0m, Math.Min(100m, newAttendanceRate)));
    }

    public PersonnelMetrics AddPersonnel(string type, bool isActive, decimal salary = 0m)
    {
        var newPersonnelByType = new Dictionary<string, int>(PersonnelByType);
        if (newPersonnelByType.ContainsKey(type)) newPersonnelByType[type]++;
        else newPersonnelByType[type] = 1;

        var newActive = isActive ? ActivePersonnel + 1 : ActivePersonnel;
        var newInactive = !isActive ? InactivePersonnel + 1 : InactivePersonnel;

        return new PersonnelMetrics(TotalPersonnel + 1, newActive, newInactive,
            newPersonnelByType, PersonnelByProject, TotalSalaryAmount + salary, AverageAttendanceRate);
    }

    public override string ToString() => $"Personnel: {ActivePersonnel}/{TotalPersonnel} active ({GetActiveRate()}%)";

    public override bool Equals(object? obj)
    {
        if (obj is not PersonnelMetrics other) return false;
        return TotalPersonnel == other.TotalPersonnel && ActivePersonnel == other.ActivePersonnel &&
               InactivePersonnel == other.InactivePersonnel && TotalSalaryAmount == other.TotalSalaryAmount;
    }

    public override int GetHashCode() => HashCode.Combine(TotalPersonnel, ActivePersonnel, InactivePersonnel, TotalSalaryAmount);
}
