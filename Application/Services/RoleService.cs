using AutoMapper;
using InventorySalesManagementSystem.Application.DTOs.Roles;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySalesManagementSystem.Application.Services;

public class RoleService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RoleService> logger) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await unitOfWork.Roles.Query()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} roles.", roles.Count);
        return mapper.Map<IReadOnlyList<RoleDto>>(roles);
    }
}
