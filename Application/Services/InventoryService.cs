using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Inventory;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesManagementSystem.Application.Services;

public class InventoryService(
    IUnitOfWork unitOfWork,
    ILogger<InventoryService> logger) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
    {
        var warehouseExists = await unitOfWork.Warehouses.Query().AnyAsync(x => x.Id == warehouseId, cancellationToken);
        if (!warehouseExists)
        {
            throw new AppException("Warehouse not found.", System.Net.HttpStatusCode.NotFound);
        }

        return await unitOfWork.Inventories.Query()
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            // .Include(x => x.Product)
            // .OrderBy(x => x.Product!.Name)
            .Select(x => new InventoryItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.Product != null ? x.Product.Name : string.Empty,
                SKU = x.Product != null ? x.Product.SKU : string.Empty,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.Quantity - x.ReservedQuantity
            })
            .OrderBy(x => x.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task AdjustAsync(InventoryAdjustmentDto request, CancellationToken cancellationToken = default)
    {
        await EnsureActiveProductAndWarehouseAsync(request.ProductId, request.WarehouseId, cancellationToken);

        // var inventory = await unitOfWork.Inventories.GetByIdAsync(request.ProductId, request.WarehouseId);
        var inventory = await unitOfWork.Inventories.GetByIdAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId);

        if (inventory == null)
        {
            if (request.QuantityChange < 0)
            {
                throw new AppException("Inventory record does not exist for this product and warehouse.");
            }

            inventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = request.QuantityChange,
                ReservedQuantity = 0
            };

            await unitOfWork.Inventories.AddAsync(inventory);
        }
        else
        {
            var newQuantity = inventory.Quantity + request.QuantityChange;
            if (newQuantity < 0)
            {
                throw new AppException("Inventory quantity cannot be negative.");
            }

            if (newQuantity < inventory.ReservedQuantity)
            {
                throw new AppException("Inventory quantity cannot be less than reserved quantity.");
            }

            inventory.Quantity = newQuantity;
            unitOfWork.Inventories.Update(inventory);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Adjusted inventory for product {ProductId} in warehouse {WarehouseId} by {QuantityChange}.",
            request.ProductId,
            request.WarehouseId,
            request.QuantityChange);
    }

    public async Task TransferAsync(InventoryTransferDto request, CancellationToken cancellationToken = default)
    {
        await EnsureActiveProductAndWarehouseAsync(request.ProductId, request.FromWarehouseId, cancellationToken);
        await EnsureActiveProductAndWarehouseAsync(request.ProductId, request.ToWarehouseId, cancellationToken);

        // var sourceInventory = await unitOfWork.Inventories.GetByIdAsync(request.ProductId, request.FromWarehouseId)
        //     ?? throw new AppException("Source inventory record not found.", System.Net.HttpStatusCode.NotFound);
        var sourceInventory = await unitOfWork.Inventories.GetByIdAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.FromWarehouseId)
            ?? throw new AppException("Source inventory record not found.", System.Net.HttpStatusCode.NotFound);
        var availableQuantity = sourceInventory.Quantity - sourceInventory.ReservedQuantity;
        if (availableQuantity < request.Quantity)
        {
            throw new AppException("Insufficient available inventory to transfer.");
        }

        // var destinationInventory = await unitOfWork.Inventories.GetByIdAsync(request.ProductId, request.ToWarehouseId);
        var destinationInventory = await unitOfWork.Inventories.GetByIdAsTrackingAsync(i=>i.ProductId == request.ProductId && i.WarehouseId == request.ToWarehouseId);
        if (destinationInventory == null)
        {
            destinationInventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.ToWarehouseId,
                Quantity = 0,
                ReservedQuantity = 0
            };

            await unitOfWork.Inventories.AddAsync(destinationInventory);
        }
        // else
        // {
        //     destinationInventory.Quantity += request.Quantity;
        // }
                

        sourceInventory.Quantity -= request.Quantity;
        destinationInventory.Quantity += request.Quantity;
        
        unitOfWork.Inventories.Update(sourceInventory);
        // unitOfWork.Inventories.Update(destinationInventory);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Transferred {Quantity} units of product {ProductId} from warehouse {FromWarehouseId} to warehouse {ToWarehouseId}.",
            request.Quantity,
            request.ProductId,
            request.FromWarehouseId,
            request.ToWarehouseId);
    }

    private async Task EnsureActiveProductAndWarehouseAsync(int productId, int warehouseId, CancellationToken cancellationToken)
    {
        var productExists = await unitOfWork.Products.Query().AnyAsync(x => x.Id == productId && x.IsActive, cancellationToken);
        if (!productExists)
        {
            throw new AppException("Active product not found.", System.Net.HttpStatusCode.NotFound);
        }

        var warehouseExists = await unitOfWork.Warehouses.Query().AnyAsync(x => x.Id == warehouseId && x.IsActive, cancellationToken);
        if (!warehouseExists)
        {
            throw new AppException("Active warehouse not found.", System.Net.HttpStatusCode.NotFound);
        }
    }
}
