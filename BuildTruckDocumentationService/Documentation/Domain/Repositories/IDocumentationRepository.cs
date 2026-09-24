using BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckDocumentationService.Documentation.Domain.Repositories;

public interface IDocumentationRepository : IBaseRepository<Model.Aggregates.Documentation>
{
    Task<IEnumerable<Model.Aggregates.Documentation>> FindByProjectIdAsync(int projectId);
    
    Task<Model.Aggregates.Documentation?> FindByIdAndProjectAsync(int id, int projectId);
    
    Task<IEnumerable<Model.Aggregates.Documentation>> FindByProjectIdOrderedByDateAsync(int projectId);
    
    Task<IEnumerable<Model.Aggregates.Documentation>> FindRecentByProjectIdAsync(int projectId, int days = 7);
    
    Task<bool> ExistsByTitleAndProjectAsync(string title, int projectId, int? excludeId = null);
}
