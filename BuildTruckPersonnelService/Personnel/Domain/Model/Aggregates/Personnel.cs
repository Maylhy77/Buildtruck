using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;

namespace BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;

public partial class Personnel
{
    public int Id { get; private set; }

    public int ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Lastname { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public PersonnelType PersonnelType { get; private set; }

    public decimal MonthlyAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal Discount { get; private set; }
    public string Bank { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public PersonnelStatus Status { get; private set; }

    public int WorkedDays { get; private set; }
    public int CompensatoryDays { get; private set; }
    public int UnpaidLeave { get; private set; }
    public int Absences { get; private set; }
    public int Sundays { get; private set; }
    public int TotalDays { get; private set; }

    public string MonthlyAttendanceJson { get; private set; } = "{}";

    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }

    public bool IsDeleted { get; private set; }

    public Dictionary<string, string> MonthlyAttendance
    {
        get => string.IsNullOrEmpty(MonthlyAttendanceJson)
            ? new Dictionary<string, string>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(MonthlyAttendanceJson) ?? new Dictionary<string, string>();
        private set => MonthlyAttendanceJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    public Personnel() { }

    public Personnel(int projectId, string name, string lastname, string documentNumber,
        string position, string department, PersonnelType personnelType, PersonnelStatus status)
    {
        ProjectId = projectId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
        DocumentNumber = documentNumber ?? throw new ArgumentNullException(nameof(documentNumber));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Department = department ?? throw new ArgumentNullException(nameof(department));
        PersonnelType = personnelType;
        Status = status;
        MonthlyAttendance = new Dictionary<string, string>();
        IsDeleted = false;
    }

    public void UpdateBasicInfo(string name, string lastname, string position, string department)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Department = department ?? throw new ArgumentNullException(nameof(department));
    }

    public void UpdateFinancialInfo(decimal monthlyAmount, decimal discount, string bank, string accountNumber)
    {
        MonthlyAmount = monthlyAmount;
        Discount = discount;
        Bank = bank ?? string.Empty;
        AccountNumber = accountNumber ?? string.Empty;
        CalculateTotalAmount();
    }

    public void UpdateContactInfo(string phone, string email)
    {
        Phone = phone ?? string.Empty;
        Email = email ?? string.Empty;
    }

    public void UpdateContractInfo(DateTime? startDate, DateTime? endDate, PersonnelStatus status)
    {
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

    public void UpdatePersonnelType(PersonnelType personnelType)
    {
        PersonnelType = personnelType;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    public void InitializeMonthAttendance(int year, int month)
    {
        var monthKey = GetMonthKey(year, month);
        var attendance = MonthlyAttendance;

        if (!attendance.ContainsKey(monthKey))
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var attendanceArray = new string[daysInMonth];
            for (int i = 0; i < daysInMonth; i++)
                attendanceArray[i] = string.Empty;

            attendance[monthKey] = string.Join("|", attendanceArray);
            MonthlyAttendance = attendance;
        }
    }

    public void SetDayAttendance(int year, int month, int day, AttendanceStatus status)
    {
        var monthKey = GetMonthKey(year, month);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        if (day < 1 || day > daysInMonth)
            throw new ArgumentException($"Invalid day: {day} for month {month}/{year}");

        InitializeMonthAttendance(year, month);

        var attendance = MonthlyAttendance;
        var attendanceArray = attendance[monthKey].Split('|');

        if (attendanceArray.Length != daysInMonth)
            Array.Resize(ref attendanceArray, daysInMonth);

        attendanceArray[day - 1] = status.ToString();
        attendance[monthKey] = string.Join("|", attendanceArray);
        MonthlyAttendance = attendance;

        CalculateMonthlyTotals(year, month);
    }

    public AttendanceStatus GetDayAttendance(int year, int month, int day)
    {
        var monthKey = GetMonthKey(year, month);
        var attendance = MonthlyAttendance;

        if (!attendance.ContainsKey(monthKey))
        {
            var date = new DateTime(year, month, day);
            return date.DayOfWeek == DayOfWeek.Sunday ? AttendanceStatus.DD : AttendanceStatus.Empty;
        }

        var attendanceArray = attendance[monthKey].Split('|');
        if (day < 1 || day > attendanceArray.Length)
            return AttendanceStatus.Empty;

        var statusStr = attendanceArray[day - 1];

        if (string.IsNullOrEmpty(statusStr))
        {
            var date = new DateTime(year, month, day);
            return date.DayOfWeek == DayOfWeek.Sunday ? AttendanceStatus.DD : AttendanceStatus.Empty;
        }

        return Enum.TryParse<AttendanceStatus>(statusStr, out var result) ? result : AttendanceStatus.Empty;
    }

    public void AutoMarkSundays(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                var currentStatus = GetDayAttendance(year, month, day);
                if (currentStatus == AttendanceStatus.Empty)
                    SetDayAttendance(year, month, day, AttendanceStatus.DD);
            }
        }
    }

    public void CalculateMonthlyTotals(int year, int month)
    {
        var monthKey = GetMonthKey(year, month);
        var attendance = MonthlyAttendance;

        if (!attendance.ContainsKey(monthKey))
        {
            ResetMonthlyTotals();
            return;
        }

        var attendanceArray = attendance[monthKey].Split('|');
        var daysInMonth = DateTime.DaysInMonth(year, month);

        WorkedDays = 0;
        CompensatoryDays = 0;
        UnpaidLeave = 0;
        Absences = 0;
        Sundays = 0;

        for (int day = 1; day <= Math.Min(daysInMonth, attendanceArray.Length); day++)
        {
            var statusStr = attendanceArray[day - 1];

            if (string.IsNullOrEmpty(statusStr))
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    Sundays++;
            }
            else if (Enum.TryParse<AttendanceStatus>(statusStr, out var status))
            {
                switch (status)
                {
                    case AttendanceStatus.X: WorkedDays++; break;
                    case AttendanceStatus.P: CompensatoryDays++; break;
                    case AttendanceStatus.PD: UnpaidLeave++; break;
                    case AttendanceStatus.F: Absences++; break;
                    case AttendanceStatus.DD: Sundays++; break;
                }
            }
        }

        CalculateTotalDays();
        CalculateTotalAmount();
    }

    private void CalculateTotalDays()
    {
        TotalDays = WorkedDays + CompensatoryDays + UnpaidLeave + Absences + Sundays;
    }

    private void CalculateTotalAmount()
    {
        if (MonthlyAmount <= 0)
        {
            TotalAmount = 0;
            return;
        }

        var paidDays = WorkedDays + CompensatoryDays;
        var discountedDays = Absences + UnpaidLeave;
        var dailyRate = MonthlyAmount / 30;

        TotalAmount = Math.Max(0, (dailyRate * paidDays) - (dailyRate * discountedDays) - Discount);
    }

    private void ResetMonthlyTotals()
    {
        WorkedDays = 0;
        CompensatoryDays = 0;
        UnpaidLeave = 0;
        Absences = 0;
        Sundays = 0;
        TotalDays = 0;
        TotalAmount = 0;
    }

    private static string GetMonthKey(int year, int month) => $"{year}-{month:D2}";

    public bool IsActive() => Status == PersonnelStatus.ACTIVE && !IsDeleted;
    public string GetFullName() => $"{Name} {Lastname}".Trim();
    public bool BelongsToProject(int projectId) => ProjectId == projectId && !IsDeleted;
}
