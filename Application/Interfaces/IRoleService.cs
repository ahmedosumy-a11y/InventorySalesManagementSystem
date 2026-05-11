using InventorySalesManagementSystem.Application.DTOs.Roles;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
