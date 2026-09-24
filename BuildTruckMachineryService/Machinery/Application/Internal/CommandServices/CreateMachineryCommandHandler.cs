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

public class CreateMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMachineryCloudinaryService _cloudinaryService;
    private readonly IProjectFacade _projectFacade;
    private readonly ILogger<CreateMachineryCommandHandler> _logger;

    public CreateMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        IMachineryCloudinaryService cloudinaryService,
        IProjectFacade projectFacade,
        ILogger<CreateMachineryCommandHandler> logger)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
        _projectFacade = projectFacade;
        _logger = logger;
    }

     public async Task<Domain.Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null)
    {
        _logger.LogInformation("Handling CreateMachineryCommand for LicensePlate {LicensePlate}", command.LicensePlate);

        // Validate ProjectId exists
        if (!await _projectFacade.ExistsByIdAsync(command.ProjectId))
        {
            _logger.LogWarning("Project with ID {ProjectId} not found", command.ProjectId);
            throw new ArgumentException($"Project with ID {command.ProjectId} not found");
        }

        // Check for existing machinery with the same license plate
        var existingMachinery = await _machineryRepository.FindByLicensePlateAsync(command.LicensePlate, command.ProjectId);
        if (existingMachinery != null)
        {
            _logger.LogWarning("Machinery with LicensePlate {LicensePlate} already exists for ProjectId {ProjectId}", command.LicensePlate, command.ProjectId);
            throw new InvalidOperationException($"Machinery with license plate {command.LicensePlate} already exists for project {command.ProjectId}");
        }

        // Create machinery
        var machinery = new Domain.Model.Aggregates.Machinery
        {
            ProjectId = command.ProjectId,
            Name = command.Name,
            LicensePlate = command.LicensePlate,
            MachineryType = command.MachineryType,
            Status = command.Status.ToString(),
            Provider = command.Provider,
            Description = command.Description,
            PersonnelId = command.PersonnelId,
            RegisterDate = command.RegisterDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Handle image upload using command.ImageBytes and command.ImageFileName
        if (command.ImageBytes != null && !string.IsNullOrEmpty(command.ImageFileName))
        {
            try
            {
                machinery.ImageUrl = await _cloudinaryService.UploadImageAsync(command.ImageBytes, command.ImageFileName);
                _logger.LogInformation("Uploaded image for Machinery ID {Id}: {ImageUrl}", machinery.Id, machinery.ImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for Machinery LicensePlate {LicensePlate}", command.LicensePlate);
                throw new InvalidOperationException($"Failed to upload image: {ex.Message}");
            }
        }
        else if (imageFile != null)
        {
            _logger.LogWarning("IFormFile provided but ignored; using ImageBytes from command for Machinery LicensePlate {LicensePlate}", command.LicensePlate);
        }

        // Save machinery
        await _machineryRepository.AddAsync(machinery);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Successfully created Machinery ID {Id}", machinery.Id);

        return machinery;
    }
}