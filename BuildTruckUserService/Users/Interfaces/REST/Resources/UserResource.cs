namespace BuildTruckUserService.Users.Interfaces.REST.Resources;

/// <summary>
/// User Resource DTO
/// </summary>
/// <remarks>
/// Represents user data returned by REST API with computed properties
/// </remarks>
public record UserResource(
    int Id,
    string Email,                    // ✅ Corporate email
    string FullName,                 // ✅ Calculated in backend
    string Initials,                 // ✅ Calculated in backend
    string Name,
    string LastName,
    string Role,
    string? PersonalEmail,           // ✅ Optional personal email
    string? Phone,                   // ✅ Optional phone
    string? ProfileImageUrl,         // ✅ Cloudinary URL
    bool IsActive,
    DateTime? LastLogin,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? PasswordHash = null      // ✅ Only for testing (remove [JsonIgnore] in User.cs)
);