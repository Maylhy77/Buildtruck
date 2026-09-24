using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Interfaces.REST.Resources;

namespace BuildTruckUserService.Users.Interfaces.REST.Transform;

/// <summary>
/// Assembler to convert User entity to UserResource
/// </summary>
/// <remarks>
/// Transforms Domain Entity to REST DTO
/// </remarks>
public static class UserResourceFromEntityAssembler
{
    /// <summary>
    /// Convert User entity to UserResource
    /// </summary>
    /// <param name="entity">The user entity</param>
    /// <returns>The user resource DTO</returns>
    public static UserResource ToResourceFromEntity(User entity)
    {
        return new UserResource(
            entity.Id,
            entity.Email,                    // ✅ Corporate email
            entity.FullName,                 // ✅ Calculated by Value Object
            entity.Initials,                 // ✅ Calculated by Value Object
            entity.Name.FirstName,           // ✅ From PersonName Value Object
            entity.Name.LastName,            // ✅ From PersonName Value Object
            entity.Role.Role,                // ✅ From UserRole Value Object
            entity.PersonalEmail,            // ✅ From ContactInfo Value Object
            entity.Phone,                    // ✅ From ContactInfo Value Object
            entity.ProfileImageUrl,
            entity.IsActive,
            entity.LastLogin,
            entity.CreatedDate,
            entity.UpdatedDate,
            entity.PasswordHash              // ✅ For testing (null in production)
        );
    }

    /// <summary>
    /// Convert multiple User entities to UserResource list
    /// </summary>
    /// <param name="entities">The user entities</param>
    /// <returns>The user resource DTOs</returns>
    public static IEnumerable<UserResource> ToResourceFromEntity(IEnumerable<User> entities)
    {
        return entities.Select(ToResourceFromEntity);
    }
}