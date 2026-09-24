using BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;
using BuildTruckDocumentationService.Documentation.Domain.Repositories;
using BuildTruckDocumentationService.Documentation.Domain.Services;

namespace BuildTruckDocumentationService.Documentation.Application.Internal.QueryServices;

public class DocumentationQueryService : IDocumentationQueryService
{
    private readonly IDocumentationRepository _documentationRepository;

    public DocumentationQueryService(IDocumentationRepository documentationRepository)
    {
        _documentationRepository = documentationRepository;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> GetDocumentationByProjectAsync(int projectId)
    {
        return await _documentationRepository.FindByProjectIdOrderedByDateAsync(projectId);
    }

    public async Task<Domain.Model.Aggregates.Documentation?> GetDocumentationByIdAsync(int documentationId)
    {
        return await _documentationRepository.FindByIdAsync(documentationId);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Documentation>> GetRecentDocumentationByProjectAsync(int projectId, int days = 7)
    {
        return await _documentationRepository.FindRecentByProjectIdAsync(projectId, days);
    }

    public async Task<bool> ValidateTitleUniqueAsync(string title, int projectId, int? excludeId = null)
    {
        return await _documentationRepository.ExistsByTitleAndProjectAsync(title, projectId, excludeId);
    }
}