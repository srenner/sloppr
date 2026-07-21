namespace sloppr.DataAccess;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> CompleteAsync();
}
