using BuildTruckPersonnelService.Personnel.Interfaces.REST.Resources;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Interfaces.REST.Transform;

public static class PersonnelResourceFromEntityAssembler
{
    public static PersonnelResource ToResourceFromEntity(PersonnelEntity personnel, bool includeAttendanceData = false)
    {
        return new PersonnelResource(
            personnel.Id,
            personnel.ProjectId,
            personnel.Name,
            personnel.Lastname,
            personnel.GetFullName(),
            personnel.DocumentNumber,
            personnel.Position,
            personnel.Department,
            personnel.PersonnelType.ToString(),
            personnel.Status.ToString(),
            personnel.MonthlyAmount,
            personnel.TotalAmount,
            personnel.Discount,
            personnel.Bank,
            personnel.AccountNumber,
            personnel.StartDate,
            personnel.EndDate,
            personnel.Phone,
            personnel.Email,
            personnel.AvatarUrl,
            personnel.WorkedDays,
            personnel.CompensatoryDays,
            personnel.UnpaidLeave,
            personnel.Absences,
            personnel.Sundays,
            personnel.TotalDays,
            includeAttendanceData ? personnel.MonthlyAttendance : null,
            personnel.CreatedDate,
            personnel.UpdatedDate
        );
    }

    public static PersonnelResource ToResourceFromEntity(PersonnelEntity personnel)
        => ToResourceFromEntity(personnel, false);

    public static IEnumerable<PersonnelResource> ToResourceFromEntity(
        IEnumerable<PersonnelEntity> personnel, bool includeAttendanceData = false)
        => personnel.Select(p => ToResourceFromEntity(p, includeAttendanceData));

    public static IEnumerable<PersonnelResource> ToResourceFromEntity(IEnumerable<PersonnelEntity> personnel)
        => ToResourceFromEntity(personnel, false);
}
