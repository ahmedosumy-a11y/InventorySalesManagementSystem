using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    // Knowlodage for know the order type
    public OrderSource OrderSource { get; set; }
    public decimal TotalAmount { get; set; }

    public User? User { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Sale? Sale { get; set; }
}
