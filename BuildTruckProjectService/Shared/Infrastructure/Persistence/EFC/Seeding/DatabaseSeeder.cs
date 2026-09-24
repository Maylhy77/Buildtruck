using BuildTruckProjectService.Projects.Domain.Model.Aggregates;
using BuildTruckProjectService.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace BuildTruckProjectService.Shared.Infrastructure.Persistence.EFC.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ProjectServiceDbContext context)
    {
        if (context.Projects.Any()) return;

        var seedProjects = new[]
        {
            new Project("Torres Miraflores", "Torres en miraflores de 20 pisos con vista a parque", 2, "Calle José Olaya 141, Miraflores, Lima", DateTime.Now, null, null, "Activo"),
            new Project("Edificio Giacoletti", "Restauración del histórico edificio Giacoletti", 2, "Avenida Nicolás de Piérola, Centro, Lima", DateTime.Now, null, null, "Activo"),
            new Project("Centro Comercial San Isidro", "Construcción de nuevo centro comercial", 3, "Avenida Paseo de la República, San Isidro, Lima", DateTime.Now.AddDays(7), null, null, "Planificado"),
            new Project("Hotel Luxury Cusco", "Construcción de hotel 5 estrellas en Cusco", 2, "Plaza de Armas, Cusco", DateTime.Now.AddDays(14), null, null, "Planificado"),
            new Project("Complejo Residencial La Molina", "Complejo residencial de 15 torres", 3, "La Molina, Lima", DateTime.Now.AddDays(21), null, null, "En estudio"),
            new Project("Autopista Metropolitana", "Ampliación de autopista metropolitana", 2, "Ruta Panamericana Sur, Lima", DateTime.Now.AddDays(30), null, null, "En estudio"),
            new Project("Puente Inca", "Construcción de puente sobre río Rímac", 3, "Zona industrial, Callao", DateTime.Now.AddDays(45), null, null, "Planificado"),
            new Project("Centro de Salud Metropolitano", "Hospital moderno de 300 camas", 2, "Carabayllo, Lima", DateTime.Now, null, null, "Activo"),
            new Project("Restaurante Gastronómico", "Restaurante de alta cocina con 200 puestos", 3, "Barranco, Lima", DateTime.Now.AddDays(60), null, null, "En estudio"),
            new Project("Centro Educativo Nacional", "Escuela de 2000 estudiantes", 2, "Lurín, Lima", DateTime.Now.AddDays(90), null, null, "Planificado"),
        };

        foreach (var p in seedProjects)
        {
            int supervisorId = seedProjects.IndexOf(p) % 2 == 0 ? 5 : 4;
            p.AssignSupervisor(supervisorId);
        }

        context.Projects.AddRange(seedProjects);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ PROJECT SEED DATA CREATED - 10 projects");
    }
}
