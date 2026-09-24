using BuildTruckPersonnelService.Personnel.Application.ACL.Services;
using BuildTruckPersonnelService.Personnel.Domain.Model.Commands;
using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckPersonnelService.Personnel.Domain.Services;
using BuildTruckShared.Domain.Repositories;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Application.Internal.CommandServices;

public class PersonnelCommandService : IPersonnelCommandService
{
    private readonly IPersonnelRepository _personnelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectContextService _projectContextService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly INotificationContextService _notificationService;

    public PersonnelCommandService(
        IPersonnelRepository personnelRepository,
        IUnitOfWork unitOfWork,
        IProjectContextService projectContextService,
        ICloudinaryService cloudinaryService,
        INotificationContextService notificationService)
    {
        _personnelRepository = personnelRepository;
        _unitOfWork = unitOfWork;
        _projectContextService = projectContextService;
        _cloudinaryService = cloudinaryService;
        _notificationService = notificationService;
    }

    public async Task<PersonnelEntity?> Handle(CreatePersonnelCommand command)
    {
        var projectExists = await _projectContextService.ProjectExistsAsync(command.ProjectId);
        if (!projectExists)
            throw new ArgumentException($"Project with ID {command.ProjectId} does not exist");

        var documentExists = await _personnelRepository.ExistsByDocumentNumberAsync(
            command.DocumentNumber, command.ProjectId);
        if (documentExists)
            throw new InvalidOperationException($"Document number {command.DocumentNumber} already exists in this project");

        if (!string.IsNullOrEmpty(command.Email))
        {
            var emailExists = await _personnelRepository.ExistsByEmailAsync(command.Email, command.ProjectId);
            if (emailExists)
                throw new InvalidOperationException($"Email {command.Email} already exists in this project");
        }

        var personnel = new PersonnelEntity(
            command.ProjectId,
            command.Name,
            command.Lastname,
            command.DocumentNumber,
            command.Position,
            command.Department,
            command.PersonnelType,
            command.Status);

        personnel.UpdateFinancialInfo(command.MonthlyAmount, command.Discount, command.Bank, command.AccountNumber);
        personnel.UpdateContactInfo(command.Phone, command.Email);
        personnel.UpdateContractInfo(command.StartDate, command.EndDate, command.Status);

        if (!string.IsNullOrEmpty(command.AvatarUrl))
            personnel.UpdateAvatar(command.AvatarUrl);

        try
        {
            await _personnelRepository.AddAsync(personnel);
            await _unitOfWork.CompleteAsync();
            await _notificationService.NotifyPersonnelAddedAsync(personnel.ProjectId, personnel.GetFullName(), personnel.Id);
            return personnel;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create personnel: {ex.Message}");
        }
    }

    public async Task<PersonnelEntity?> Handle(UpdatePersonnelCommand command)
    {
        var personnel = await _personnelRepository.FindByIdAsync(command.PersonnelId);
        if (personnel == null)
            throw new ArgumentException($"Personnel with ID {command.PersonnelId} not found");

        if (!string.IsNullOrEmpty(command.Email) && command.Email != personnel.Email)
        {
            var emailExists = await _personnelRepository.ExistsByEmailAsync(
                command.Email, personnel.ProjectId, command.PersonnelId);
            if (emailExists)
                throw new InvalidOperationException($"Email {command.Email} already exists in this project");
        }

        try
        {
            personnel.UpdateBasicInfo(command.Name, command.Lastname, command.Position, command.Department);
            personnel.UpdateFinancialInfo(command.MonthlyAmount, command.Discount, command.Bank, command.AccountNumber);
            personnel.UpdateContactInfo(command.Phone, command.Email);
            personnel.UpdateContractInfo(command.StartDate, command.EndDate, command.Status);
            personnel.UpdatePersonnelType(command.PersonnelType);

            if (!string.IsNullOrEmpty(command.AvatarUrl))
                personnel.UpdateAvatar(command.AvatarUrl);

            _personnelRepository.Update(personnel);
            await _unitOfWork.CompleteAsync();
            await _notificationService.NotifyPersonnelUpdatedAsync(personnel.ProjectId, personnel.GetFullName());
            return personnel;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update personnel: {ex.Message}");
        }
    }

    public async Task<bool> Handle(UpdateAttendanceCommand command)
    {
        if (!command.PersonnelAttendances.Any())
            return true;

        var personnelIds = command.PersonnelAttendances.Select(p => p.PersonnelId).ToList();
        var personnelList = new List<PersonnelEntity>();

        foreach (var personnelId in personnelIds)
        {
            var personnel = await _personnelRepository.FindByIdAsync(personnelId);
            if (personnel == null)
                throw new ArgumentException($"Personnel with ID {personnelId} not found");

            personnelList.Add(personnel);
        }

        try
        {
            foreach (var attendanceUpdate in command.PersonnelAttendances)
            {
                var personnel = personnelList.First(p => p.Id == attendanceUpdate.PersonnelId);

                personnel.InitializeMonthAttendance(attendanceUpdate.Year, attendanceUpdate.Month);
                personnel.AutoMarkSundays(attendanceUpdate.Year, attendanceUpdate.Month);

                foreach (var dailyAttendance in attendanceUpdate.DailyAttendance)
                {
                    personnel.SetDayAttendance(
                        attendanceUpdate.Year,
                        attendanceUpdate.Month,
                        dailyAttendance.Key,
                        dailyAttendance.Value);
                }

                personnel.CalculateMonthlyTotals(attendanceUpdate.Year, attendanceUpdate.Month);
            }

            var success = await _personnelRepository.UpdateAttendanceBatchAsync(personnelList);
            if (success)
                await _unitOfWork.CompleteAsync();

            return success;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update attendance: {ex.Message}");
        }
    }

    public async Task<bool> Handle(DeletePersonnelCommand command)
    {
        var personnel = await _personnelRepository.FindByIdAsync(command.PersonnelId);
        if (personnel == null)
            return false;

        try
        {
            personnel.SoftDelete();
            _personnelRepository.Update(personnel);
            await _unitOfWork.CompleteAsync();

            if (!string.IsNullOrEmpty(personnel.AvatarUrl))
            {
                try { await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl); }
                catch { }
            }

            await _notificationService.NotifyPersonnelRemovedAsync(personnel.ProjectId, personnel.GetFullName());
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete personnel: {ex.Message}");
        }
    }
}
