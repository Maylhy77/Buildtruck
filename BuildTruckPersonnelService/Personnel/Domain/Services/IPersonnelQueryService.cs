using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Domain.Services;

public interface IPersonnelQueryService
{
    Task<IEnumerable<PersonnelEntity>> GetPersonnelByProjectAsync(int projectId);

    Task<IEnumerable<PersonnelEntity>> GetPersonnelWithAttendanceAsync(
        int projectId, int year, int month, bool includeAttendance = true);

    Task<PersonnelEntity?> GetPersonnelByIdAsync(int personnelId);

    Task<IEnumerable<PersonnelEntity>> GetActivePersonnelByProjectAsync(int projectId);

    Task<IEnumerable<string>> GetDepartmentsByProjectAsync(int projectId);

    Task<bool> ValidateDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null);

    Task<bool> ValidateEmailAsync(string email, int projectId, int? excludePersonnelId = null);
}
