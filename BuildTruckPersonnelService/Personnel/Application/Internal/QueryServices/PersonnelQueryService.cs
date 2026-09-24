using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckPersonnelService.Personnel.Domain.Services;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Application.Internal.QueryServices;

public class PersonnelQueryService : IPersonnelQueryService
{
    private readonly IPersonnelRepository _personnelRepository;

    public PersonnelQueryService(IPersonnelRepository personnelRepository)
    {
        _personnelRepository = personnelRepository;
    }

    public async Task<IEnumerable<PersonnelEntity>> GetPersonnelByProjectAsync(int projectId)
    {
        return await _personnelRepository.FindByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<PersonnelEntity>> GetPersonnelWithAttendanceAsync(
        int projectId, int year, int month, bool includeAttendance = true)
    {
        if (!includeAttendance)
            return await _personnelRepository.FindByProjectIdAsync(projectId);

        var personnel = await _personnelRepository.FindByProjectIdWithAttendanceAsync(projectId, year, month);

        var personnelList = personnel.ToList();
        foreach (var person in personnelList)
        {
            person.InitializeMonthAttendance(year, month);
            person.AutoMarkSundays(year, month);
            person.CalculateMonthlyTotals(year, month);
        }

        return personnelList;
    }

    public async Task<PersonnelEntity?> GetPersonnelByIdAsync(int personnelId)
    {
        return await _personnelRepository.FindByIdAsync(personnelId);
    }

    public async Task<IEnumerable<PersonnelEntity>> GetActivePersonnelByProjectAsync(int projectId)
    {
        return await _personnelRepository.FindActiveByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<string>> GetDepartmentsByProjectAsync(int projectId)
    {
        return await _personnelRepository.GetDepartmentsByProjectIdAsync(projectId);
    }

    public async Task<bool> ValidateDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null)
    {
        return await _personnelRepository.ExistsByDocumentNumberAsync(documentNumber, projectId, excludePersonnelId);
    }

    public async Task<bool> ValidateEmailAsync(string email, int projectId, int? excludePersonnelId = null)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return await _personnelRepository.ExistsByEmailAsync(email, projectId, excludePersonnelId);
    }
}
