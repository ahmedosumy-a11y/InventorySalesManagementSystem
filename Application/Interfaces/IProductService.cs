using InventorySalesManagementSystem.Application.DTOs.Products;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(ProductCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, ProductCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductInventoryDto>> GetInventoryAsync(int id, CancellationToken cancellationToken = default);
}
