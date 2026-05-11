using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Application.DTOs.Orders;

public class OrderCreateDto
{
    public int WarehouseId { get; set; }
    public OrderSource OrderSource { get; set; }
    public ICollection<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
}
