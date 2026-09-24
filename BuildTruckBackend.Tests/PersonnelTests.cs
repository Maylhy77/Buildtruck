using BuildTruckPersonnelService.Personnel.Application.ACL.Services;
using BuildTruckPersonnelService.Personnel.Application.Internal.CommandServices;
using BuildTruckPersonnelService.Personnel.Application.Internal.QueryServices;
using BuildTruckPersonnelService.Personnel.Domain.Model.Commands;
using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;
using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckShared.Domain.Repositories;
using Xunit;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckBackend.Tests;

public class PersonnelTests
{
    [Fact]
    public void PersonnelAggregate_UpdatesAttendance_AndCalculatesMonthlyTotals()
    {
        var personnel = new PersonnelEntity(
            projectId: 10,
            name: "Carlos",
            lastname: "Mendoza",
            documentNumber: "12345678",
            position: "Foreman",
            department: "Construction",
            personnelType: PersonnelType.TECHNICAL,
            status: PersonnelStatus.ACTIVE);
        personnel.UpdateFinancialInfo(3000m, 0m, "BCP", "0011223344");

        personnel.SetDayAttendance(2026, 1, 5, AttendanceStatus.X);
        personnel.SetDayAttendance(2026, 1, 6, AttendanceStatus.P);
        personnel.SetDayAttendance(2026, 1, 7, AttendanceStatus.PD);
        personnel.SetDayAttendance(2026, 1, 8, AttendanceStatus.F);

        Assert.Equal(AttendanceStatus.X, personnel.GetDayAttendance(2026, 1, 5));
        Assert.Equal(1, personnel.WorkedDays);
        Assert.Equal(1, personnel.CompensatoryDays);
        Assert.Equal(1, personnel.UnpaidLeave);
        Assert.Equal(1, personnel.Absences);
        Assert.True(personnel.Sundays > 0);
        Assert.Equal(0m, personnel.TotalAmount);
    }

    [Fact]
    public async Task PersonnelCommandService_CreatesPersonnel_WhenProjectExists_AndNotifiesOnce()
    {
        var repository = new FakePersonnelRepository();
        var unitOfWork = new FakeUnitOfWork();
        var notifications = new FakeNotificationContextService();
        var service = new PersonnelCommandService(
            repository,
            unitOfWork,
            new FakeProjectContextService(projectExists: true),
            new FakeCloudinaryService(),
            notifications);

        var personnel = await service.Handle(new CreatePersonnelCommand(
            ProjectId: 22,
            Name: "Ana",
            Lastname: "Torres",
            DocumentNumber: "45678912",
            Position: "Safety supervisor",
            Department: "Safety",
            PersonnelType: PersonnelType.SPECIALIST,
            Status: PersonnelStatus.ACTIVE,
            MonthlyAmount: 4500m,
            Discount: 100m,
            Bank: "Interbank",
            AccountNumber: "9988776655",
            StartDate: new DateTime(2026, 1, 10),
            EndDate: null,
            Phone: "999888777",
            Email: "ana.torres@buildtruck.com",
            AvatarUrl: "https://res.cloudinary.com/demo/personnel/ana.jpg"));

        Assert.NotNull(personnel);
        Assert.Equal(1, personnel.Id);
        Assert.Equal(22, personnel.ProjectId);
        Assert.Equal("Ana Torres", personnel.GetFullName());
        Assert.Equal("ana.torres@buildtruck.com", personnel.Email);
        Assert.Single(repository.Personnel);
        Assert.Equal(1, unitOfWork.CompleteCalls);
        Assert.Equal(1, notifications.AddedCalls);
    }

    [Fact]
    public async Task PersonnelCommandService_RejectsDuplicateDocumentWithinProject()
    {
        var repository = new FakePersonnelRepository();
        await repository.AddAsync(new PersonnelEntity(
            projectId: 4,
            name: "Existing",
            lastname: "Worker",
            documentNumber: "11112222",
            position: "Operator",
            department: "Machinery",
            personnelType: PersonnelType.RENTED_OPERATOR,
            status: PersonnelStatus.ACTIVE));

        var service = new PersonnelCommandService(
            repository,
            new FakeUnitOfWork(),
            new FakeProjectContextService(projectExists: true),
            new FakeCloudinaryService(),
            new FakeNotificationContextService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new CreatePersonnelCommand(
                ProjectId: 4,
                Name: "Duplicate",
                Lastname: "Worker",
                DocumentNumber: "11112222",
                Position: "Assistant",
                Department: "Machinery",
                PersonnelType: PersonnelType.LABORER,
                Status: PersonnelStatus.ACTIVE,
                MonthlyAmount: 2500m,
                Discount: 0m,
                Bank: "BBVA",
                AccountNumber: "123123123",
                StartDate: null,
                EndDate: null,
                Phone: "",
                Email: "",
                AvatarUrl: null)));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task PersonnelCommandService_UpdatesAttendanceBatch_AndPersistsOnce()
    {
        var repository = new FakePersonnelRepository();
        var unitOfWork = new FakeUnitOfWork();
        await repository.AddAsync(new PersonnelEntity(
            projectId: 8,
            name: "Luis",
            lastname: "Vega",
            documentNumber: "87654321",
            position: "Worker",
            department: "Concrete",
            personnelType: PersonnelType.LABORER,
            status: PersonnelStatus.ACTIVE));

        var service = new PersonnelCommandService(
            repository,
            unitOfWork,
            new FakeProjectContextService(projectExists: true),
            new FakeCloudinaryService(),
            new FakeNotificationContextService());

        var success = await service.Handle(new UpdateAttendanceCommand([
            new PersonnelAttendanceUpdate(
                PersonnelId: 1,
                Year: 2026,
                Month: 2,
                DailyAttendance: new Dictionary<int, AttendanceStatus>
                {
                    [2] = AttendanceStatus.X,
                    [3] = AttendanceStatus.X,
                    [4] = AttendanceStatus.F
                })
        ]));

        var personnel = await repository.FindByIdAsync(1);

        Assert.True(success);
        Assert.NotNull(personnel);
        Assert.Equal(2, personnel.WorkedDays);
        Assert.Equal(1, personnel.Absences);
        Assert.Equal(1, repository.AttendanceBatchCalls);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task PersonnelQueryService_ReturnsOnlyActivePersonnelByProject()
    {
        var repository = new FakePersonnelRepository();
        await repository.AddAsync(new PersonnelEntity(5, "Active", "Worker", "100", "Worker", "Field",
            PersonnelType.LABORER, PersonnelStatus.ACTIVE));
        await repository.AddAsync(new PersonnelEntity(5, "Inactive", "Worker", "101", "Worker", "Field",
            PersonnelType.LABORER, PersonnelStatus.INACTIVE));
        await repository.AddAsync(new PersonnelEntity(6, "Other", "Project", "102", "Worker", "Field",
            PersonnelType.LABORER, PersonnelStatus.ACTIVE));

        var service = new PersonnelQueryService(repository);

        var active = (await service.GetActivePersonnelByProjectAsync(5)).ToList();
        var departments = (await service.GetDepartmentsByProjectAsync(5)).ToList();

        var item = Assert.Single(active);
        Assert.Equal("Active Worker", item.GetFullName());
        Assert.Single(departments);
        Assert.Equal("Field", departments[0]);
    }

    private static void SetEntityId(object entity, int id)
    {
        var backingField = entity.GetType().GetField(
            "<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CompleteCalls { get; private set; }

        public Task CompleteAsync()
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjectContextService(bool projectExists) : IProjectContextService
    {
        public Task<bool> ProjectExistsAsync(int projectId) => Task.FromResult(projectExists);

        public Task<string?> GetProjectNameAsync(int projectId) =>
            Task.FromResult<string?>(projectExists ? "Test Project" : null);

        public Task<bool> UserCanAccessProjectAsync(int userId, int projectId) =>
            Task.FromResult(projectExists);
    }

    private sealed class FakeCloudinaryService : ICloudinaryService
    {
        public int DeleteCalls { get; private set; }

        public Task<string> UploadPersonnelPhotoAsync(byte[] imageBytes, string fileName, int personnelId) =>
            Task.FromResult($"https://res.cloudinary.com/demo/personnel/{personnelId}-{fileName}");

        public Task<bool> DeletePersonnelPhotoAsync(string imageUrl)
        {
            DeleteCalls++;
            return Task.FromResult(true);
        }

        public string GetOptimizedPhotoUrl(string imageUrl, int width = 200, int height = 200) =>
            $"{imageUrl}?w={width}&h={height}";
    }

    private sealed class FakeNotificationContextService : INotificationContextService
    {
        public int AddedCalls { get; private set; }
        public int UpdatedCalls { get; private set; }
        public int RemovedCalls { get; private set; }

        public Task NotifyPersonnelAddedAsync(int projectId, string personnelName, int personnelId)
        {
            AddedCalls++;
            return Task.CompletedTask;
        }

        public Task NotifyPersonnelUpdatedAsync(int projectId, string personnelName)
        {
            UpdatedCalls++;
            return Task.CompletedTask;
        }

        public Task NotifyPersonnelRemovedAsync(int projectId, string personnelName)
        {
            RemovedCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePersonnelRepository : IPersonnelRepository
    {
        private int _nextId = 1;

        public List<PersonnelEntity> Personnel { get; } = [];
        public int AttendanceBatchCalls { get; private set; }

        public Task AddAsync(PersonnelEntity entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);

            Personnel.Add(entity);
            return Task.CompletedTask;
        }

        public Task<PersonnelEntity?> FindByIdAsync(int id) =>
            Task.FromResult(Personnel.FirstOrDefault(person => person.Id == id && !person.IsDeleted));

        public void Update(PersonnelEntity entity)
        {
        }

        public void Remove(PersonnelEntity entity) => Personnel.Remove(entity);

        public Task<IEnumerable<PersonnelEntity>> ListAsync() =>
            Task.FromResult<IEnumerable<PersonnelEntity>>(Personnel.Where(person => !person.IsDeleted));

        public Task<IEnumerable<PersonnelEntity>> FindByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<PersonnelEntity>>(
                Personnel.Where(person => person.BelongsToProject(projectId)));

        public Task<IEnumerable<PersonnelEntity>> FindByProjectIdWithAttendanceAsync(int projectId, int year, int month) =>
            FindByProjectIdAsync(projectId);

        public Task<PersonnelEntity?> FindByDocumentNumberAsync(string documentNumber, int projectId) =>
            Task.FromResult(Personnel.FirstOrDefault(person =>
                person.ProjectId == projectId &&
                person.DocumentNumber == documentNumber &&
                !person.IsDeleted));

        public Task<PersonnelEntity?> FindByEmailAsync(string email, int projectId) =>
            Task.FromResult(Personnel.FirstOrDefault(person =>
                person.ProjectId == projectId &&
                string.Equals(person.Email, email, StringComparison.OrdinalIgnoreCase) &&
                !person.IsDeleted));

        public Task<bool> ExistsByDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null) =>
            Task.FromResult(Personnel.Any(person =>
                person.ProjectId == projectId &&
                person.DocumentNumber == documentNumber &&
                !person.IsDeleted &&
                person.Id != excludePersonnelId));

        public Task<bool> ExistsByEmailAsync(string email, int projectId, int? excludePersonnelId = null) =>
            Task.FromResult(Personnel.Any(person =>
                person.ProjectId == projectId &&
                string.Equals(person.Email, email, StringComparison.OrdinalIgnoreCase) &&
                !person.IsDeleted &&
                person.Id != excludePersonnelId));

        public Task<IEnumerable<PersonnelEntity>> FindActiveByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<PersonnelEntity>>(
                Personnel.Where(person => person.ProjectId == projectId && person.IsActive()));

        public Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<string>>(
                Personnel
                    .Where(person => person.ProjectId == projectId && !person.IsDeleted)
                    .Select(person => person.Department)
                    .Distinct()
                    .OrderBy(department => department));

        public Task<bool> UpdateAttendanceBatchAsync(IEnumerable<PersonnelEntity> personnelList)
        {
            AttendanceBatchCalls++;
            return Task.FromResult(true);
        }
    }
}
