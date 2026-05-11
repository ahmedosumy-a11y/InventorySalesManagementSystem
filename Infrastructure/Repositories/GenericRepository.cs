using System.Linq.Expressions;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesManagementSystem.Infrastructure.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .AsNoTracking() 
            .FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> GetByIdAsTrackingAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .FirstOrDefaultAsync(predicate);
    }

    // public async Task<T?> GetByIdAsync(params object[] keyValues)
    // {
    //     return await _dbSet.FindAsync(keyValues);
    // }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    // public void Update(T entity)
    // {
    //     _dbSet.Update(entity);
    // }
    public void Update(T entity)
{
    var entry = _dbSet.Entry(entity);

    if (entry.State == EntityState.Detached)
        _dbSet.Attach(entity);

    entry.State = EntityState.Modified;
}

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}
