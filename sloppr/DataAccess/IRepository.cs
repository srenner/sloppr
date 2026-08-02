using System.Linq.Expressions;

namespace sloppr.DataAccess;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    /// <example>
    /// <code>
    /// var user = await repository.GetByIdAsync(1, u => u.Orders);
    /// </code>
    /// </example>
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}