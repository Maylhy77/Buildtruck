using BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;

namespace BuildTruckDocumentationService.Documentation.Domain.Services;

public interface IDocumentationQueryService
{
    Task<IEnumerable<Documentation.Domain.Model.Aggregates.Documentation>> GetDocumentationByProjectAsync(int projectId);
    
    Task<Documentation.Domain.Model.Aggregates.Documentation?> GetDocumentationByIdAsync(int documentationId);
    
    Task<IEnumerable<Documentation.Domain.Model.Aggregates.Documentation>> GetRecentDocumentationByProjectAsync(int projectId, int days = 7);
    
    Task<bool> ValidateTitleUniqueAsync(string title, int projectId, int? excludeId = null);
}