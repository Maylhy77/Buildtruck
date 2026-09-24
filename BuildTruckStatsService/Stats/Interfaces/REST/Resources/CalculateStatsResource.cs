namespace BuildTruckStatsService.Stats.Interfaces.REST.Resources;

public record CalculateStatsResource(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool ForceRecalculation = false,
    bool SaveHistory = true,
    string? Notes = null)
{
    public bool IsValid()
    {
        if (StartDate.HasValue && EndDate.HasValue)
            return StartDate.Value <= EndDate.Value;
        return true;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            errors.Add("Start date cannot be after end date");
        if (StartDate.HasValue && StartDate.Value > DateTime.Now)
            errors.Add("Start date cannot be in the future");
        if (EndDate.HasValue && EndDate.Value > DateTime.Now)
            errors.Add("End date cannot be in the future");
        return errors;
    }
}
