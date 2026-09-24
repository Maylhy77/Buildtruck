using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.ValueObjects;
using BuildTruckShared.Domain.Repositories;
using BuildTruckUserService.Users.Domain.Model.Commands;

namespace BuildTruckUserService.Users.Domain.Repositories;

/**
 * <summary>
 *     The user repository interface
 * </summary>
 * <remarks>
 *     This repository manages users in the BuildTruck platform using Value Objects
 * </remarks>
 */
public interface IUserRepository : IBaseRepository<User>
{
    /**
     * <summary>
     *     Find user by email
     * </summary>
     * <param name="email">The email to search</param>
     * <returns>The user if found, null otherwise</returns>
     */
    Task<User?> FindByEmailAsync(EmailAddress email);

    /**
     * <summary>
     *     Check if email exists
     * </summary>
     * <param name="email">The email to check</param>
     * <returns>True if email exists, false otherwise</returns>
     */
    Task<bool> ExistsByEmailAsync(EmailAddress email);

    /**
     * <summary>
     *     Find users by role
     * </summary>
     * <param name="role">The role to search</param>
     * <returns>List of users with the specified role</returns>
     */
    Task<IEnumerable<User>> FindByRoleAsync(UserRole role);
    
    
    /// <summary>
    /// Remove user from database (physical deletion)
    /// </summary>
    /// <param name="user">User to remove</param>
    void Remove(User user);
    
}