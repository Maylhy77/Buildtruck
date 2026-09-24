using System.Text.Json;

namespace BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;

public class TutorialProgress
{
    private Dictionary<string, bool> _tutorials;
    
    public static readonly string[] ValidTutorials = { "admin", "supervisor", "manager", "manager-projects" };

    // â† AGREGAR constructor sin parÃ¡metros para EF Core:
    public TutorialProgress()
    {
        _tutorials = new Dictionary<string, bool>();
    }

    public TutorialProgress(string jsonValue = "{}")
    {
        if (string.IsNullOrWhiteSpace(jsonValue))
            jsonValue = "{}";
            
        _tutorials = ParseAndValidate(jsonValue);
    }

    public TutorialProgress(Dictionary<string, bool> tutorials)
    {
        _tutorials = ValidateStructure(tutorials) ?? new Dictionary<string, bool>();
    }

    // â† AGREGAR propiedad para EF Core mapping:
    public string JsonValue 
    { 
        get => ToJsonString(); 
        set => _tutorials = ParseAndValidate(value ?? "{}"); 
    }

    private Dictionary<string, bool> ParseAndValidate(string jsonValue)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, bool>>(jsonValue);
            return ValidateStructure(parsed) ?? new Dictionary<string, bool>();
        }
        catch (JsonException)
        {
            throw new ArgumentException($"Invalid JSON format for tutorials. Expected format: {{\"admin\": true, \"supervisor\": false}}");
        }
    }

    private Dictionary<string, bool> ValidateStructure(Dictionary<string, bool>? tutorials)
    {
        if (tutorials == null) return new Dictionary<string, bool>();

        // Validar que solo contenga claves vÃ¡lidas
        var invalidKeys = tutorials.Keys.Where(k => !ValidTutorials.Contains(k)).ToList();
        if (invalidKeys.Any())
        {
            throw new ArgumentException($"Invalid tutorial keys: {string.Join(", ", invalidKeys)}. Valid keys: {string.Join(", ", ValidTutorials)}");
        }

        return tutorials;
    }

    // MÃ©todos pÃºblicos
    public bool IsCompleted(string tutorialId)
    {
        return _tutorials.GetValueOrDefault(tutorialId, false);
    }

    public TutorialProgress MarkCompleted(string tutorialId)
    {
        if (!ValidTutorials.Contains(tutorialId))
            throw new ArgumentException($"Invalid tutorial ID: {tutorialId}");

        var newTutorials = new Dictionary<string, bool>(_tutorials)
        {
            [tutorialId] = true
        };
        return new TutorialProgress(newTutorials);
    }

    public TutorialProgress MarkIncomplete(string tutorialId)
    {
        if (!ValidTutorials.Contains(tutorialId))
            throw new ArgumentException($"Invalid tutorial ID: {tutorialId}");

        var newTutorials = new Dictionary<string, bool>(_tutorials)
        {
            [tutorialId] = false
        };
        return new TutorialProgress(newTutorials);
    }

    public string ToJsonString()
    {
        return JsonSerializer.Serialize(_tutorials);
    }

    public Dictionary<string, bool> GetAll()
    {
        return new Dictionary<string, bool>(_tutorials);
    }

    // Implicit conversions
    public static implicit operator string(TutorialProgress progress)
    {
        return progress.ToJsonString();
    }

    public static implicit operator TutorialProgress(string jsonValue)
    {
        return new TutorialProgress(jsonValue);
    }

    public override string ToString() => ToJsonString();
    
    public override bool Equals(object? obj)
    {
        if (obj is TutorialProgress other)
        {
            return ToJsonString() == other.ToJsonString();
        }
        return false;
    }

    public override int GetHashCode() => ToJsonString().GetHashCode();
}
