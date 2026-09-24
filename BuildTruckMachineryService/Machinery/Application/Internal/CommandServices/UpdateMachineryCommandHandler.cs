using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckMachineryService.Machinery.Domain.Model.Aggregates;
using BuildTruckMachineryService.Machinery.Domain.Model.Commands;
using BuildTruckMachineryService.Machinery.Domain.Repositories;
using BuildTruckMachineryService.Machinery.Domain.Services;
using BuildTruckMachineryService.Projects.Application.Internal.OutboundServices;
using BuildTruckShared.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace BuildTruckMachineryService.Machinery.Application.Internal.CommandServices;

public class UpdateMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMachineryCloudinaryService _cloudinaryService;
    private readonly IProjectFacade _projectFacade;
    private readonly ILogger<UpdateMachineryCommandHandler> _logger;

    public UpdateMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        IMachineryCloudinaryService cloudinaryService,
        IProjectFacade projectFacade,
        ILogger<UpdateMachineryCommandHandler> logger)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
        _projectFacade = projectFacade;
        _logger = logger;
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null)
{
    _logger.LogInformation("Handling UpdateMachineryCommand for Machinery ID {Id}", command.Id);

    // Validate ProjectId exists
    if (!await _projectFacade.ExistsByIdAsync(command.ProjectId))
    {
        _logger.LogWarning("Project with ID {ProjectId} not found", command.ProjectId);
        throw new ArgumentException($"Project with ID {command.ProjectId} not found");
    }

    // Check for existing machinery with the same license plate (excluding current machinery)
    var existingMachinery = await _machineryRepository.FindByLicensePlateAsync(command.LicensePlate, command.ProjectId);
    if (existingMachinery != null && existingMachinery.Id != command.Id)
    {
        _logger.LogWarning("Machinery with LicensePlate {LicensePlate} already exists for ProjectId {ProjectId}", command.LicensePlate, command.ProjectId);
        throw new InvalidOperationException($"Machinery with license plate {command.LicensePlate} already exists for project {command.ProjectId}");
    }

    // Retrieve the machinery
    var machinery = await _machineryRepository.FindByIdAsync(command.Id);
    if (machinery == null)
    {
        _logger.LogWarning("Machinery with ID {Id} not found", command.Id);
        throw new ArgumentException($"Machinery with ID {command.Id} not found");
    }

    // Update machinery properties
    machinery.ProjectId = command.ProjectId;
    machinery.Name = command.Name;
    machinery.LicensePlate = command.LicensePlate;
    machinery.MachineryType = command.MachineryType;
    machinery.Status = command.Status.ToString();
    machinery.Provider = command.Provider;
    machinery.Description = command.Description;
    machinery.PersonnelId = command.PersonnelId;
    machinery.UpdatedAt = DateTime.UtcNow;

    // Handle image upload
    if (imageFile != null)
    {
        try
        {
            // Delete existing image if it exists
            if (!string.IsNullOrEmpty(machinery.ImageUrl))
            {
                _logger.LogInformation("Attempting to delete existing image: {ImageUrl}", machinery.ImageUrl);
                
                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(machinery.ImageUrl);
                _logger.LogInformation("Extracted public ID: {PublicId}", publicId);
                
                if (!string.IsNullOrEmpty(publicId))
                {
                    var deleteResult = await _cloudinaryService.DeleteImageAsync(publicId);
                    if (deleteResult)
                    {
                        _logger.LogInformation("Successfully deleted existing image for Machinery ID {Id} with public ID {PublicId}", command.Id, publicId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete existing image for Machinery ID {Id} with public ID {PublicId}", command.Id, publicId);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not extract public ID from URL: {ImageUrl}", machinery.ImageUrl);
                }
            }

            // Upload new image
            var newImageUrl = await _cloudinaryService.UploadImageAsync(imageFile);
            machinery.ImageUrl = newImageUrl;
            _logger.LogInformation("Uploaded new image for Machinery ID {Id}: {ImageUrl}", command.Id, newImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle image update for Machinery ID {Id}", command.Id);
            throw new InvalidOperationException($"Failed to handle image update: {ex.Message}");
        }
    }

    // Update in repository
    await _machineryRepository.UpdateAsync(machinery);
    await _unitOfWork.CompleteAsync();
    _logger.LogInformation("Successfully updated Machinery ID {Id}", command.Id);

    return machinery;
}
}