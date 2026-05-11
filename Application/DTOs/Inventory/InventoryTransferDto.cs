namespace InventorySalesManagementSystem.Application.DTOs.Inventory;

public class InventoryTransferDto
{
    public int ProductId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int Quantity { get; set; }
}
