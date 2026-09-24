using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Configuration;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries");
        builder.HasKey(nd => nd.Id);
        builder.Property(nd => nd.Id).ValueGeneratedOnAdd();
        builder.Property(nd => nd.NotificationId).IsRequired();

        builder.Property(nd => nd.Channel)
            .HasConversion(c => c.Value, c => NotificationChannel.FromString(c))
            .HasMaxLength(20).IsRequired();

        builder.Property(nd => nd.Status)
            .HasConversion(s => s.Value, s => DeliveryStatus.FromString(s))
            .HasMaxLength(20).IsRequired();

        builder.Property(nd => nd.AttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(nd => nd.LastAttemptAt);
        builder.Property(nd => nd.SentAt);
        builder.Property(nd => nd.ErrorMessage).HasMaxLength(500);
        builder.Property(nd => nd.CreatedDate).HasColumnName("CreatedAt").IsRequired();
        builder.Property(nd => nd.UpdatedDate).HasColumnName("UpdatedAt");

        builder.HasIndex(nd => nd.NotificationId);
        builder.HasIndex(nd => new { nd.NotificationId, nd.Channel }).IsUnique();
        builder.HasIndex(nd => nd.Status);
        builder.HasIndex(nd => nd.Channel);

        builder.HasOne<Notification>()
            .WithMany()
            .HasForeignKey(nd => nd.NotificationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_NotificationDeliveries_Notifications_NotificationId");
    }
}
