namespace BuildTruckShared.Domain.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync();
}
