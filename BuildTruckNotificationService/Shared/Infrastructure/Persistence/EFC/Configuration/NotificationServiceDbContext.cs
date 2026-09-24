using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Configuration;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckNotificationService.Shared.Infrastructure.Persistence.EFC.Configuration;

public class NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        builder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new NotificationConfiguration());
        builder.ApplyConfiguration(new NotificationDeliveryConfiguration());
        builder.ApplyConfiguration(new NotificationPreferenceConfiguration());
    }
}
