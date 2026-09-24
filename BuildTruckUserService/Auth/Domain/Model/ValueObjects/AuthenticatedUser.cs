using BuildTruckUserService.Users.Domain.Model.Aggregates;

namespace BuildTruckUserService.Auth.Domain.Model.ValueObjects;

public record AuthenticatedUser
{
    public int Id { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }
    public string Role { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? LastLogin { get; init; }

    public AuthenticatedUser(int id, string fullName, string email, string role, string? profileImageUrl = null, DateTime? lastLogin = null)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("FullName cannot be null or empty.", nameof(fullName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        Id = id;
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role.Trim();
        ProfileImageUrl = profileImageUrl?.Trim();
        LastLogin = lastLogin;
    }

    // Constructor que recibe Users.User (para conversión desde ACL)
    public AuthenticatedUser(User user) : this(
        user.Id,
        user.FullName,
        user.Email,
        user.Role.ToString(),
        user.ProfileImageUrl,
        user.LastLogin)
    {
    }

    // Métodos de dominio AUTH
    public bool IsAdmin() => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsManager() => Role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
    public bool IsSupervisor() => Role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase);
    public bool IsWorker() => Role.Equals("Worker", StringComparison.OrdinalIgnoreCase);
    
    public bool HasRole(string roleName) => Role.Equals(roleName, StringComparison.OrdinalIgnoreCase);
    
    // Métodos de autorización basados en UserRole
    public bool CanManageProjects() => IsAdmin() || IsManager();
    public bool CanBeAssignedToProject() => IsSupervisor();
    
    public bool IsActive => true; // Los usuarios inactivos no pueden autenticarse
    
    public string GetDisplayRole() => Role switch
    {
        "Admin" => "Administrador",
        "Manager" => "Gerente",
        "Supervisor" => "Supervisor",
        "Worker" => "Trabajador", 
        _ => Role
    };
}