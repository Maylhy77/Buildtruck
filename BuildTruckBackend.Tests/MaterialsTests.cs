using BuildTruckMaterialsService.Materials.Application.ACL.Services;
using BuildTruckMaterialsService.Materials.Application.Internal.CommandServices;
using BuildTruckMaterialsService.Materials.Application.Internal.QueryServices;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Commands;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckShared.Domain.Repositories;
using Xunit;

namespace BuildTruckBackend.Tests;

public class MaterialsTests
{
    [Fact]
    public void MaterialValueObjects_NormalizeValidValues_AndRejectInvalidValues()
    {
        var type = new MaterialType("cemento");
        var unit = new MaterialUnit("kg");
        var quantity = new MaterialQuantity(10.555m);

        Assert.Equal("CEMENTO", type.Value);
        Assert.Equal("KG", unit.Value);
        Assert.Equal(10.56m, quantity.Value);
        Assert.Throws<ArgumentException>(() => new MaterialName(""));
        Assert.Throws<ArgumentException>(() => new MaterialQuantity(-1));
        Assert.Throws<ArgumentException>(() => new PaymentMethod("barter"));
    }

    [Fact]
    public async Task MaterialCommandService_CreatesMaterial_AndNotifiesOnce()
    {
        var materialRepository = new FakeMaterialRepository();
        var unitOfWork = new FakeUnitOfWork();
        var notifications = new FakeNotificationContextService();
        var cache = new FakeMaterialCacheService();
        var service = new MaterialCommandService(materialRepository, unitOfWork, notifications, cache);

        var material = await service.Handle(new CreateMaterialCommand(
            ProjectId: 12,
            Name: "Portland cement",
            Type: "CEMENTO",
            Unit: "SACO",
            MinimumStock: 20,
            Provider: "Cementos Lima"));

        Assert.NotNull(material);
        Assert.Equal(1, material.Id);
        Assert.Equal(12, material.ProjectId);
        Assert.Single(materialRepository.Materials);
        Assert.Equal(1, unitOfWork.CompleteCalls);
        Assert.Equal(1, notifications.MaterialAddedCalls);
        Assert.Equal(new[] { 12 }, cache.InvalidatedProjects);
    }

    [Fact]
    public async Task MaterialEntryCommandService_RejectsEntry_WhenMaterialBelongsToDifferentProject()
    {
        var materialRepository = new FakeMaterialRepository();
        await materialRepository.AddAsync(new Material(
            4,
            new MaterialName("Steel bars"),
            new MaterialType("ACERO"),
            new MaterialUnit("UND"),
            new MaterialQuantity(5),
            "Steel Provider"));

        var service = new MaterialEntryCommandService(
            new FakeMaterialEntryRepository(),
            materialRepository,
            new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new CreateMaterialEntryCommand(
                ProjectId: 9,
                MaterialId: 1,
                Date: DateTime.Today,
                Quantity: 100,
                Unit: "UND",
                Provider: "Steel Provider",
                Ruc: "12345678901",
                Payment: "CASH",
                DocumentType: "INVOICE",
                DocumentNumber: "F001-99",
                UnitCost: 12,
                TotalCost: 1200,
                Status: "CONFIRMED",
                Observations: "Wrong project")));

        Assert.Contains("no pertenece", exception.Message);
    }

    [Fact]
    public async Task MaterialUsageCommandService_CreatesUsage_AndNotifiesMaterialUsed()
    {
        var usageRepository = new FakeMaterialUsageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var notifications = new FakeNotificationContextService();
        var service = new MaterialUsageCommandService(usageRepository, unitOfWork, notifications);

        var usage = await service.Handle(new CreateMaterialUsageCommand(
            ProjectId: 6,
            MaterialId: 3,
            Date: DateTime.Today,
            Quantity: 8,
            Area: "North wing",
            UsageType: "CONSTRUCCION",
            Worker: "Juan Perez",
            Observations: "Column pour"));

        Assert.NotNull(usage);
        Assert.Single(usageRepository.Usages);
        Assert.Equal(1, unitOfWork.CompleteCalls);
        Assert.Equal(1, notifications.MaterialUsedCalls);
        Assert.Equal(8, notifications.LastUsedQuantity);
    }

    [Fact]
    public async Task InventoryQueryService_CalculatesStockAndWeightedAveragePrice()
    {
        var materialRepository = new FakeMaterialRepository();
        var entryRepository = new FakeMaterialEntryRepository();
        var usageRepository = new FakeMaterialUsageRepository();

        var material = new Material(
            20,
            new MaterialName("Ready mix concrete"),
            new MaterialType("CEMENTO"),
            new MaterialUnit("M3"),
            new MaterialQuantity(10),
            "Concrete Provider");
        await materialRepository.AddAsync(material);

        await entryRepository.AddAsync(new MaterialEntry(
            20,
            material.Id,
            DateTime.Today.AddDays(-2),
            new MaterialQuantity(100),
            new MaterialUnit("M3"),
            new MaterialCost(10, "PEN"),
            new PaymentMethod("CASH"),
            new DocumentType("INVOICE"),
            "F001-1",
            "Concrete Provider",
            "12345678901",
            "First delivery"));

        await entryRepository.AddAsync(new MaterialEntry(
            20,
            material.Id,
            DateTime.Today.AddDays(-1),
            new MaterialQuantity(50),
            new MaterialUnit("M3"),
            new MaterialCost(14, "PEN"),
            new PaymentMethod("TRANSFER"),
            new DocumentType("INVOICE"),
            "F001-2",
            "Concrete Provider",
            "12345678901",
            "Second delivery"));

        await usageRepository.AddAsync(new MaterialUsage(
            20,
            material.Id,
            DateTime.Today,
            new MaterialQuantity(40),
            new UsageType("CONSTRUCCION"),
            "Foundation",
            "Crew A",
            "Daily usage"));

        var service = new InventoryQueryService(materialRepository, entryRepository, usageRepository);

        var inventory = await service.HandleDetailed(new GetInventoryByProjectQuery(20));
        var item = Assert.Single(inventory);

        Assert.Equal(150, item.TotalEntries);
        Assert.Equal(40, item.TotalUsages);
        Assert.Equal(110, item.StockActual);
        Assert.Equal(11.33m, Math.Round(item.UnitPrice, 2));
        Assert.Equal(1246.67m, Math.Round(item.Total, 2));
    }

    private static void SetEntityId(object entity, int id)
    {
        var backingField = entity.GetType().GetField(
            "<Id>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        backingField?.SetValue(entity, id);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CompleteCalls { get; private set; }

        public Task CompleteAsync()
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMaterialCacheService : IMaterialCacheService
    {
        public List<int> InvalidatedProjects { get; } = new();
        public List<int> InvalidatedMaterials { get; } = new();

        public Task<T?> GetOrSetProjectAsync<T>(int projectId, string suffix, Func<Task<T?>> factory, CancellationToken ct = default)
            => factory();

        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, CancellationToken ct = default)
            => factory();

        public Task InvalidateProjectAsync(int projectId, CancellationToken ct = default)
        {
            InvalidatedProjects.Add(projectId);
            return Task.CompletedTask;
        }

        public Task InvalidateMaterialAsync(int materialId, CancellationToken ct = default)
        {
            InvalidatedMaterials.Add(materialId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationContextService : INotificationContextService
    {
        public int MaterialAddedCalls { get; private set; }
        public int LowStockCalls { get; private set; }
        public int CriticalStockCalls { get; private set; }
        public int MaterialUsedCalls { get; private set; }
        public decimal LastUsedQuantity { get; private set; }

        public Task NotifyMaterialAddedAsync(int projectId, int materialId, string materialName)
        {
            MaterialAddedCalls++;
            return Task.CompletedTask;
        }

        public Task NotifyLowStockAsync(int projectId, string materialName, decimal stock)
        {
            LowStockCalls++;
            return Task.CompletedTask;
        }

        public Task NotifyCriticalStockAsync(int projectId, string materialName)
        {
            CriticalStockCalls++;
            return Task.CompletedTask;
        }

        public Task NotifyMaterialUsedAsync(int projectId, decimal quantity, string usageType, string? observations)
        {
            MaterialUsedCalls++;
            LastUsedQuantity = quantity;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeMaterialRepository : IMaterialRepository
    {
        private int _nextId = 1;
        public List<Material> Materials { get; } = [];

        public Task AddAsync(Material entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);
            Materials.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Material?> FindByIdAsync(int id) =>
            Task.FromResult(Materials.FirstOrDefault(material => material.Id == id));

        public Task<Material?> GetByIdAsync(int materialId) => FindByIdAsync(materialId);

        public Task<List<Material>> GetByProjectIdAsync(int projectId) =>
            Task.FromResult(Materials.Where(material => material.ProjectId == projectId).ToList());

        public Task<IEnumerable<Material>> ListAsync() =>
            Task.FromResult<IEnumerable<Material>>(Materials);

        public void Remove(Material entity) => Materials.Remove(entity);

        public void Update(Material entity)
        {
        }
    }

    private sealed class FakeMaterialEntryRepository : IMaterialEntryRepository
    {
        private int _nextId = 1;
        public List<MaterialEntry> Entries { get; } = [];

        public Task AddAsync(MaterialEntry entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);
            Entries.Add(entity);
            return Task.CompletedTask;
        }

        public Task<MaterialEntry?> FindByIdAsync(int id) =>
            Task.FromResult(Entries.FirstOrDefault(entry => entry.Id == id));

        public Task<MaterialEntry?> GetByIdAsync(int entryId) => FindByIdAsync(entryId);

        public Task<List<MaterialEntry>> GetByMaterialIdAsync(int materialId) =>
            Task.FromResult(Entries.Where(entry => entry.MaterialId == materialId).ToList());

        public Task<List<MaterialEntry>> GetByProjectIdAsync(int projectId) =>
            Task.FromResult(Entries.Where(entry => entry.ProjectId == projectId).ToList());

        public Task<IEnumerable<MaterialEntry>> ListAsync() =>
            Task.FromResult<IEnumerable<MaterialEntry>>(Entries);

        public void Remove(MaterialEntry entity) => Entries.Remove(entity);

        public void Update(MaterialEntry entity)
        {
        }
    }

    private sealed class FakeMaterialUsageRepository : IMaterialUsageRepository
    {
        private int _nextId = 1;
        public List<MaterialUsage> Usages { get; } = [];

        public Task AddAsync(MaterialUsage entity)
        {
            if (entity.Id == 0)
                SetEntityId(entity, _nextId++);
            Usages.Add(entity);
            return Task.CompletedTask;
        }

        public Task<MaterialUsage?> FindByIdAsync(int id) =>
            Task.FromResult(Usages.FirstOrDefault(usage => usage.Id == id));

        public Task<MaterialUsage?> GetByIdAsync(int usageId) => FindByIdAsync(usageId);

        public Task<List<MaterialUsage>> GetByMaterialIdAsync(int materialId) =>
            Task.FromResult(Usages.Where(usage => usage.MaterialId == materialId).ToList());

        public Task<List<MaterialUsage>> GetByProjectIdAsync(int projectId) =>
            Task.FromResult(Usages.Where(usage => usage.ProjectId == projectId).ToList());

        public Task<IEnumerable<MaterialUsage>> ListAsync() =>
            Task.FromResult<IEnumerable<MaterialUsage>>(Usages);

        public void Remove(MaterialUsage entity) => Usages.Remove(entity);

        public void Update(MaterialUsage entity)
        {
        }
    }
}
