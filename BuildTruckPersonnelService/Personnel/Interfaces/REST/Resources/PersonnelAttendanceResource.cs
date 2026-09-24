using System.ComponentModel.DataAnnotations;

namespace BuildTruckPersonnelService.Personnel.Interfaces.REST.Resources;

public class PersonnelAttendanceResource
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PersonnelId { get; init; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; init; }

    [Required]
    [Range(1, 12)]
    public int Month { get; init; }

    [Required]
    public Dictionary<int, string> DailyAttendance { get; init; } = new();

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!DailyAttendance.Any())
        {
            errors.Add("At least one day of attendance is required");
            return errors;
        }

        var daysInMonth = DateTime.DaysInMonth(Year, Month);
        var validStatuses = new[] { "X", "F", "P", "DD", "PD", "" };

        foreach (var (day, status) in DailyAttendance)
        {
            if (day < 1 || day > daysInMonth)
                errors.Add($"Invalid day {day} for month {Month}/{Year}");

            if (!validStatuses.Contains(status))
                errors.Add($"Invalid attendance status '{status}' for day {day}. Valid: X, F, P, DD, PD");
        }

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}
