using System.Text.Json.Serialization;
using BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

namespace BuildTruckProjectService.Projects.Domain.Model.Aggregates;

/// <summary>
/// Project Aggregate Root
/// </summary>
/// <remarks>
/// Represents a construction project with all its details and business rules
/// </remarks>
public partial class Project
{
    public int Id { get; }
    public ProjectName Name { get; private set; }
    public ProjectDescription Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public int ManagerId { get; private set; }
    public ProjectLocation Location { get; private set; }
    public ProjectCoordinates? Coordinates { get; private set; }
    public DateTime? StartDate { get; private set; }
    public int? SupervisorId { get; private set; }
    public ProjectState State { get; private set; }

    // Calculated properties for convenience
    public string ProjectName => Name.Name;
    public string ProjectDescription => Description.Description;
    public string ProjectLocation => Location.Location;
    public string ProjectState => State.State;
    public bool HasSupervisor => SupervisorId.HasValue;
    public bool HasCoordinates => Coordinates != null && !Coordinates.IsZero();
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);

    // Default constructor for EF
    public Project()
    {
        Name = new ProjectName();
        Description = new ProjectDescription();
        Location = new ProjectLocation();
        State = new ProjectState();
        ImageUrl = null;
    }

    // Constructor for creating new projects
    public Project(string name, string description, int managerId, string location, 
                   DateTime? startDate = null, ProjectCoordinates? coordinates = null, 
                   string? imageUrl = null, string state = "En estudio")
    {
        Name = new ProjectName(name);
        Description = new ProjectDescription(description);
        ManagerId = managerId > 0 ? managerId : throw new ArgumentException("ManagerId must be greater than 0", nameof(managerId));
        Location = new ProjectLocation(location);
        StartDate = startDate;
        Coordinates = coordinates;
        ImageUrl = imageUrl;
        State = new ProjectState(state);
        SupervisorId = null; // Will be assigned later
    }

    // Business methods for updating project details
    public Project UpdateDetails(string? name = null, string? description = null, 
                                string? location = null, DateTime? startDate = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = new ProjectName(name);

        if (!string.IsNullOrWhiteSpace(description))
            Description = new ProjectDescription(description);

        if (!string.IsNullOrWhiteSpace(location))
            Location = new ProjectLocation(location);

        if (startDate.HasValue)
            StartDate = startDate;

        return this;
    }

    public Project UpdateCoordinates(ProjectCoordinates? coordinates)
    {
        Coordinates = coordinates;
        return this;
    }

    public Project UpdateImage(string? imageUrl)
    {
        ImageUrl = imageUrl;
        return this;
    }

    public Project ChangeState(string newState)
    {
        var newProjectState = new ProjectState(newState);
        
        if (!State.CanTransitionTo(newProjectState))
            throw new InvalidOperationException($"Cannot transition from {State} to {newState}");

        // Business rule: Active projects must have a supervisor
        if (newProjectState.IsActive && !HasSupervisor)
            throw new InvalidOperationException("Cannot activate project without assigning a supervisor");

        State = newProjectState;
        return this;
    }

    public Project AssignSupervisor(int supervisorId)
    {
        if (supervisorId <= 0)
            throw new ArgumentException("SupervisorId must be greater than 0", nameof(supervisorId));

        SupervisorId = supervisorId;
        return this;
    }

    public Project RemoveSupervisor()
    {
        // Business rule: Cannot remove supervisor from active projects
        if (State.IsActive)
            throw new InvalidOperationException("Cannot remove supervisor from active project");

        SupervisorId = null;
        return this;
    }

    public Project ChangeSupervisor(int newSupervisorId)
    {
        if (newSupervisorId <= 0)
            throw new ArgumentException("SupervisorId must be greater than 0", nameof(newSupervisorId));

        SupervisorId = newSupervisorId;
        return this;
    }

    // Business validation methods
    public bool IsReadyToStart()
    {
        return !string.IsNullOrWhiteSpace(ProjectName) &&
               !string.IsNullOrWhiteSpace(ProjectDescription) &&
               !string.IsNullOrWhiteSpace(ProjectLocation) &&
               StartDate.HasValue &&
               HasSupervisor;
    }

    public bool CanBeDeleted()
    {
        // Only projects that are not completed can be deleted
        return !State.IsCompleted;
    }

    public bool RequiresSupervisorChange(int? newSupervisorId)
    {
        return SupervisorId != newSupervisorId;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!Name.IsValid())
            errors.Add("Project name is invalid");

        if (!Description.IsValid())
            errors.Add("Project description is invalid");

        if (!Location.IsValid())
            errors.Add("Project location is invalid");

        if (!State.IsValid())
            errors.Add("Project state is invalid");

        if (ManagerId <= 0)
            errors.Add("Manager ID is required");

        if (State.IsActive && !HasSupervisor)
            errors.Add("Active projects must have a supervisor assigned");

        if (StartDate.HasValue && StartDate < DateTime.Now.Date.AddDays(-1))
            errors.Add("Start date cannot be in the past");

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    // Calculated business properties
    public int DaysUntilStart()
    {
        if (!StartDate.HasValue)
            return 0;

        var days = (StartDate.Value.Date - DateTime.Now.Date).Days;
        return Math.Max(0, days);
    }

    public bool IsOverdue()
    {
        return StartDate.HasValue && 
               StartDate.Value.Date < DateTime.Now.Date && 
               !State.IsCompleted;
    }

    public string GetProjectSummary()
    {
        var summary = $"{ProjectName} - {State.GetDisplayName()}";
        
        if (HasSupervisor)
            summary += $" (Supervisor: {SupervisorId})";
            
        if (StartDate.HasValue)
        {
            var daysUntilStart = DaysUntilStart();
            if (daysUntilStart > 0)
                summary += $" - Starts in {daysUntilStart} days";
            else if (IsOverdue())
                summary += " - Overdue";
        }

        return summary;
    }

    // Equality and comparison (based on business identity)
    public override bool Equals(object? obj)
    {
        if (obj is not Project other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id > 0 && other.Id > 0 && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id > 0 ? Id.GetHashCode() : base.GetHashCode();
    }

    public override string ToString() => GetProjectSummary();
}