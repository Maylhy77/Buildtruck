using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckUserService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckUserService.Users.Domain.Model.Aggregates;
using BuildTruckUserService.Users.Domain.Model.ValueObjects;
using BuildTruckUserService.Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckUserService.Users.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository(UserServiceDbContext context)
    : BaseRepository<User, UserServiceDbContext>(context), IUserRepository
{
    public async Task<User?> FindByEmailAsync(EmailAddress email)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(user => user.CorporateEmail.Address == email.Address);
    }

    public async Task<bool> ExistsByEmailAsync(EmailAddress email)
    {
        return await Context.Set<User>()
            .AnyAsync(user => user.CorporateEmail.Address == email.Address);
    }

    public async Task<IEnumerable<User>> FindByRoleAsync(UserRole role)
    {
        return await Context.Set<User>()
            .Where(user => user.Role.Role == role.Role && user.IsActive)
            .ToListAsync();
    }

    public async Task<User?> FindByPersonNameAsync(PersonName personName)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.Name.FirstName == personName.FirstName
                                   && u.Name.LastName == personName.LastName);
    }
}
