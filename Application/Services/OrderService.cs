using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Orders;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using InventorySalesManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesManagementSystem.Application.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<OrderService> logger,
    ISlackService slackService) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await BuildOrderQuery()
            .AsNoTracking()
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await BuildOrderQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Order not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> GetByIdForUserAsync(int id, int userId, bool isPrivilegedUser, CancellationToken cancellationToken = default)
    {
        var query = BuildOrderQuery()
            .AsNoTracking()
            .Where(x => x.Id == id);

        if (!isPrivilegedUser)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var order = await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("Order not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(int userId, CancellationToken cancellationToken = default)
    {
        var orders = await BuildOrderQuery()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(int userId, OrderCreateDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var userExists = await unitOfWork.Users.Query().AnyAsync(x => x.Id == userId && x.IsActive, cancellationToken);
        if (!userExists)
        {
            throw new AppException("Active user not found.", System.Net.HttpStatusCode.NotFound);
        }

        var warehouseExists = await unitOfWork.Warehouses.Query().AnyAsync(x => x.Id == request.WarehouseId && x.IsActive, cancellationToken);
        if (!warehouseExists)
        {
            throw new AppException("Active warehouse not found.", System.Net.HttpStatusCode.NotFound);
        }

        var groupedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        var productIds = groupedItems.Select(x => x.ProductId).ToList();
        var products = await unitOfWork.Products.Query()
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new AppException("One or more products were not found or are inactive.");
        }

        var inventories = await unitOfWork.Inventories.Query()
            .Where(x => x.WarehouseId == request.WarehouseId && productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);




        // var orderType =   request.OrderSorce;
        // if( orderType = OrderSorce)
        // {
        //     var order = new Order
        // {
        //     UserId = userId,
        //     WarehouseId = request.WarehouseId,
        //     OrderDate = DateTime.UtcNow,
            
        //     Status = OrderStatus.Delivered
        //     // orderSorce = request.
        // };
        // }

        var order = new Order
        {
            UserId = userId,
            WarehouseId = request.WarehouseId,
            OrderDate = DateTime.UtcNow,
            OrderSource = request.OrderSource,
            Status = request.OrderSource == OrderSource.POS
            ? OrderStatus.Delivered
            : OrderStatus.Pending
            // orderSorce = request.
        };

        

        decimal totalAmount = 0;

        foreach (var item in groupedItems)
        {
            var product = products.First(x => x.Id == item.ProductId);
            var inventory = inventories.FirstOrDefault(x => x.ProductId == item.ProductId);
            var availableQuantity =
                (inventory?.Quantity ?? 0) -
                (inventory?.ReservedQuantity ?? 0);
            if (inventory == null || availableQuantity < item.Quantity)
            {
                throw new AppException($"Insufficient inventory for product '{product.Name}'.");
            }
            if (request.OrderSource == OrderSource.POS)
            {
                inventory.Quantity -= item.Quantity;
            }
            else
            {
                // Online → حجز فقط
                inventory.ReservedQuantity += item.Quantity;
            }

            
            unitOfWork.Inventories.Update(inventory);

            var totalPrice = product.Price * item.Quantity;
            totalAmount += totalPrice;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = totalPrice
            });
        }

        order.TotalAmount = totalAmount;

        await unitOfWork.Orders.AddAsync(order);

        if (order.OrderSource == OrderSource.POS)
        {
            await CreateSaleForOrderAsync(order, cancellationToken);
            
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (order.Sale != null && string.IsNullOrEmpty(order.Sale.InvoiceNumber))
        {
            order.Sale.InvoiceNumber =
                $"INV-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}";

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        if (order.OrderSource == OrderSource.POS &&
            order.Sale != null)
        {
            await slackService.SendAsync(
                SlackChannelType.Sales,
                "New Sale Completed",
                $"""
                Order Id: {order.Id}
                Invoice: {order.Sale.InvoiceNumber}
                Total: {order.TotalAmount}
                """
            );
        }

        logger.LogInformation("Created order {OrderId} for user {UserId}.", order.Id, userId);
        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto request, CancellationToken cancellationToken = default)
    {
        var order = await BuildOrderQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Order not found.", System.Net.HttpStatusCode.NotFound);

        if (order.Status == request.Status)
        {
            return mapper.Map<OrderDto>(order);
        }

        ValidateStatusTransition(order.Status, request.Status);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        if (request.Status == OrderStatus.Shipped)
        {
            await ShipOrderAsync(order, cancellationToken);
        }

        if (request.Status == OrderStatus.Cancelled)
        {
            await ReleaseReservedInventoryAsync(order, cancellationToken);
        }

        order.Status = request.Status;
        unitOfWork.Orders.Update(order);
        if (request.Status == OrderStatus.Delivered && order.Sale == null)
        {
            await CreateSaleForOrderAsync(order, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (order.Sale != null && string.IsNullOrEmpty(order.Sale.InvoiceNumber))
        {
            order.Sale.InvoiceNumber =
                $"INV-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}";

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        //if (request.Status == OrderStatus.Delivered && order.Sale == null)
        //{
          //  await CreateSaleForOrderAsync(order, cancellationToken);
        //}

        //await unitOfWork.SaveChangesAsync(cancellationToken);
        //await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Updated order {OrderId} status to {Status}.", id, request.Status);
        return await GetByIdAsync(id, cancellationToken);
        // return mapper.Map<OrderDto>(order);
    }

    private IQueryable<Order> BuildOrderQuery()
    {
        return unitOfWork.Orders.Query()
            .Include(x => x.User)
            .Include(x => x.Warehouse)
            .Include(x => x.Sale)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product);
    }

    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            throw new AppException("Delivered or cancelled orders cannot be updated.");
        }

        var valid = currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Approved or OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Approved => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => newStatus == OrderStatus.Delivered,
            _ => false
        };

        if (!valid)
        {
            throw new AppException($"Invalid order status transition from {currentStatus} to {newStatus}.");
        }
    }

    private async Task ShipOrderAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.OrderItems.Select(x => x.ProductId).ToList();
        var inventories = await unitOfWork.Inventories.Query()
            .Where(x => x.WarehouseId == order.WarehouseId && productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);

        foreach (var item in order.OrderItems)
        {
            var inventory = inventories.FirstOrDefault(x => x.ProductId == item.ProductId)
                ?? throw new AppException($"Inventory record not found for product id {item.ProductId}.", System.Net.HttpStatusCode.NotFound);

            if (inventory.ReservedQuantity < item.Quantity || inventory.Quantity < item.Quantity)
            {
                throw new AppException($"Inventory is inconsistent for product id {item.ProductId}. Cannot ship order.");
            }

            inventory.Quantity -= item.Quantity;
            inventory.ReservedQuantity -= item.Quantity;
            unitOfWork.Inventories.Update(inventory);
        }
    }

    private async Task ReleaseReservedInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.OrderItems.Select(x => x.ProductId).ToList();
        var inventories = await unitOfWork.Inventories.Query()
            .Where(x => x.WarehouseId == order.WarehouseId && productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);

        foreach (var item in order.OrderItems)
        {
            var inventory = inventories.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (inventory == null)
            {
                continue;
            }

            inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - item.Quantity);
            unitOfWork.Inventories.Update(inventory);
        }
    }

    private async Task CreateSaleForOrderAsync(Order order, CancellationToken cancellationToken)
    {
        var sale = new Sale
        {
            //OrderId = order.Id,
            // InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}",
            SaleDate = DateTime.UtcNow,
            GrandTotal = order.TotalAmount,
            Order = order
        };

        await unitOfWork.Sales.AddAsync(sale);
        order.Sale = sale;
    }
}
