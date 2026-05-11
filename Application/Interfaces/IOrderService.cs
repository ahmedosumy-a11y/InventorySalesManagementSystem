using InventorySalesManagementSystem.Application.DTOs.Orders;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdForUserAsync(int id, int userId, bool isPrivilegedUser, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(int userId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(int userId, OrderCreateDto request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto request, CancellationToken cancellationToken = default);
}
