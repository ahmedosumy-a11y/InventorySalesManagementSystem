namespace InventorySalesManagementSystem.Domain.Entities;

public class Inventory
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
