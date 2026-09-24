using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.Queries;
using BuildTruckUserService.Users.Domain.Repositories;
using BuildTruckUserService.Users.Domain.Services;

namespace BuildTruckUserService.Users.Application.Internal.QueryServices;

/**
 * <summary>
 *     The user query service implementation
 * </summary>
 * <remarks>
 *     This service handles user queries for the BuildTruck platform
 * </remarks>
 */
public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    /**
     * <summary>
     *     Handle get user by id query
     * </summary>
     * <param name="query">The query object containing the user id to search</param>
     * <returns>The user</returns>
     */
    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await userRepository.FindByIdAsync(query.Id);
    }

    /**
     * <summary>
     *     Handle get all users query
     * </summary>
     * <param name="query">The query object for getting all users</param>
     * <returns>The list of users</returns>
     */
    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query)
    {
        return await userRepository.ListAsync();
    }
}