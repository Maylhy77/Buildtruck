using BuildTruckShared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork
    where TContext : DbContext
{
    public async Task CompleteAsync() => await context.SaveChangesAsync();
}
