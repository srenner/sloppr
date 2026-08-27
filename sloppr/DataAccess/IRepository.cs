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

    /// <summary>
    /// Gets entity collection according to its filtering rules defined on
    /// <see cref="DataAccess.ApplicationDbContext.OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder)"/>
    /// </summary>
    Task<IEnumerable<T>> GetFilteredAsync();

    /// <summary>
    /// Gets all entities using IgnoreQueryFilters()
    /// </summary>
    /// <seealso cref="DataAccess.ApplicationDbContext.OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder)"/> 
    /// <returns></returns>
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}