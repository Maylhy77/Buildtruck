using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;
using BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;
using PersonnelEntity = BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates.Personnel;

namespace BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PersonnelServiceDbContext context)
    {
        if (context.Personnel.Any()) return;

        var now = DateTime.Now;
        int year = now.Year;
        int month = now.Month;

        var seedPersonnel = new List<PersonnelEntity>
        {
            // ===== PROJECT 1 - Torres Miraflores =====
            Make(1, "Carlos", "Mendoza Ríos",    "45123678", "Ingeniero Civil",          "Ingeniería",     PersonnelType.TECHNICAL,       PersonnelStatus.ACTIVE,  4500m, "BCP",        "19012345678",  new DateTime(2024, 3,  1), "987654321", "carlos.mendoza@buildtruck.pe"),
            Make(1, "Ana",    "García Torres",   "47892345", "Operador de Grúa",          "Operaciones",    PersonnelType.RENTED_OPERATOR,  PersonnelStatus.ACTIVE,  3200m, "BBVA",       "01198765432",  new DateTime(2024, 3,  1), "976543210", "ana.garcia@buildtruck.pe"),

            // ===== PROJECT 2 - Edificio Giacoletti =====
            Make(2, "Luis",   "Flores Huanca",   "43567890", "Maestro de Obra",           "Construcción",   PersonnelType.SPECIALIST,      PersonnelStatus.ACTIVE,  3800m, "SCOTIABANK", "00898765432",  new DateTime(2024, 1, 15), "965432109", "luis.flores@buildtruck.pe"),
            Make(2, "María",  "López Ccari",     "46234567", "Asistente Administrativo",  "Administración", PersonnelType.ADMINISTRATIVE,  PersonnelStatus.ACTIVE,  2800m, "BCP",        "19087654321",  new DateTime(2024, 2,  1), "954321098", "maria.lopez@buildtruck.pe"),

            // ===== PROJECT 3 - Centro Comercial San Isidro (PLANIFICADO) =====
            Make(3, "Roberto",  "Vargas Quispe",    "48901234", "Técnico Eléctrico",     "Eléctrica",   PersonnelType.TECHNICAL,       PersonnelStatus.PENDING,  3500m, "INTERBANK",  "06012345678", new DateTime(2026, 7, 1), "943210987", "roberto.vargas@buildtruck.pe"),
            Make(3, "Patricia", "Sánchez Yupanqui", "44012345", "Arquitecta",            "Diseño",      PersonnelType.SPECIALIST,      PersonnelStatus.PENDING,  5000m, "BBVA",       "01101234567", new DateTime(2026, 7, 1), "932109876", "patricia.sanchez@buildtruck.pe"),

            // ===== PROJECT 4 - Hotel Luxury Cusco =====
            Make(4, "José",   "Mamani Condori",  "41234567", "Operador de Maquinaria",    "Operaciones",    PersonnelType.RENTED_OPERATOR,  PersonnelStatus.ACTIVE,  3000m, "BCP",        "19023456789",  new DateTime(2025, 6,  1), "921098765", "jose.mamani@buildtruck.pe"),
            Make(4, "Carmen", "Quispe Apaza",    "49876543", "Peón de Construcción",      "Construcción",   PersonnelType.LABORER,         PersonnelStatus.ACTIVE,  2200m, "SCOTIABANK", "00901234567",  new DateTime(2025, 6,  1), "910987654", "carmen.quispe@buildtruck.pe"),

            // ===== PROJECT 5 - Complejo Residencial La Molina (EN ESTUDIO) =====
            Make(5, "Miguel", "Torres Huamán",   "42345678", "Técnico en Seguridad",      "Seguridad",      PersonnelType.TECHNICAL,       PersonnelStatus.PENDING,  3300m, "BCP",        "19034567890",  new DateTime(2026, 8,  1), "909876543", "miguel.torres@buildtruck.pe"),
            Make(5, "Rosa",   "Díaz Ccahuana",   "44567890", "Ingeniera de Proyectos",    "Ingeniería",     PersonnelType.SPECIALIST,      PersonnelStatus.PENDING,  4800m, "BBVA",       "01145678901",  new DateTime(2026, 8,  1), "898765432", "rosa.diaz@buildtruck.pe"),

            // ===== PROJECT 6 - Autopista Metropolitana =====
            Make(6, "Fernando", "Castro Rimachi", "40123456", "Topógrafo",               "Topografía",     PersonnelType.SPECIALIST,      PersonnelStatus.ACTIVE,  4200m, "INTERBANK",  "06056789012",  new DateTime(2025, 1, 10), "887654321", "fernando.castro@buildtruck.pe"),
            Make(6, "Elena",    "Ramos Ticona",   "45678901", "Ayudante de Campo",        "Operaciones",    PersonnelType.LABORER,         PersonnelStatus.ACTIVE,  2500m, "BCP",        "19067890123",  new DateTime(2025, 1, 10), "876543210", "elena.ramos@buildtruck.pe"),

            // ===== PROJECT 7 - Puente Inca =====
            Make(7, "Alejandro", "Núñez Mamani",  "43456789", "Ingeniero Estructural",   "Ingeniería",     PersonnelType.SPECIALIST,      PersonnelStatus.ACTIVE,  5500m, "SCOTIABANK", "00934567890",  new DateTime(2025, 3,  1), "865432109", "alejandro.nunez@buildtruck.pe"),
            Make(7, "Sandra",    "Morales Anco",   "46789012", "Operador de Excavadora",  "Operaciones",    PersonnelType.RENTED_OPERATOR,  PersonnelStatus.ACTIVE,  3400m, "BBVA",       "01178901234",  new DateTime(2025, 3,  1), "854321098", "sandra.morales@buildtruck.pe"),

            // ===== PROJECT 8 - Centro de Salud Metropolitano =====
            Make(8, "Diego", "Herrera Paucar",   "47890123", "Técnico en Instalaciones", "Instalaciones",  PersonnelType.TECHNICAL,       PersonnelStatus.ACTIVE,  3600m, "BCP",        "19089012345",  new DateTime(2024, 5, 20), "843210987", "diego.herrera@buildtruck.pe"),
            Make(8, "Luz",   "Chávez Sullca",    "41890123", "Supervisora de Obra",      "Gestión",        PersonnelType.ADMINISTRATIVE,  PersonnelStatus.ACTIVE,  3900m, "INTERBANK",  "06090123456",  new DateTime(2024, 5, 20), "832109876", "luz.chavez@buildtruck.pe"),

            // ===== PROJECT 9 - Restaurante Gastronómico (EN ESTUDIO) =====
            Make(9, "Ricardo", "Medina Quispe",   "42890123", "Maestro Albañil",         "Construcción",   PersonnelType.SPECIALIST,      PersonnelStatus.PENDING,  3100m, "BCP",        "19001234567",  new DateTime(2026, 9, 1), "821098765", "ricardo.medina@buildtruck.pe"),
            Make(9, "Sofía",   "Guerrero Mamani", "48012345", "Técnica en Acabados",     "Acabados",       PersonnelType.TECHNICAL,       PersonnelStatus.PENDING,  2900m, "BBVA",       "01112345678",  new DateTime(2026, 9, 1), "810987654", "sofia.guerrero@buildtruck.pe"),

            // ===== PROJECT 10 - Centro Educativo Nacional =====
            Make(10, "Andrés", "Ruiz Condori",   "43901234", "Jefe de Cuadrilla",        "Construcción",   PersonnelType.SPECIALIST,      PersonnelStatus.ACTIVE,  3700m, "SCOTIABANK", "00901234567",  new DateTime(2025, 9, 15), "809876543", "andres.ruiz@buildtruck.pe"),
            Make(10, "Isabel", "Moreno Apaza",   "46012345", "Peona General",            "Construcción",   PersonnelType.LABORER,         PersonnelStatus.ACTIVE,  2100m, "BCP",        "19012345670",  new DateTime(2025, 9, 15), "798765432", "isabel.moreno@buildtruck.pe"),
        };

        // Mark attendance for current month — only ACTIVE personnel
        // Days marked: 1 through yesterday (day before today)
        int daysToMark = now.Day - 1;

        MarkAttendance(seedPersonnel[0],  year, month, daysToMark, absenceDays: [10]);
        MarkAttendance(seedPersonnel[1],  year, month, daysToMark, absenceDays: [15, 20]);
        MarkAttendance(seedPersonnel[2],  year, month, daysToMark, absenceDays: [5]);
        MarkAttendance(seedPersonnel[3],  year, month, daysToMark, absenceDays: []);
        // [4],[5] PENDING — no attendance
        MarkAttendance(seedPersonnel[6],  year, month, daysToMark, absenceDays: [8]);
        MarkAttendance(seedPersonnel[7],  year, month, daysToMark, absenceDays: [3, 17]);
        // [8],[9] PENDING — no attendance
        MarkAttendance(seedPersonnel[10], year, month, daysToMark, absenceDays: []);
        MarkAttendance(seedPersonnel[11], year, month, daysToMark, absenceDays: [12]);
        MarkAttendance(seedPersonnel[12], year, month, daysToMark, absenceDays: []);
        MarkAttendance(seedPersonnel[13], year, month, daysToMark, absenceDays: [18]);
        MarkAttendance(seedPersonnel[14], year, month, daysToMark, absenceDays: [6]);
        MarkAttendance(seedPersonnel[15], year, month, daysToMark, absenceDays: []);
        // [16],[17] PENDING — no attendance
        MarkAttendance(seedPersonnel[18], year, month, daysToMark, absenceDays: [11]);
        MarkAttendance(seedPersonnel[19], year, month, daysToMark, absenceDays: [4, 19]);

        context.Personnel.AddRange(seedPersonnel);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ PERSONNEL SEED DATA CREATED - {seedPersonnel.Count} records");
    }

    private static PersonnelEntity Make(
        int projectId, string name, string lastname, string documentNumber,
        string position, string department, PersonnelType type, PersonnelStatus status,
        decimal monthlyAmount, string bank, string accountNumber,
        DateTime startDate, string phone, string email)
    {
        var p = new PersonnelEntity(projectId, name, lastname, documentNumber, position, department, type, status);
        p.UpdateFinancialInfo(monthlyAmount, 0m, bank, accountNumber);
        p.UpdateContactInfo(phone, email);
        p.UpdateContractInfo(startDate, null, status);
        return p;
    }

    private static void MarkAttendance(PersonnelEntity p, int year, int month, int daysToMark, int[] absenceDays)
    {
        if (daysToMark <= 0) return;

        p.InitializeMonthAttendance(year, month);
        p.AutoMarkSundays(year, month);

        int limit = Math.Min(daysToMark, DateTime.DaysInMonth(year, month));
        for (int day = 1; day <= limit; day++)
        {
            if (new DateTime(year, month, day).DayOfWeek == DayOfWeek.Sunday) continue;
            p.SetDayAttendance(year, month, day,
                absenceDays.Contains(day) ? AttendanceStatus.F : AttendanceStatus.X);
        }
    }
}
