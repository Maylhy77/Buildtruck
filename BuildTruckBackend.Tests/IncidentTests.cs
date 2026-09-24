using BuildTruckIncidentService.Incidents.Application.ACL.Services;
using BuildTruckIncidentService.Incidents.Application.Internal;
using BuildTruckIncidentService.Incidents.Application.REST.Transform;
using BuildTruckIncidentService.Incidents.Domain.Aggregates;
using BuildTruckIncidentService.Incidents.Domain.Model.Commands;
using BuildTruckIncidentService.Incidents.Domain.Model.Queries;
using BuildTruckIncidentService.Incidents.Domain.Repositories;
using BuildTruckIncidentService.Incidents.Domain.ValueObjects;
using BuildTruckIncidentService.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildTruckBackend.Tests;

public class IncidentTests
{
    [Fact]
    public void IncidentValueObjects_ParseValidValues_AndRejectInvalidValues()
    {
        var severity = IncidentSeverityExtensions.FromString("high");
        var status = IncidentStatusExtensions.FromString("inprogress");

        Assert.Equal(IncidentSeverity.High, severity);
        Assert.Equal(IncidentStatus.InProgress, status);
        Assert.Throws<ArgumentException>(() => IncidentSeverityExtensions.FromString("urgent"));
        Assert.Throws<ArgumentException>(() => IncidentStatusExtensions.FromString("closed"));
    }

    [Fact]
    public void IncidentResourceAssembler_MapsAggregateToResource()
    {
        var incident = new Incident
        {
            Id = 15,
            ProjectId = 4,
            Title = "Concrete crack",
            Description = "Crack reported near column C4",
            IncidentType = "Structural",
            Severity = IncidentSeverity.High,
            Status = IncidentStatus.Reportado,
            Location = "North wing",
            ReportedBy = "7",
            AssignedTo = "9",
            OccurredAt = DateTime.Now.Date.AddHours(8),
            ResolvedAt = null,
            Image = "https://res.cloudinary.com/demo/incidents/crack.jpg",
            Notes = "Requires engineering inspection"
        };

        var resource = IncidentResourceAssembler.ToResource(incident);

        Assert.Equal(incident.Id, resource.Id);
        Assert.Equal(incident.ProjectId, resource.ProjectId);
        Assert.Equal("Concrete crack", resource.Title);
        Assert.Equal(IncidentSeverity.High, resource.Severity);
        Assert.Equal(IncidentStatus.Reportado, resource.Status);
        Assert.Equal("https://res.cloudinary.com/demo/incidents/crack.jpg", resource.Image);
    }

    [Fact]
    public async Task IncidentCommandHandler_CreatesIncident_AndUploadsImageWhenProvided()
    {
        await using var context = CreateContext();
        var cloudinary = new FakeCloudinaryService();
        var handler = new IncidentCommandHandler(context, cloudinary);
        var imagePath = CreateTempImageFile();

        try
        {
            var incidentId = await handler.HandleAsync(new CreateIncidentCommand(
                ProjectId: 20,
                Title: "Worker fall",
                Description: "Worker slipped near access ramp",
                IncidentType: "Safety",
                Severity: "High",
                Status: "Reportado",
                Location: "Access ramp",
                ReportedBy: "7",
                AssignedTo: "9",
                OccurredAt: DateTime.Now.Date.AddHours(10),
                Image: Path.GetFileName(imagePath),
                Notes: "Medical evaluation requested",
                ImagePath: imagePath));

            var incident = await context.Incidents.FindAsync(incidentId);

            Assert.NotNull(incident);
            Assert.Equal(20, incident.ProjectId);
            Assert.Equal(IncidentSeverity.High, incident.Severity);
            Assert.Equal(IncidentStatus.Reportado, incident.Status);
            Assert.Equal(1, cloudinary.UploadCalls);
            Assert.Contains(Path.GetFileName(imagePath), cloudinary.UploadedFileNames);
            Assert.Contains("https://res.cloudinary.com/demo/incidents/", incident.Image);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public async Task IncidentCommandHandler_CreatesIncidentWithoutImage_AndDoesNotUpload()
    {
        await using var context = CreateContext();
        var cloudinary = new FakeCloudinaryService();
        var handler = new IncidentCommandHandler(context, cloudinary);

        var incidentId = await handler.HandleAsync(new CreateIncidentCommand(
            ProjectId: 12,
            Title: "Loose cable",
            Description: "Loose electrical cable found near storage",
            IncidentType: "Safety",
            Severity: "Medio",
            Status: "Reportado",
            Location: "Storage",
            ReportedBy: "4",
            AssignedTo: null,
            OccurredAt: DateTime.Now.Date.AddHours(9),
            Image: null,
            Notes: "Needs immediate isolation",
            ImagePath: null));

        var incident = await context.Incidents.FindAsync(incidentId);

        Assert.NotNull(incident);
        Assert.Equal(12, incident.ProjectId);
        Assert.Equal("Loose cable", incident.Title);
        Assert.Null(incident.Image);
        Assert.Equal(0, cloudinary.UploadCalls);
    }

    [Fact]
    public async Task IncidentCommandHandler_UpdatesIncident_AndReplacesExistingImage()
    {
        await using var context = CreateContext();
        var incident = new Incident
        {
            ProjectId = 20,
            Title = "Initial incident",
            Description = "Initial description",
            IncidentType = "Safety",
            Severity = IncidentSeverity.Medio,
            Status = IncidentStatus.Reportado,
            Location = "Warehouse",
            Image = "https://res.cloudinary.com/demo/incidents/old.jpg",
            Notes = "Initial notes"
        };
        await context.Incidents.AddAsync(incident);
        await context.SaveChangesAsync();

        var cloudinary = new FakeCloudinaryService();
        var handler = new IncidentCommandHandler(context, cloudinary);
        var imagePath = CreateTempImageFile();

        try
        {
            await handler.HandleAsync(new UpdateIncidentCommand(
                Id: incident.Id,
                ProjectId: 20,
                Title: "Updated incident",
                Description: "Updated description",
                IncidentType: "Operational",
                Severity: "Low",
                Status: "Resolved",
                Location: "Warehouse door",
                ReportedBy: "7",
                AssignedTo: "10",
                OccurredAt: DateTime.Now.Date.AddHours(11),
                ResolvedAt: DateTime.Now.Date.AddHours(12),
                Image: Path.GetFileName(imagePath),
                Notes: "Resolved after inspection",
                ImagePath: imagePath));

            var updated = await context.Incidents.FindAsync(incident.Id);

            Assert.NotNull(updated);
            Assert.Equal("Updated incident", updated.Title);
            Assert.Equal(IncidentSeverity.Low, updated.Severity);
            Assert.Equal(IncidentStatus.Resolved, updated.Status);
            Assert.Equal(1, cloudinary.DeleteCalls);
            Assert.Equal(1, cloudinary.UploadCalls);
            Assert.DoesNotContain("old.jpg", updated.Image);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public async Task IncidentCommandHandler_UpdatesIncidentWithoutNewImage_PreservesExistingImage()
    {
        await using var context = CreateContext();
        var incident = new Incident
        {
            ProjectId = 20,
            Title = "Initial incident",
            Description = "Initial description",
            IncidentType = "Safety",
            Severity = IncidentSeverity.Medio,
            Status = IncidentStatus.Reportado,
            Location = "Warehouse",
            Image = "https://res.cloudinary.com/demo/incidents/current.jpg",
            Notes = "Initial notes"
        };
        await context.Incidents.AddAsync(incident);
        await context.SaveChangesAsync();

        var cloudinary = new FakeCloudinaryService();
        var handler = new IncidentCommandHandler(context, cloudinary);
        var resolvedAt = DateTime.Now.Date.AddHours(14);

        await handler.HandleAsync(new UpdateIncidentCommand(
            Id: incident.Id,
            ProjectId: 25,
            Title: "Updated without image",
            Description: "Description changed",
            IncidentType: "Operational",
            Severity: "High",
            Status: "Resolved",
            Location: "Warehouse aisle",
            ReportedBy: "7",
            AssignedTo: "11",
            OccurredAt: DateTime.Now.Date.AddHours(13),
            ResolvedAt: resolvedAt,
            Image: null,
            Notes: "Resolved without replacing evidence",
            ImagePath: null));

        var updated = await context.Incidents.FindAsync(incident.Id);

        Assert.NotNull(updated);
        Assert.Equal(25, updated.ProjectId);
        Assert.Equal("Updated without image", updated.Title);
        Assert.Equal(IncidentSeverity.High, updated.Severity);
        Assert.Equal(IncidentStatus.Resolved, updated.Status);
        Assert.Equal(resolvedAt, updated.ResolvedAt);
        Assert.Equal("https://res.cloudinary.com/demo/incidents/current.jpg", updated.Image);
        Assert.Equal(0, cloudinary.DeleteCalls);
        Assert.Equal(0, cloudinary.UploadCalls);
    }

    [Fact]
    public async Task IncidentCommandHandler_DeleteIncident_RemovesRecordAndDeletesImage()
    {
        await using var context = CreateContext();
        var incident = new Incident
        {
            ProjectId = 30,
            Title = "Incident to delete",
            Description = "Temporary incident",
            IncidentType = "Safety",
            Image = "https://res.cloudinary.com/demo/incidents/delete.jpg"
        };
        await context.Incidents.AddAsync(incident);
        await context.SaveChangesAsync();

        var cloudinary = new FakeCloudinaryService();
        var handler = new IncidentCommandHandler(context, cloudinary);

        await handler.DeleteAsync(incident.Id);

        Assert.Null(await context.Incidents.FindAsync(incident.Id));
        Assert.Equal(1, cloudinary.DeleteCalls);
    }

    [Fact]
    public async Task IncidentCommandHandler_RejectsInvalidSeverityDuringCreate()
    {
        await using var context = CreateContext();
        var handler = new IncidentCommandHandler(context, new FakeCloudinaryService());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new CreateIncidentCommand(
                ProjectId: 10,
                Title: "Invalid severity",
                Description: "Invalid severity should fail",
                IncidentType: "Safety",
                Severity: "Urgent",
                Status: "Reportado",
                Location: "Main gate",
                ReportedBy: null,
                AssignedTo: null,
                OccurredAt: DateTime.Now,
                Image: null,
                Notes: "Invalid test",
                ImagePath: null)));

        Assert.Contains("IncidentSeverity", exception.Message);
    }

    [Fact]
    public async Task IncidentQueryHandler_ReturnsIncidentsByIdAndProject()
    {
        var repository = new FakeIncidentRepository();
        await repository.AddAsync(new Incident { Id = 1, ProjectId = 5, Title = "Project 5 incident" });
        await repository.AddAsync(new Incident { Id = 2, ProjectId = 6, Title = "Project 6 incident" });
        await repository.AddAsync(new Incident { Id = 3, ProjectId = 5, Title = "Second project 5 incident" });

        var handler = new IncidentQueryHandler(repository);

        var found = await handler.HandleAsync(new GetIncidentByIdQuery(2));
        var projectIncidents = await handler.HandleAsync(new GetIncidentsByProjectIdQuery(5));

        Assert.NotNull(found);
        Assert.Equal("Project 6 incident", found.Title);
        Assert.Equal(2, projectIncidents.Count());
    }

    [Fact]
    public async Task IncidentFacade_DelegatesCommandsAndQueries()
    {
        var commandHandler = new FakeIncidentCommandHandler();
        var queryHandler = new FakeIncidentQueryHandler();
        var facade = new IncidentFacade(commandHandler, queryHandler);

        var createdId = await facade.CreateIncidentAsync(new CreateIncidentCommand(
            ProjectId: 3,
            Title: "Facade incident",
            Description: "Facade command delegation",
            IncidentType: "Safety",
            Severity: "Medio",
            Status: "Reportado",
            Location: "Site office",
            ReportedBy: null,
            AssignedTo: null,
            OccurredAt: DateTime.Now,
            Image: null,
            Notes: "Created through facade",
            ImagePath: null));
        var incident = await facade.GetIncidentByIdAsync(createdId);
        await facade.UpdateIncidentAsync(new UpdateIncidentCommand(
            Id: createdId,
            ProjectId: 3,
            Title: "Facade incident updated",
            Description: "Facade update delegation",
            IncidentType: "Safety",
            Severity: "Low",
            Status: "Resolved",
            Location: "Site office",
            ReportedBy: null,
            AssignedTo: null,
            OccurredAt: DateTime.Now,
            ResolvedAt: DateTime.Now,
            Image: null,
            Notes: "Updated through facade",
            ImagePath: null));
        await facade.DeleteIncidentAsync(createdId);

        Assert.Equal(99, createdId);
        Assert.NotNull(incident);
        Assert.Equal(1, commandHandler.CreateCalls);
        Assert.Equal(1, commandHandler.UpdateCalls);
        Assert.Equal(1, commandHandler.DeleteCalls);
        Assert.Equal(1, queryHandler.FindByIdCalls);
    }

    private static IncidentServiceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IncidentServiceDbContext>()
            .UseInMemoryDatabase($"incidents-{Guid.NewGuid()}")
            .Options;

        return new IncidentServiceDbContext(options);
    }

    private static string CreateTempImageFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"incident-test-{Guid.NewGuid()}.jpg");
        File.WriteAllBytes(path, [1, 2, 3, 4]);
        return path;
    }

    private sealed class FakeCloudinaryService : ICloudinaryService
    {
        public int UploadCalls { get; private set; }
        public int DeleteCalls { get; private set; }
        public List<string> UploadedFileNames { get; } = [];

        public Task<string> UploadImageAsync(string imagePath) =>
            Task.FromResult($"https://res.cloudinary.com/demo/incidents/{Path.GetFileName(imagePath)}");

        public Task<string> UploadIncidentImageAsync(byte[] imageBytes, string fileName, int incidentId)
        {
            UploadCalls++;
            UploadedFileNames.Add(fileName);
            return Task.FromResult($"https://res.cloudinary.com/demo/incidents/{incidentId}-{fileName}");
        }

        public Task<bool> DeleteIncidentImageAsync(string imageUrl)
        {
            DeleteCalls++;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeIncidentRepository : IIncidentRepository
    {
        private readonly List<Incident> _incidents = [];

        public Task AddAsync(Incident entity)
        {
            _incidents.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Incident?> FindByIdAsync(int id) =>
            Task.FromResult(_incidents.FirstOrDefault(incident => incident.Id == id));

        public void Update(Incident entity)
        {
        }

        public void Remove(Incident entity) => _incidents.Remove(entity);

        public Task<IEnumerable<Incident>> ListAsync() =>
            Task.FromResult<IEnumerable<Incident>>(_incidents);

        public Task<IEnumerable<Incident>> FindByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<Incident>>(
                _incidents.Where(incident => incident.ProjectId == projectId));
    }

    private sealed class FakeIncidentCommandHandler : IIncidentCommandHandler
    {
        public int CreateCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int DeleteCalls { get; private set; }

        public Task<int> HandleAsync(CreateIncidentCommand command)
        {
            CreateCalls++;
            return Task.FromResult(99);
        }

        public Task HandleAsync(UpdateIncidentCommand command)
        {
            UpdateCalls++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            DeleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIncidentQueryHandler : IIncidentQueryHandler
    {
        public int FindByIdCalls { get; private set; }

        public Task<Incident?> HandleAsync(GetIncidentByIdQuery query)
        {
            FindByIdCalls++;
            return Task.FromResult<Incident?>(new Incident { Id = query.Id, Title = "Facade incident" });
        }

        public Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query) =>
            Task.FromResult<IEnumerable<Incident>>([new Incident { Id = 1, ProjectId = query.ProjectId }]);
    }
}
