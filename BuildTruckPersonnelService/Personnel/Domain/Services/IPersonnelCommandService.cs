using BuildTruckPersonnelService.Personnel.Domain.Model.Commands;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Personnel.Domain.Services;

public interface IPersonnelCommandService
{
    Task<PersonnelEntity?> Handle(CreatePersonnelCommand command);
    Task<PersonnelEntity?> Handle(UpdatePersonnelCommand command);
    Task<bool> Handle(UpdateAttendanceCommand command);
    Task<bool> Handle(DeletePersonnelCommand command);
}
