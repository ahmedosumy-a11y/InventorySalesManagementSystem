namespace InventorySalesManagementSystem.Application.DTOs.Inventory;

public class InventoryAdjustmentDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int QuantityChange { get; set; }
}
