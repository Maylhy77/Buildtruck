using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;

namespace BuildTruckPersonnelService.Personnel.Domain.Model.Commands;

public record CreatePersonnelCommand(
    int ProjectId,
    string Name,
    string Lastname,
    string DocumentNumber,
    string Position,
    string Department,
    PersonnelType PersonnelType,
    PersonnelStatus Status,
    decimal MonthlyAmount,
    decimal Discount,
    string Bank,
    string AccountNumber,
    DateTime? StartDate,
    DateTime? EndDate,
    string Phone,
    string Email,
    string? AvatarUrl
);
