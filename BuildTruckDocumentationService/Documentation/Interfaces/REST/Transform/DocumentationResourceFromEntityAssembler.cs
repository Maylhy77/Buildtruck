using BuildTruckDocumentationService.Documentation.Interfaces.REST.Resources;

namespace BuildTruckDocumentationService.Documentation.Interfaces.REST.Transform;

public static class DocumentationResourceFromEntityAssembler
{
    public static DocumentationResource ToResourceFromEntity(Domain.Model.Aggregates.Documentation documentation)
    {
        return new DocumentationResource(
            documentation.Id,
            documentation.ProjectId,
            documentation.Title,
            documentation.Description,
            documentation.ImagePath,
            documentation.Date,
            documentation.CreatedBy,
            documentation.HasValidImage(),
            documentation.Date.ToString("yyyy-MM-dd"),
            documentation.CreatedDate,
            documentation.UpdatedDate
        );
    }

    public static IEnumerable<DocumentationResource> ToResourceFromEntity(
        IEnumerable<Domain.Model.Aggregates.Documentation> documentation)
    {
        return documentation.Select(ToResourceFromEntity);
    }
}