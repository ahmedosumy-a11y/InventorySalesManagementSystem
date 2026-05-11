using InventorySalesManagementSystem.Application.DTOs.Users;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateStatusAsync(int id, UpdateUserStatusDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateRoleAsync(int id, UpdateUserRoleDto request, CancellationToken cancellationToken = default);
}
