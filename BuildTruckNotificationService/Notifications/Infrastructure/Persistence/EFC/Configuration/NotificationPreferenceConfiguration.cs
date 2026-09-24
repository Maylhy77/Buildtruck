using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Configuration;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(np => np.Id);
        builder.Property(np => np.Id).ValueGeneratedOnAdd();
        builder.Property(np => np.UserId).IsRequired();

        builder.Property(np => np.Context)
            .HasConversion(c => c.Value, c => NotificationContext.FromString(c))
            .HasMaxLength(20).IsRequired();

        builder.Property(np => np.InAppEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(np => np.EmailEnabled).IsRequired().HasDefaultValue(false);

        builder.Property(np => np.MinimumPriority)
            .HasConversion(p => p.Value, p => NotificationPriority.FromString(p))
            .HasMaxLength(20).IsRequired();

        builder.Property(np => np.CreatedDate).HasColumnName("CreatedAt").IsRequired();
        builder.Property(np => np.UpdatedDate).HasColumnName("UpdatedAt");

        builder.HasIndex(np => np.UserId);
        builder.HasIndex(np => new { np.UserId, np.Context }).IsUnique();
    }
}
