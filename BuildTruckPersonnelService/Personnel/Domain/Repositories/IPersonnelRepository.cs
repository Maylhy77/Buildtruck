using BuildTruckShared.Domain.Repositories;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Domain.Repositories;

public interface IPersonnelRepository : IBaseRepository<PersonnelEntity>
{
    Task<IEnumerable<PersonnelEntity>> FindByProjectIdAsync(int projectId);

    Task<IEnumerable<PersonnelEntity>> FindByProjectIdWithAttendanceAsync(int projectId, int year, int month);

    Task<PersonnelEntity?> FindByDocumentNumberAsync(string documentNumber, int projectId);

    Task<PersonnelEntity?> FindByEmailAsync(string email, int projectId);

    Task<bool> ExistsByDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null);

    Task<bool> ExistsByEmailAsync(string email, int projectId, int? excludePersonnelId = null);

    Task<IEnumerable<PersonnelEntity>> FindActiveByProjectIdAsync(int projectId);

    Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId);

    Task<bool> UpdateAttendanceBatchAsync(IEnumerable<PersonnelEntity> personnelList);
}
