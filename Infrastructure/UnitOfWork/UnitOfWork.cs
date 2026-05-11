using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using InventorySalesManagementSystem.Infrastructure.Data;
using InventorySalesManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventorySalesManagementSystem.Infrastructure.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public IGenericRepository<User> Users { get; } = new GenericRepository<User>(context);
    public IGenericRepository<Role> Roles { get; } = new GenericRepository<Role>(context);
    public IGenericRepository<Warehouse> Warehouses { get; } = new GenericRepository<Warehouse>(context);
    public IGenericRepository<Category> Categories { get; } = new GenericRepository<Category>(context);
    public IGenericRepository<Product> Products { get; } = new GenericRepository<Product>(context);
    public IGenericRepository<Inventory> Inventories { get; } = new GenericRepository<InventorySalesManagementSystem.Domain.Entities.Inventory>(context);
    public IGenericRepository<Order> Orders { get; } = new GenericRepository<Order>(context);
    public IGenericRepository<OrderItem> OrderItems { get; } = new GenericRepository<OrderItem>(context);
    public IGenericRepository<Sale> Sales { get; } = new GenericRepository<Sale>(context);
    public IGenericRepository<RefreshToken> RefreshTokens { get; } = new GenericRepository<RefreshToken>(context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }
}
