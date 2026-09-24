using BuildTruckDocumentationService.Documentation.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

using DocumentationDocument = BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates.Documentation;

namespace BuildTruckDocumentationService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class DocumentationServiceDbContext(DbContextOptions<DocumentationServiceDbContext> options) : DbContext(options)
{
    public DbSet<DocumentationDocument> Documentation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new DocumentationConfiguration());
    }
}
