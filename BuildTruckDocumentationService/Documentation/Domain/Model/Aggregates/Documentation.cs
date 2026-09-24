namespace BuildTruckDocumentationService.Documentation.Domain.Model.Aggregates;

/// <summary>
/// Documentation Aggregate Root - Clean (without Data Annotations)
/// </summary>
public partial class Documentation
{
    public int Id { get; private set; }

    // Basic Information
    public int ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ImagePath { get; private set; } = string.Empty; // Cloudinary URL
    public DateTime Date { get; private set; }
    public int CreatedBy { get; private set; }

    // Audit Fields
    public bool IsDeleted { get; private set; }

    // Constructors
    public Documentation()
    {
        // Required by EF Core
    }

    public Documentation(int projectId, string title, string description, string imagePath, DateTime date, int createdBy)
    {
        ProjectId = projectId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        ImagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
        Date = date;
        CreatedBy = createdBy;
        IsDeleted = false;
    }

    // Business Methods
    public void UpdateBasicInfo(string title, string description, DateTime date)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Date = date;
    }

    public void UpdateImage(string imagePath)
    {
        ImagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    // Validation
    public bool IsActive() => !IsDeleted;
    
    public bool BelongsToProject(int projectId) => ProjectId == projectId && !IsDeleted;

    public bool HasValidImage() => !string.IsNullOrEmpty(ImagePath) && ImagePath.Contains("cloudinary.com");
}