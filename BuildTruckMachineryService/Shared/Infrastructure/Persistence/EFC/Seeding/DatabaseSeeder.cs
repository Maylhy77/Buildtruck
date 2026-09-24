using BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Configuration;
using MachineryEntity = BuildTruckMachineryService.Machinery.Domain.Model.Aggregates.Machinery;

namespace BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(MachineryServiceDbContext context)
    {
        if (context.Machinery.Any()) return;

        var seedMachinery = new[]
        {
            new MachineryEntity { ProjectId = 1, Name = "Excavadora CAT 320", LicensePlate = "EXC-001", MachineryType = "Excavadora", Status = "active", Provider = "Caterpillar", Description = "Excavadora de 20 toneladas", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Grúa Móvil 50T", LicensePlate = "GRU-002", MachineryType = "Grúa", Status = "active", Provider = "Liebherr", Description = "Grúa móvil 50 toneladas", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Compactador Dynapac", LicensePlate = "COM-003", MachineryType = "Compactador", Status = "active", Provider = "Dynapac", Description = "Compactador de asfalto", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Cargador Frontal CAT 950", LicensePlate = "CAR-004", MachineryType = "Cargador", Status = "active", Provider = "Caterpillar", Description = "Cargador frontal 3.5 m3", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Retroexcavadora Komatsu", LicensePlate = "RET-005", MachineryType = "Retroexcavadora", Status = "maintenance", Provider = "Komatsu", Description = "Retroexcavadora 1.2 m3", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Motoniveladora Volvo", LicensePlate = "MOT-006", MachineryType = "Motoniveladora", Status = "active", Provider = "Volvo", Description = "Motoniveladora para nivelación", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Rodillo Compactador", LicensePlate = "ROD-007", MachineryType = "Rodillo", Status = "active", Provider = "Bomag", Description = "Rodillo neumático compactador", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 1, Name = "Hormigonera", LicensePlate = "HOR-008", MachineryType = "Hormigonera", Status = "active", Provider = "IMER", Description = "Hormigonera de 500L", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Excavadora JCB 3CX", LicensePlate = "EXC-009", MachineryType = "Excavadora", Status = "active", Provider = "JCB", Description = "Excavadora 3CX compacta", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Cargador Bobcat", LicensePlate = "BOB-010", MachineryType = "Cargador", Status = "active", Provider = "Bobcat", Description = "Cargador compacto multifuncional", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Grúa Torre Potain", LicensePlate = "TOR-011", MachineryType = "Grúa Torre", Status = "active", Provider = "Potain", Description = "Grúa torre para edificación", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Martillo Neumático", LicensePlate = "MAR-012", MachineryType = "Herramienta", Status = "active", Provider = "Atlas Copco", Description = "Martillo neumático para demolición", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Compresor de aire", LicensePlate = "COM-013", MachineryType = "Compresor", Status = "active", Provider = "Atlas Copco", Description = "Compresor 250 CFM", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 2, Name = "Soldadora Eléctrica", LicensePlate = "SOL-014", MachineryType = "Soldadora", Status = "active", Provider = "ESAB", Description = "Soldadora 400A", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 3, Name = "Dumper Volquete", LicensePlate = "VOL-015", MachineryType = "Volquete", Status = "active", Provider = "Volvo", Description = "Volquete 15 toneladas", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 3, Name = "Camión Mixer", LicensePlate = "MIX-016", MachineryType = "Mixer", Status = "active", Provider = "Hino", Description = "Camión mixer 8 m3", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 4, Name = "Perforadora DTH", LicensePlate = "PER-017", MachineryType = "Perforadora", Status = "active", Provider = "Sandvik", Description = "Perforadora DTH para pozos", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 4, Name = "Vibrador de concreto", LicensePlate = "VIB-018", MachineryType = "Vibrador", Status = "active", Provider = "Wacker", Description = "Vibrador de aguja", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
            new MachineryEntity { ProjectId = 5, Name = "Grúa pórtico", LicensePlate = "POR-019", MachineryType = "Grúa Pórtico", Status = "active", Provider = "Gantry", Description = "Grúa pórtico 100 toneladas", PersonnelId = null, ImageUrl = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RegisterDate = DateTime.UtcNow.Date },
        };

        context.Machinery.AddRange(seedMachinery);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ MACHINERY SEED DATA CREATED - 20 items");
    }
}
