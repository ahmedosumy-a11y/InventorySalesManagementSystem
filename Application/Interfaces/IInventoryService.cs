using InventorySalesManagementSystem.Application.DTOs.Inventory;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDto>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);
    Task AdjustAsync(InventoryAdjustmentDto request, CancellationToken cancellationToken = default);
    Task TransferAsync(InventoryTransferDto request, CancellationToken cancellationToken = default);
}
