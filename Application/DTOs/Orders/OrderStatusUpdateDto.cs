using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Application.DTOs.Orders;

public class OrderStatusUpdateDto
{
    public OrderStatus Status { get; set; }
}
