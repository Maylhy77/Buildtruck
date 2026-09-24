using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckMaterialsService.Materials.Application.Internal.CommandServices;

public class MaterialCommandService : IMaterialCommandService
{
    private readonly IMaterialRepository _materialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContextService _notificationContextService;
    private readonly IMaterialCacheService _cache;

    public MaterialCommandService(
        IMaterialRepository materialRepository,
        IUnitOfWork unitOfWork,
        INotificationContextService notificationContextService,
        IMaterialCacheService cache)
    {
        _materialRepository = materialRepository;
        _unitOfWork = unitOfWork;
        _notificationContextService = notificationContextService;
        _cache = cache;
    }

    public async Task<Material?> Handle(CreateMaterialCommand command)
    {
        var material = new Material(
            command.ProjectId,
            new MaterialName(command.Name),
            new MaterialType(command.Type),
            new MaterialUnit(command.Unit),
            new MaterialQuantity(command.MinimumStock),
            command.Provider);

        await _materialRepository.AddAsync(material);
        await _unitOfWork.CompleteAsync();
        await _cache.InvalidateProjectAsync(material.ProjectId);
        await _notificationContextService.NotifyMaterialAddedAsync(
            material.ProjectId,
            material.Id,
            material.Name.Value);

        return material;
    }

    public async Task<Material?> Handle(UpdateMaterialCommand command)
    {
        var material = await _materialRepository.GetByIdAsync(command.MaterialId);
        if (material == null)
            throw new InvalidOperationException("Material not found");

        material.UpdateBasicInfo(
            new MaterialName(command.Name),
            new MaterialType(command.Type),
            new MaterialUnit(command.Unit),
            new MaterialQuantity(command.MinimumStock),
            command.Provider);

        await _unitOfWork.CompleteAsync();
        await _cache.InvalidateProjectAsync(material.ProjectId);
        await _cache.InvalidateMaterialAsync(material.Id);

        if (material.Stock <= material.MinimumStock && material.Stock > 0)
        {
            await _notificationContextService.NotifyLowStockAsync(
                material.ProjectId,
                material.Name.Value,
                material.Stock.Value);
        }
        else if (material.Stock == 0)
        {
            await _notificationContextService.NotifyCriticalStockAsync(
                material.ProjectId,
                material.Name.Value);
        }

        return material;
    }

    public async Task<bool> Handle(DeleteMaterialCommand command)
    {
        var material = await _materialRepository.GetByIdAsync(command.MaterialId);
        if (material == null)
            return false;

        var projectId = material.ProjectId;

        _materialRepository.Remove(material);
        await _unitOfWork.CompleteAsync();
        await _cache.InvalidateProjectAsync(projectId);
        await _cache.InvalidateMaterialAsync(command.MaterialId);
        return true;
    }
}
