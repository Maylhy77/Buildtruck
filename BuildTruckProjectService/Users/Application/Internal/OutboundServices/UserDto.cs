namespace BuildTruckProjectService.Users.Application.Internal.OutboundServices;

public record UserDto(
    int Id, string FirstName, string LastName, string FullName,
    string Email, string Role, bool IsActive,
    string? ProfileImageUrl, DateTime? LastLogin,
    string? PersonalEmail, string? Phone
);
