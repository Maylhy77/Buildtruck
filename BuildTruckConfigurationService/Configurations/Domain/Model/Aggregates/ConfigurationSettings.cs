using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;

namespace BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;

/// <summary>
/// Represents a user's configuration settings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public Theme Themes { get; set; }

    public Plan Plans { get; set; }

    public bool NotificationsEnable { get; set; } = true;
    public bool EmailNotifications { get; set; } = false;

    public TutorialProgress TutorialsCompleted { get; set; } = new TutorialProgress();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for User
    //[ForeignKey("UserId")]
    //public User User { get; set; }

    public ConfigurationSettings(int userId, Theme themes, Plan plans, bool notificationsEnable, bool emailNotifications, TutorialProgress? tutorialsCompleted = null)
    {
        UserId = userId;
        Themes = themes;
        Plans = plans;
        NotificationsEnable = notificationsEnable;
        EmailNotifications = emailNotifications;
        TutorialsCompleted = tutorialsCompleted ?? new TutorialProgress();
    }
    
    // Constructors
    public ConfigurationSettings()
    {
        // Required by EF Core
    }
    
}
