using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Users;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySalesManagementSystem.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UserService> logger) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(x => x.Role)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} users.", users.Count);
        return mapper.Map<IReadOnlyList<UserDto>>(users);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("User not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateStatusAsync(int id, UpdateUserStatusDto request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("User not found.", System.Net.HttpStatusCode.NotFound);

        user.IsActive = request.IsActive;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated status for user {UserId} to {IsActive}.", id, request.IsActive);
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateRoleAsync(int id, UpdateUserRoleDto request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("User not found.", System.Net.HttpStatusCode.NotFound);

        var role = await unitOfWork.Roles.GetByIdAsync(r => r.Id == request.RoleId)
            ?? throw new AppException("Role not found.", System.Net.HttpStatusCode.NotFound);

        user.RoleId = role.Id;
        user.Role = role;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated role for user {UserId} to role {RoleId}.", id, request.RoleId);
        return mapper.Map<UserDto>(user);
    }
}
