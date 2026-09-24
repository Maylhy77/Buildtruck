namespace BuildTruckDocumentationService.Documentation.Domain.Model.Commands;

public record CreateOrUpdateDocumentationCommand(
    int? Id, // null for create, value for update
    int ProjectId,
    string Title,
    string Description,
    string ImagePath, // Cloudinary URL
    DateTime Date,
    int CreatedBy
);