using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public OrderSource OrderSorce { get; set; }
    public decimal TotalAmount { get; set; }
    public string? InvoiceNumber { get; set; }
    public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}
