using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;

namespace BuildTruckPersonnelService.Personnel.Domain.Model.Commands;

public record UpdateAttendanceCommand(
    List<PersonnelAttendanceUpdate> PersonnelAttendances
);

public record PersonnelAttendanceUpdate(
    int PersonnelId,
    int Year,
    int Month,
    Dictionary<int, AttendanceStatus> DailyAttendance
);
