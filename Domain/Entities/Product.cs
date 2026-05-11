namespace InventorySalesManagementSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;

    public Category? Category { get; set; }
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
