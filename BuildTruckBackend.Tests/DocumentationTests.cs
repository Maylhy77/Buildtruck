using System.Text;
using BuildTruckDocumentationService.Documentation.Application.ACL.Services;
using BuildTruckDocumentationService.Documentation.Application.Internal.CommandServices;
using BuildTruckDocumentationService.Documentation.Domain.Model.Commands;
using BuildTruckDocumentationService.Documentation.Domain.Repositories;
using BuildTruckDocumentationService.Documentation.Infrastructure.Exports;
using BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;
using BuildTruckShared.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Xunit;
using DocumentationDocument = BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates.Documentation;

namespace BuildTruckBackend.Tests;

public class DocumentationTests
{
    [Fact]
    public void CreateOrUpdateDocumentationResource_RequiresImageForCreate_AndAcceptsImageForUpdate()
    {
        var createResource = new CreateOrUpdateDocumentationResource
        {
            ProjectId = 20,
            Title = "Daily progress",
            Description = "Concrete pour inspection evidence",
            Date = DateTime.Now.Date
        };

        var updateResource = new CreateOrUpdateDocumentationResource
        {
            Id = 8,
            ProjectId = 20,
            Title = "Daily progress",
            Description = "Concrete pour inspection evidence",
            Date = DateTime.Now.Date
        };

        Assert.False(createResource.IsValid());
        Assert.Contains("Image file is required", string.Join(" ", createResource.GetValidationErrors()));
        Assert.True(updateResource.IsValid());
        Assert.True(updateResource.IsUpdate());
    }

    [Fact]
    public void CreateOrUpdateDocumentationResource_RejectsUnsupportedOrEmptyImage()
    {
        var resource = new CreateOrUpdateDocumentationResource
        {
            ProjectId = 20,
            Title = "Daily progress",
            Description = "Concrete pour inspection evidence",
            Date = DateTime.Now.Date,
            ImageFile = CreateFormFile("evidence.txt", [])
        };

        var errors = resource.GetValidationErrors();

        Assert.False(resource.IsValid());
        Assert.Contains(errors, error => error.Contains("format"));
        Assert.Contains(errors, error => error.Contains("empty"));
    }

    [Fact]
    public void DocumentationAggregate_UpdatesImageAndSoftDeletes()
    {
        var documentation = new DocumentationDocument(
            projectId: 15,
            title: "Initial excavation",
            description: "Excavation completed near the north wing",
            imagePath: "https://res.cloudinary.com/demo/documentation/initial.jpg",
            date: DateTime.Now.Date,
            createdBy: 7);

        documentation.UpdateBasicInfo(
            "Excavation and shoring",
            "Shoring evidence added to the daily documentation",
            DateTime.Now.Date.AddDays(-1));
        documentation.UpdateImage("https://res.cloudinary.com/demo/documentation/shoring.jpg");

        Assert.True(documentation.BelongsToProject(15));
        Assert.True(documentation.HasValidImage());
        Assert.Equal("Excavation and shoring", documentation.Title);

        documentation.SoftDelete();

        Assert.False(documentation.IsActive());
        Assert.False(documentation.BelongsToProject(15));
    }

    [Fact]
    public async Task DocumentationCommandService_CreatesDocumentation_WhenProjectExistsAndTitleIsUnique()
    {
        var repository = new FakeDocumentationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new DocumentationCommandService(
            repository,
            unitOfWork,
            new FakeProjectContextService(projectExists: true),
            new FakeCloudinaryService());

        var result = await service.Handle(new CreateOrUpdateDocumentationCommand(
            Id: null,
            ProjectId: 30,
            Title: "Foundation progress",
            Description: "Foundation reinforcement installed and documented",
            ImagePath: "https://res.cloudinary.com/demo/documentation/foundation.jpg",
            Date: DateTime.Now.Date,
            CreatedBy: 12));

        Assert.NotNull(result);
        Assert.Single(repository.Documents);
        Assert.Equal(1, unitOfWork.CompleteCalls);
        Assert.Equal("Foundation progress", result.Title);
        Assert.Equal(30, result.ProjectId);
    }

    [Fact]
    public async Task DocumentationCommandService_RejectsDuplicateTitleWithinProject()
    {
        var repository = new FakeDocumentationRepository();
        await repository.AddAsync(new DocumentationDocument(
            projectId: 30,
            title: "Foundation progress",
            description: "Existing project evidence",
            imagePath: "https://res.cloudinary.com/demo/documentation/existing.jpg",
            date: DateTime.Now.Date,
            createdBy: 12));

        var service = new DocumentationCommandService(
            repository,
            new FakeUnitOfWork(),
            new FakeProjectContextService(projectExists: true),
            new FakeCloudinaryService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new CreateOrUpdateDocumentationCommand(
                Id: null,
                ProjectId: 30,
                Title: "Foundation progress",
                Description: "Duplicate project evidence",
                ImagePath: "https://res.cloudinary.com/demo/documentation/duplicate.jpg",
                Date: DateTime.Now.Date,
                CreatedBy: 12)));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void DocumentationExportHandler_ExportsCsvAndStatsIgnoringSoftDeletedDocuments()
    {
        var active = new DocumentationDocument(
            40,
            "Active evidence",
            "Daily active documentation entry",
            "https://res.cloudinary.com/demo/documentation/active.jpg",
            DateTime.Now.Date,
            9);
        var deleted = new DocumentationDocument(
            40,
            "Deleted evidence",
            "Daily deleted documentation entry",
            "https://res.cloudinary.com/demo/documentation/deleted.jpg",
            DateTime.Now.Date.AddDays(-2),
            9);
        deleted.SoftDelete();

        var handler = new DocumentationExportHandler();
        var csv = Encoding.UTF8.GetString(handler.ExportToCsv([active, deleted]));
        var stats = handler.GetProjectStatistics([active, deleted]);

        Assert.Contains("Active evidence", csv);
        Assert.DoesNotContain("Deleted evidence", csv);
        Assert.Equal(1, stats["totalDocuments"]);
        Assert.Equal(1, stats["documentsWithImages"]);
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "image", fileName);
    }

    private static void SetEntityId(object entity, int id)
    {
        var backingField = entity.GetType().GetField(
            "<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private sealed class FakeDocumentationRepository : IDocumentationRepository
    {
        private int _nextId = 1;

        public List<DocumentationDocument> Documents { get; } = [];

        public Task AddAsync(DocumentationDocument entity)
        {
            if (entity.Id == 0)
            {
                SetEntityId(entity, _nextId++);
            }

            Documents.Add(entity);
            return Task.CompletedTask;
        }

        public Task<DocumentationDocument?> FindByIdAsync(int id) =>
            Task.FromResult(Documents.FirstOrDefault(document => document.Id == id && !document.IsDeleted));

        public void Update(DocumentationDocument entity)
        {
        }

        public void Remove(DocumentationDocument entity) => Documents.Remove(entity);

        public Task<IEnumerable<DocumentationDocument>> ListAsync() =>
            Task.FromResult<IEnumerable<DocumentationDocument>>(Documents.Where(document => !document.IsDeleted));

        public Task<IEnumerable<DocumentationDocument>> FindByProjectIdAsync(int projectId) =>
            Task.FromResult<IEnumerable<DocumentationDocument>>(
                Documents.Where(document => document.ProjectId == projectId && !document.IsDeleted));

        public Task<DocumentationDocument?> FindByIdAndProjectAsync(int id, int projectId) =>
            Task.FromResult(Documents.FirstOrDefault(document =>
                document.Id == id &&
                document.ProjectId == projectId &&
                !document.IsDeleted));

        public Task<IEnumerable<DocumentationDocument>> FindByProjectIdOrderedByDateAsync(int projectId) =>
            Task.FromResult<IEnumerable<DocumentationDocument>>(
                Documents
                    .Where(document => document.ProjectId == projectId && !document.IsDeleted)
                    .OrderByDescending(document => document.Date));

        public Task<IEnumerable<DocumentationDocument>> FindRecentByProjectIdAsync(int projectId, int days = 7)
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-days);
            return Task.FromResult<IEnumerable<DocumentationDocument>>(
                Documents.Where(document =>
                    document.ProjectId == projectId &&
                    !document.IsDeleted &&
                    document.Date >= cutoffDate));
        }

        public Task<bool> ExistsByTitleAndProjectAsync(string title, int projectId, int? excludeId = null) =>
            Task.FromResult(Documents.Any(document =>
                document.Title == title &&
                document.ProjectId == projectId &&
                !document.IsDeleted &&
                document.Id != excludeId));
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
        public Task<string> UploadDocumentationImageAsync(byte[] imageBytes, string fileName, int documentationId) =>
            Task.FromResult($"https://res.cloudinary.com/demo/documentation/{documentationId}-{fileName}");

        public Task<bool> DeleteDocumentationImageAsync(string imageUrl) => Task.FromResult(true);

        public string GetOptimizedImageUrl(string imageUrl, int width = 400, int height = 300) =>
            $"{imageUrl}?w={width}&h={height}";
    }
}
