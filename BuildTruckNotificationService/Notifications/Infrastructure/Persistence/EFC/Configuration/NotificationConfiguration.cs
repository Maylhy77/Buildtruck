using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildTruckNotificationService.Notifications.Infrastructure.Persistence.EFC.Configuration;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedOnAdd();
        builder.Property(n => n.UserId).IsRequired();

        builder.Property(n => n.Type)
            .HasConversion(t => t.Value, t => NotificationType.FromString(t))
            .HasMaxLength(50).IsRequired();

        builder.Property(n => n.Context)
            .HasConversion(c => c.Value, c => NotificationContext.FromString(c))
            .HasMaxLength(20).IsRequired();

        builder.Property(n => n.Priority)
            .HasConversion(p => p.Value, p => NotificationPriority.FromString(p))
            .HasMaxLength(20).IsRequired();

        builder.Property(n => n.Status)
            .HasConversion(s => s.Value, s => NotificationStatus.FromString(s))
            .HasMaxLength(20).IsRequired();

        builder.Property(n => n.Scope)
            .HasConversion(s => s.Value, s => NotificationScope.FromString(s))
            .HasMaxLength(20).IsRequired();

        builder.Property(n => n.TargetRole)
            .HasConversion(r => r.Value, r => UserRole.FromString(r))
            .HasMaxLength(20).IsRequired();

        builder.OwnsOne(n => n.Content, content =>
        {
            content.WithOwner().HasForeignKey("Id");
            content.Property(c => c.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
            content.Property(c => c.Message).HasColumnName("Message").HasMaxLength(500).IsRequired();
            content.Property(c => c.ActionUrl).HasColumnName("ActionUrl").HasMaxLength(300);
            content.Property(c => c.ActionText).HasColumnName("ActionText").HasMaxLength(100);
            content.Property(c => c.IconClass).HasColumnName("IconClass").HasMaxLength(100);
        });

        builder.Property(n => n.RelatedProjectId);
        builder.Property(n => n.RelatedEntityId);
        builder.Property(n => n.RelatedEntityType).HasMaxLength(50);
        builder.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
        builder.Property(n => n.ReadAt);
        builder.Property(n => n.MetadataJson).HasColumnType("longtext");
        builder.Property(n => n.CreatedDate).HasColumnName("CreatedAt").IsRequired();
        builder.Property(n => n.UpdatedDate).HasColumnName("UpdatedAt");

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.Context });
        builder.HasIndex(n => n.RelatedProjectId);
        builder.HasIndex(n => n.CreatedDate);
    }
}
