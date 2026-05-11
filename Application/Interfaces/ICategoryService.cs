using InventorySalesManagementSystem.Application.DTOs.Categories;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryCreateUpdateDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
