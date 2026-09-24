using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckMachineryService.Machinery.Application.Internal.CommandServices;
using BuildTruckMachineryService.Machinery.Application.Internal.QueryServices;
using BuildTruckMachineryService.Machinery.Domain.Model.Commands;
using BuildTruckMachineryService.Machinery.Domain.Model.Queries;
using BuildTruckMachineryService.Machinery.Domain.Model.ValueObjects;
using BuildTruckMachineryService.Machinery.Domain.Repositories;
using BuildTruckMachineryService.Machinery.Interfaces.REST.Transform;
using BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;
using BuildTruckShared.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using MachineryAggregate = BuildTruckMachineryService.Machinery.Domain.Model.Aggregates.Machinery;

namespace BuildTruckBackend.Tests;

public class MachineryTests
{
    [Fact]
    public void MachineryAggregate_ReportsStatusCaseInsensitive()
    {
        var machinery = new MachineryAggregate { Status = "maintenance" };

        Assert.True(machinery.IsInMaintenance());

        machinery.Status = "ACTIVE";

        Assert.True(machinery.IsActive());
    }

    [Fact]
    public void MachineryResourceAssembler_MapsAggregateToResource()
    {
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var registerDate = DateTime.UtcNow.Date;
        var machinery = new MachineryAggregate
        {
            Id = 15,
            ProjectId = 4,
            Name = "Excavadora CAT",
            LicensePlate = "MCH-001",
            MachineryType = "Excavadora",
            Status = "Maintenance",
            Provider = "Ferreyros",
            Description = "Mantenimiento preventivo",
            PersonnelId = 9,
            ImageUrl = "https://res.cloudinary.com/demo/machinery/mch-001.jpg",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            RegisterDate = registerDate
        };

        var resource = machinery.ToResource();

        Assert.Equal(15, resource.Id);
        Assert.Equal(4, resource.ProjectId);
        Assert.Equal("Excavadora CAT", resource.Name);
        Assert.Equal(MachineryStatus.Maintenance, resource.Status);
        Assert.Equal("https://res.cloudinary.com/demo/machinery/mch-001.jpg", resource.ImageUrl);
        Assert.Equal(registerDate, resource.RegisterDate);
    }

    [Fact]
    public async Task CreateMachineryCommandHandler_CreatesMachinery_UploadsImageAndPersists()
    {
        var repository = new FakeMachineryRepository();
        var unitOfWork = new FakeUnitOfWork();
        var cloudinary = new FakeMachineryCloudinaryService();
        var projectFacade = new FakeProjectFacade(existingProjectIds: [7]);
        var handler = new CreateMachineryCommandHandler(
            repository,
            unitOfWork,
            cloudinary,
            projectFacade,
            NullLogger<CreateMachineryCommandHandler>.Instance);

        var machinery = await handler.Handle(new CreateMachineryCommand(
            ProjectId: 7,
            Name: "Grua torre",
            LicensePlate: "GRU-777",
            MachineryType: "Grua",
            Status: MachineryStatus.Active,
            Provider: "Proveedor Andino",
            Description: "Equipo asignado a obra principal",
            PersonnelId: 22,
            RegisterDate: DateTime.Today,
            ImageBytes: [1, 2, 3],
            ImageFileName: "grua.jpg"));

        Assert.Equal(1, machinery.Id);
        Assert.Equal("Active", machinery.Status);
        Assert.Equal("https://res.cloudinary.com/demo/machinery/grua.jpg", machinery.ImageUrl);
        Assert.Single(repository.Machinery);
        Assert.Equal(1, cloudinary.ByteUploadCalls);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task CreateMachineryCommandHandler_RejectsDuplicateLicensePlateForProject()
    {
        var repository = new FakeMachineryRepository();
        await repository.AddAsync(new MachineryAggregate
        {
            ProjectId = 3,
            Name = "Retroexcavadora",
            LicensePlate = "RET-123",
            MachineryType = "Retroexcavadora",
            Status = "Active"
        });
        var handler = new CreateMachineryCommandHandler(
            repository,
            new FakeUnitOfWork(),
            new FakeMachineryCloudinaryService(),
            new FakeProjectFacade(existingProjectIds: [3]),
            NullLogger<CreateMachineryCommandHandler>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateMachineryCommand(
                ProjectId: 3,
                Name: "Retroexcavadora duplicada",
                LicensePlate: "RET-123",
                MachineryType: "Retroexcavadora",
                Status: MachineryStatus.Active,
                Provider: "Proveedor",
                Description: "Duplicada",
                PersonnelId: null,
                RegisterDate: DateTime.Today,
                ImageBytes: null,
                ImageFileName: null)));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateMachineryCommandHandler_ReplacesImage_WhenNewFileIsProvided()
    {
        var repository = new FakeMachineryRepository();
        await repository.AddAsync(new MachineryAggregate
        {
            ProjectId = 8,
            Name = "Cargador frontal",
            LicensePlate = "CAR-800",
            MachineryType = "Cargador",
            Status = "Active",
            ImageUrl = "https://res.cloudinary.com/demo/machinery/old-car-800.jpg"
        });
        var unitOfWork = new FakeUnitOfWork();
        var cloudinary = new FakeMachineryCloudinaryService();
        var handler = new UpdateMachineryCommandHandler(
            repository,
            unitOfWork,
            cloudinary,
            new FakeProjectFacade(existingProjectIds: [8]),
            NullLogger<UpdateMachineryCommandHandler>.Instance);

        await using var stream = new MemoryStream([4, 5, 6]);
        var image = new FormFile(stream, 0, stream.Length, "image", "updated.jpg");

        var updated = await handler.Handle(new UpdateMachineryCommand(
            Id: 1,
            ProjectId: 8,
            Name: "Cargador frontal actualizado",
            LicensePlate: "CAR-801",
            MachineryType: "Cargador",
            Status: MachineryStatus.Maintenance,
            Provider: "Proveedor Norte",
            Description: "Cambio a mantenimiento",
            PersonnelId: 14), image);

        Assert.Equal("Cargador frontal actualizado", updated.Name);
        Assert.Equal("Maintenance", updated.Status);
        Assert.Equal("https://res.cloudinary.com/demo/machinery/updated.jpg", updated.ImageUrl);
        Assert.Equal(["old-car-800"], cloudinary.DeletedPublicIds);
        Assert.Equal(1, cloudinary.FormFileUploadCalls);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task DeleteMachineryCommandHandler_RemovesMachineryAndDeletesImage()
    {
        var repository = new FakeMachineryRepository();
        await repository.AddAsync(new MachineryAggregate
        {
            ProjectId = 2,
            Name = "Mixer",
            LicensePlate = "MIX-222",
            MachineryType = "Mixer",
            Status = "Inactive",
            ImageUrl = "https://res.cloudinary.com/demo/machinery/mix-222.jpg"
        });
        var unitOfWork = new FakeUnitOfWork();
        var cloudinary = new FakeMachineryCloudinaryService();
        var handler = new DeleteMachineryCommandHandler(
            repository,
            unitOfWork,
            cloudinary,
            NullLogger<DeleteMachineryCommandHandler>.Instance);

        await handler.Handle(new DeleteMachineryCommand(1));

        Assert.Empty(repository.Machinery);
        Assert.Equal(["mix-222"], cloudinary.DeletedPublicIds);
        Assert.Equal(1, unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task MachineryQueryHandlers_ReturnProjectMachineryAndActiveSubset()
    {
        var repository = new FakeMachineryRepository();
        await repository.AddAsync(new MachineryAggregate { ProjectId = 5, Name = "Excavadora", LicensePlate = "EXC-1", Status = "Active" });
        await repository.AddAsync(new MachineryAggregate { ProjectId = 5, Name = "Mixer", LicensePlate = "MIX-1", Status = "Maintenance" });
        await repository.AddAsync(new MachineryAggregate { ProjectId = 6, Name = "Grua", LicensePlate = "GRU-1", Status = "Active" });

        var byProject = await new GetMachineryByProjectQueryHandler(repository)
            .Handle(new GetMachineryByProjectQuery(5));
        var active = await new GetActiveMachineryQueryHandler(repository)
            .Handle(new GetActiveMachineryQuery(5));

        Assert.Equal(2, byProject.Count());
        Assert.Single(active);
        Assert.Equal("Excavadora", active.Single().Name);
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

    private sealed class FakeProjectFacade : IProjectFacade
    {
        private readonly HashSet<int> _existingProjectIds;

        public FakeProjectFacade(IEnumerable<int> existingProjectIds)
        {
            _existingProjectIds = existingProjectIds.ToHashSet();
        }

        public Task<bool> ExistsByIdAsync(int projectId) => Task.FromResult(_existingProjectIds.Contains(projectId));

        public Task<ProjectInfo?> GetProjectByIdAsync(int projectId) =>
            Task.FromResult(_existingProjectIds.Contains(projectId) ? new ProjectInfo { Id = projectId } : null);

        public Task<bool> UserHasAccessToProjectAsync(int userId, int projectId) =>
            Task.FromResult(_existingProjectIds.Contains(projectId));
    }

    private sealed class FakeMachineryCloudinaryService : IMachineryCloudinaryService
    {
        public int ByteUploadCalls { get; private set; }
        public int FormFileUploadCalls { get; private set; }
        public List<string> DeletedPublicIds { get; } = [];

        public Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            ByteUploadCalls++;
            return Task.FromResult($"https://res.cloudinary.com/demo/machinery/{fileName}");
        }

        public Task<string> UploadImageAsync(IFormFile imageFile)
        {
            FormFileUploadCalls++;
            return Task.FromResult($"https://res.cloudinary.com/demo/machinery/{imageFile.FileName}");
        }

        public Task<bool> DeleteImageAsync(string publicId)
        {
            DeletedPublicIds.Add(publicId);
            return Task.FromResult(true);
        }

        public string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200) =>
            $"https://res.cloudinary.com/demo/image/upload/c_fill,w_{width},h_{height}/{publicId}.jpg";

        public string ExtractPublicIdFromUrl(string cloudinaryUrl)
        {
            var fileName = Path.GetFileNameWithoutExtension(new Uri(cloudinaryUrl).AbsolutePath);
            return fileName;
        }
    }

    private sealed class FakeMachineryRepository : IMachineryRepository
    {
        private int _nextId = 1;
        public List<MachineryAggregate> Machinery { get; } = [];
        public int UpdateCalls { get; private set; }

        public Task AddAsync(MachineryAggregate entity)
        {
            if (entity.Id == 0)
                entity.Id = _nextId++;
            Machinery.Add(entity);
            return Task.CompletedTask;
        }

        public Task<MachineryAggregate?> FindByIdAsync(int id) =>
            Task.FromResult(Machinery.FirstOrDefault(machinery => machinery.Id == id));

        public Task<IEnumerable<MachineryAggregate>> FindByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<MachineryAggregate>>(
                Machinery.Where(machinery => machinery.ProjectId == projectId));

        public Task<MachineryAggregate?> FindByLicensePlateAsync(string licensePlate, int projectId) =>
            Task.FromResult(Machinery.FirstOrDefault(machinery =>
                machinery.ProjectId == projectId &&
                machinery.LicensePlate.Equals(licensePlate, StringComparison.OrdinalIgnoreCase)));

        public Task<IEnumerable<MachineryAggregate>> ListAsync() =>
            Task.FromResult<IEnumerable<MachineryAggregate>>(Machinery);

        public void Remove(MachineryAggregate entity) => Machinery.Remove(entity);

        public void Update(MachineryAggregate entity)
        {
            UpdateCalls++;
        }

        public Task UpdateAsync(MachineryAggregate machinery)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }
    }
}
