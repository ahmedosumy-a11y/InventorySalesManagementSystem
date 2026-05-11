using System.Linq.Expressions;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query();
    // Task<T?> GetByIdAsync(params object[] keyValues);
    Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsTrackingAsync(Expression<Func<T, bool>> predicate);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}
