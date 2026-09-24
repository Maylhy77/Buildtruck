using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckMachineryService.Machinery.Domain.Model.Aggregates;
using BuildTruckMachineryService.Machinery.Domain.Model.Commands;
using BuildTruckMachineryService.Machinery.Domain.Repositories;
using BuildTruckMachineryService.Machinery.Domain.Services;
using BuildTruckShared.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BuildTruckMachineryService.Machinery.Application.Internal.CommandServices;

public class DeleteMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMachineryCloudinaryService _cloudinaryService;
    private readonly ILogger<DeleteMachineryCommandHandler> _logger;

    public DeleteMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        IMachineryCloudinaryService cloudinaryService,
        ILogger<DeleteMachineryCommandHandler> logger)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task Handle(DeleteMachineryCommand command)
    {
        _logger.LogInformation("Handling DeleteMachineryCommand for Machinery ID {Id}", command.Id);

        var machinery = await _machineryRepository.FindByIdAsync(command.Id);
        if (machinery == null)
        {
            _logger.LogWarning("Machinery with ID {Id} not found", command.Id);
            throw new ArgumentException($"Machinery with ID {command.Id} not found");
        }

        // Delete associated image from Cloudinary
        if (!string.IsNullOrEmpty(machinery.ImageUrl))
        {
            try
            {
                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(machinery.ImageUrl);
                await _cloudinaryService.DeleteImageAsync(publicId);
                _logger.LogInformation("Deleted image for Machinery ID {Id} with public ID {PublicId}", command.Id, publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image for Machinery ID {Id}", command.Id);
                // Continue with deletion to ensure machinery is removed
            }
        }

        // Delete machinery
        _machineryRepository.Remove(machinery);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Successfully deleted Machinery ID {Id}", command.Id);
    }
}