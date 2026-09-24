using BuildTruckDocumentationService.Shared.Infrastructure.Persistence.EFC.Configuration;
using DocumentationEntity = BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates.Documentation;

namespace BuildTruckDocumentationService.Shared.Infrastructure.Persistence.EFC.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DocumentationServiceDbContext context)
    {
        if (context.Documentation.Any()) return;

        var seedDocs = new[]
        {
            new DocumentationEntity(1, "Plan de Seguridad", "Plan integral de seguridad para el proyecto", "https://res.cloudinary.com/dummy/image/upload/v1/plan-seguridad.pdf", DateTime.UtcNow.AddDays(-30), 2),
            new DocumentationEntity(1, "Planos Arquitectónicos", "Planos A01-A10 del proyecto", "https://res.cloudinary.com/dummy/image/upload/v1/planos-arq.pdf", DateTime.UtcNow.AddDays(-25), 2),
            new DocumentationEntity(1, "Especificaciones Técnicas", "Especificaciones técnicas de materiales", "https://res.cloudinary.com/dummy/image/upload/v1/especif-tecnicas.pdf", DateTime.UtcNow.AddDays(-20), 2),
            new DocumentationEntity(1, "Cronograma de Obra", "Cronograma detallado de 12 meses", "https://res.cloudinary.com/dummy/image/upload/v1/cronograma.pdf", DateTime.UtcNow.AddDays(-15), 2),
            new DocumentationEntity(1, "Acta de Inicio", "Acta de inicio de obra firmada", "https://res.cloudinary.com/dummy/image/upload/v1/acta-inicio.pdf", DateTime.UtcNow.AddDays(-10), 2),
            new DocumentationEntity(1, "Certificado de Permisos", "Permisos municipales y ambientales", "https://res.cloudinary.com/dummy/image/upload/v1/permisos.pdf", DateTime.UtcNow.AddDays(-8), 2),
            new DocumentationEntity(1, "Reportes Mensuales - Junio", "Reporte de avance mes de junio", "https://res.cloudinary.com/dummy/image/upload/v1/reporte-junio.pdf", DateTime.UtcNow.AddDays(-5), 5),
            new DocumentationEntity(1, "Inspección de Calidad", "Inspecciones de calidad realizadas", "https://res.cloudinary.com/dummy/image/upload/v1/inspeccion-calidad.pdf", DateTime.UtcNow.AddDays(-2), 4),
            new DocumentationEntity(1, "Fotos de Avance", "Galería fotográfica del proyecto", "https://res.cloudinary.com/dummy/image/upload/v1/fotos-avance.pdf", DateTime.UtcNow.AddDays(-1), 5),
            new DocumentationEntity(1, "Manual de Mantenimiento", "Manual para mantenimiento de estructuras", "https://res.cloudinary.com/dummy/image/upload/v1/manual-mtto.pdf", DateTime.UtcNow, 4),
            new DocumentationEntity(2, "Plan de Restauración", "Plan detallado de restauración", "https://res.cloudinary.com/dummy/image/upload/v1/plan-restauracion.pdf", DateTime.UtcNow.AddDays(-28), 2),
            new DocumentationEntity(2, "Planos Históricos", "Planos originales del edificio", "https://res.cloudinary.com/dummy/image/upload/v1/planos-historicos.pdf", DateTime.UtcNow.AddDays(-20), 2),
            new DocumentationEntity(2, "Estudio de Suelos", "Análisis geotécnico del terreno", "https://res.cloudinary.com/dummy/image/upload/v1/estudio-suelos.pdf", DateTime.UtcNow.AddDays(-18), 2),
            new DocumentationEntity(2, "Presupuesto Detallado", "Presupuesto por partidas", "https://res.cloudinary.com/dummy/image/upload/v1/presupuesto.pdf", DateTime.UtcNow.AddDays(-12), 2),
            new DocumentationEntity(2, "Acta de Reunión Semanal", "Acta de coordinación semanal", "https://res.cloudinary.com/dummy/image/upload/v1/acta-reunion.pdf", DateTime.UtcNow.AddDays(-3), 4),
            new DocumentationEntity(3, "Viabilidad del Proyecto", "Estudio de viabilidad comercial", "https://res.cloudinary.com/dummy/image/upload/v1/viabilidad.pdf", DateTime.UtcNow.AddDays(-25), 3),
            new DocumentationEntity(4, "Diseño Arquitectónico", "Diseño completo del hotel", "https://res.cloudinary.com/dummy/image/upload/v1/diseno-arq.pdf", DateTime.UtcNow.AddDays(-22), 2),
            new DocumentationEntity(5, "Estudio de Impacto Ambiental", "EIA del complejo residencial", "https://res.cloudinary.com/dummy/image/upload/v1/eia.pdf", DateTime.UtcNow.AddDays(-19), 3),
            new DocumentationEntity(5, "Licencia Ambiental", "Licencia otorgada por autoridad ambiental", "https://res.cloudinary.com/dummy/image/upload/v1/licencia-ambiental.pdf", DateTime.UtcNow.AddDays(-14), 3),
            new DocumentationEntity(6, "Estudio de Tráfico", "Análisis de impacto vial", "https://res.cloudinary.com/dummy/image/upload/v1/estudio-trafico.pdf", DateTime.UtcNow.AddDays(-21), 2),
        };

        context.Documentation.AddRange(seedDocs);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ DOCUMENTATION SEED DATA CREATED - 20 documents");
    }
}
