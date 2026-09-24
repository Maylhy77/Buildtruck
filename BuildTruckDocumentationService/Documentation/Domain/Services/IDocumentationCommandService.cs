using BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;
using BuildTruckDocumentationService.Documentation.Domain.Model.Commands;

namespace BuildTruckDocumentationService.Documentation.Domain.Services;

public interface IDocumentationCommandService
{
    Task<Documentation.Domain.Model.Aggregates.Documentation?> Handle(CreateOrUpdateDocumentationCommand command);
    
    Task<bool> Handle(DeleteDocumentationCommand command);
}