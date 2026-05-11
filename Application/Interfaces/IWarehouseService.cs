using InventorySalesManagementSystem.Application.DTOs.Warehouses;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(WarehouseCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task<WarehouseDto> UpdateAsync(int id, WarehouseCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
