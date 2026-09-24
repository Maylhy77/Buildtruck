using System.ComponentModel.DataAnnotations;

namespace BuildTruckPersonnelService.Personnel.Interfaces.REST.Resources;

public class UpdateAttendanceResource
{
    [Required]
    public List<PersonnelAttendanceResource> PersonnelAttendances { get; init; } = new();

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!PersonnelAttendances.Any())
        {
            errors.Add("At least one personnel attendance is required");
            return errors;
        }

        foreach (var attendance in PersonnelAttendances)
            errors.AddRange(attendance.GetValidationErrors());

        var duplicateIds = PersonnelAttendances
            .GroupBy(p => p.PersonnelId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateId in duplicateIds)
            errors.Add($"Duplicate personnel ID: {duplicateId}");

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}
