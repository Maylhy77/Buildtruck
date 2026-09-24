using BuildTruckDocumentationService.Documentation.Application.ACL.Services;
using BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;
using BuildTruckDocumentationService.Documentation.Domain.Model.Commands;
using BuildTruckDocumentationService.Documentation.Domain.Repositories;
using BuildTruckDocumentationService.Documentation.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckDocumentationService.Documentation.Application.Internal.CommandServices;

public class DocumentationCommandService : IDocumentationCommandService
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectContextService _projectContextService;
    private readonly ICloudinaryService _cloudinaryService;

    public DocumentationCommandService(
        IDocumentationRepository documentationRepository,
        IUnitOfWork unitOfWork,
        IProjectContextService projectContextService,
        ICloudinaryService cloudinaryService)
    {
        _documentationRepository = documentationRepository;
        _unitOfWork = unitOfWork;
        _projectContextService = projectContextService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Domain.Model.Aggregates.Documentation?> Handle(CreateOrUpdateDocumentationCommand command)
    {
        // TODO: Validate project exists (commented - same approach as Incidents)
        // var projectExists = await _projectContextService.ProjectExistsAsync(command.ProjectId);
        // if (!projectExists)
        //     throw new ArgumentException($"Project with ID {command.ProjectId} does not exist");

        // Validate image is provided
        if (string.IsNullOrEmpty(command.ImagePath))
            throw new ArgumentException("Image is required for documentation");

        try
        {
            Domain.Model.Aggregates.Documentation documentation;

            if (command.Id.HasValue)
            {
                // UPDATE existing documentation
                var existingDocumentation = await _documentationRepository.FindByIdAsync(command.Id.Value);
                if (existingDocumentation == null)
                    throw new ArgumentException($"Documentation with ID {command.Id.Value} not found");

                documentation = existingDocumentation;

                // Validate title uniqueness (excluding current document)
                var titleExists = await _documentationRepository.ExistsByTitleAndProjectAsync(
                    command.Title, command.ProjectId, command.Id.Value);
                if (titleExists)
                    throw new InvalidOperationException($"Title '{command.Title}' already exists in this project");

                // Update documentation
                documentation.UpdateBasicInfo(command.Title, command.Description, command.Date);
                
                // Update image if changed
                if (documentation.ImagePath != command.ImagePath)
                {
                    documentation.UpdateImage(command.ImagePath);
                }

                _documentationRepository.Update(documentation);
            }
            else
            {
                // CREATE new documentation
                // Validate title uniqueness
                var titleExists = await _documentationRepository.ExistsByTitleAndProjectAsync(
                    command.Title, command.ProjectId);
                if (titleExists)
                    throw new InvalidOperationException($"Title '{command.Title}' already exists in this project");

                // Create documentation aggregate
                documentation = new Domain.Model.Aggregates.Documentation(
                    command.ProjectId,
                    command.Title,
                    command.Description,
                    command.ImagePath,
                    command.Date,
                    command.CreatedBy);

                await _documentationRepository.AddAsync(documentation);
            }

            await _unitOfWork.CompleteAsync();
            return documentation;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save documentation: {ex.Message}");
        }
    }

    public async Task<bool> Handle(DeleteDocumentationCommand command)
    {
        var documentation = await _documentationRepository.FindByIdAsync(command.DocumentationId);
        if (documentation == null)
            return false;

        try
        {
            // Soft delete
            documentation.SoftDelete();
            _documentationRepository.Update(documentation);
            await _unitOfWork.CompleteAsync();

            // Optionally delete image from cloud storage
            if (!string.IsNullOrEmpty(documentation.ImagePath))
            {
                try
                {
                    await _cloudinaryService.DeleteDocumentationImageAsync(documentation.ImagePath);
                }
                catch
                {
                    // Log but don't fail the operation
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete documentation: {ex.Message}");
        }
    }
}
