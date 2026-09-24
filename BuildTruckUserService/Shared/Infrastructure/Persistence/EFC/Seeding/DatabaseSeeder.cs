using BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckUserService.Users.Domain.Model.Aggregates;

namespace BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(UserServiceDbContext context)
    {
        if (context.Users.Any()) return;

        var seedUsers = new[]
        {
            // Admin
            new User("Carlos", "Mendoza", "Admin",
                BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                personalEmail: "carlos.mendoza.personal@gmail.com",
                phone: "987654321"),

            // Manager
            new User("Ana", "Torres", "Manager",
                BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                personalEmail: "ana.torres.personal@gmail.com",
                phone: "987654322"),

            // Manager 2
            new User("Roberto", "Vargas", "Manager",
                BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                personalEmail: "roberto.vargas.personal@gmail.com",
                phone: "987654323"),

            // Supervisor
            new User("Maria", "Lopez", "Supervisor",
                BCrypt.Net.BCrypt.HashPassword("Supervisor123!"),
                personalEmail: "maria.lopez.personal@gmail.com",
                phone: "987654324"),

            // Supervisor 2
            new User("Jorge", "Ramirez", "Supervisor",
                BCrypt.Net.BCrypt.HashPassword("Supervisor123!"),
                personalEmail: "jorge.ramirez.personal@gmail.com",
                phone: "987654325"),

            // Worker
            new User("Luis", "Garcia", "Worker",
                BCrypt.Net.BCrypt.HashPassword("Worker123!"),
                personalEmail: "luis.garcia.personal@gmail.com",
                phone: "987654326"),

            // Worker 2
            new User("Carmen", "Flores", "Worker",
                BCrypt.Net.BCrypt.HashPassword("Worker123!"),
                personalEmail: "carmen.flores.personal@gmail.com",
                phone: "987654327"),

            // Worker 3
            new User("Pedro", "Castillo", "Worker",
                BCrypt.Net.BCrypt.HashPassword("Worker123!"),
                personalEmail: "pedro.castillo.personal@gmail.com",
                phone: "987654328"),
        };

        context.Users.AddRange(seedUsers);
        await context.SaveChangesAsync();

        Console.WriteLine("=== SEED DATA CREATED ===");
        foreach (var u in seedUsers)
            Console.WriteLine($"  [{u.Role}] {u.FullName} → {u.Email} / password: {GetPasswordForRole(u.Role.ToString())}");
        Console.WriteLine("=========================");
    }

    private static string GetPasswordForRole(string role) => role switch
    {
        "Admin" => "Admin123!",
        "Manager" => "Manager123!",
        "Supervisor" => "Supervisor123!",
        _ => "Worker123!"
    };
}
