namespace InventorySalesManagementSystem.Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
