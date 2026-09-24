namespace BuildTruckProjectService.Projects.Domain.Model.ValueObjects;

/// <summary>
/// Project State Value Object
/// </summary>
/// <remarks>
/// Represents project states with validation and business rules
/// Matches frontend states: "En estudio", "Planificado", "Activo", "Completado"
/// </remarks>
public record ProjectState
{
    // Valid project states based on frontend
    private static readonly HashSet<string> ValidStates = new()
    {
        "En estudio",
        "Planificado", 
        "Activo",
        "Completado"
    };

    // Static instances for easy access
    public static readonly ProjectState EnEstudio = new("En estudio");
    public static readonly ProjectState Planificado = new("Planificado");
    public static readonly ProjectState Activo = new("Activo");
    public static readonly ProjectState Completado = new("Completado");

    public string State { get; init; }

    public ProjectState()
    {
        State = "En estudio"; // Default state
    }

    public ProjectState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("Project state cannot be null or empty.", nameof(state));

        if (!ValidStates.Contains(state))
            throw new ArgumentException($"Invalid project state: {state}. Valid states: {string.Join(", ", ValidStates)}", nameof(state));

        State = state;
    }

    // Business logic methods
    public bool IsInStudy => State == "En estudio";
    public bool IsPlanned => State == "Planificado";
    public bool IsActive => State == "Activo";
    public bool IsCompleted => State == "Completado";

    public bool CanTransitionTo(ProjectState newState)
    {
        return State switch
        {
            "En estudio" => newState.State is "Planificado" or "Activo", // Can skip to active
            "Planificado" => newState.State is "Activo" or "En estudio", // Can go back or forward
            "Activo" => newState.State is "Completado" or "Planificado", // Can complete or go back
            "Completado" => false, // Completed projects cannot change state
            _ => false
        };
    }

    public bool RequiresSupervisor()
    {
        // Only active projects require a supervisor
        return IsActive;
    }

    public string GetDisplayName()
    {
        return State switch
        {
            "En estudio" => "En Estudio",
            "Planificado" => "Planificado",
            "Activo" => "En Ejecución", // More descriptive for active
            "Completado" => "Completado",
            _ => State
        };
    }

    public string GetColorCode()
    {
        return State switch
        {
            "En estudio" => "#6c757d", // Gray
            "Planificado" => "#0d6efd", // Blue  
            "Activo" => "#28a745", // Green
            "Completado" => "#17a2b8", // Teal
            _ => "#6c757d"
        };
    }

    public static List<string> GetAllValidStates() => ValidStates.ToList();

    public bool IsValid() => ValidStates.Contains(State);

    public override string ToString() => State;

    // Implicit conversion operators for convenience
    public static implicit operator string(ProjectState projectState) => projectState.State;
    public static implicit operator ProjectState(string state) => new(state);
}