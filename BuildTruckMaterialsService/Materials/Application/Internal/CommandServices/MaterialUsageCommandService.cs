using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckMaterialsService.Materials.Application.Internal.CommandServices;

public class MaterialUsageCommandService : IMaterialUsageCommandService
{
    private readonly IMaterialUsageRepository _usageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContextService _notificationContextService;

    public MaterialUsageCommandService(
        IMaterialUsageRepository usageRepository,
        IUnitOfWork unitOfWork,
        INotificationContextService notificationContextService)
    {
        _usageRepository = usageRepository;
        _unitOfWork = unitOfWork;
        _notificationContextService = notificationContextService;
    }

    public async Task<MaterialUsage?> Handle(CreateMaterialUsageCommand command)
    {
        var usage = new MaterialUsage(
            command.ProjectId,
            command.MaterialId,
            command.Date,
            new MaterialQuantity(command.Quantity),
            new UsageType(command.UsageType),
            command.Area,
            command.Worker,
            command.Observations);

        await _usageRepository.AddAsync(usage);
        await _unitOfWork.CompleteAsync();
        await _notificationContextService.NotifyMaterialUsedAsync(
            usage.ProjectId,
            usage.Quantity.Value,
            usage.UsageType.Value,
            usage.Observations);

        return usage;
    }

    public async Task<MaterialUsage?> Handle(UpdateMaterialUsageCommand command)
    {
        var usage = await _usageRepository.GetByIdAsync(command.UsageId);
        if (usage == null)
            throw new InvalidOperationException("Usage not found");

        usage.UpdateDetails(
            command.Date,
            new MaterialQuantity(command.Quantity),
            new UsageType(command.UsageType),
            command.Area,
            command.Worker,
            command.Observations);

        await _unitOfWork.CompleteAsync();
        return usage;
    }

    public async Task<bool> Handle(DeleteMaterialUsageCommand command)
    {
        var usage = await _usageRepository.GetByIdAsync(command.UsageId);
        if (usage == null)
            return false;

        _usageRepository.Remove(usage);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
