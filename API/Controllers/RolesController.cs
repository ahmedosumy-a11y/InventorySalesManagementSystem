using InventorySalesManagementSystem.Application.DTOs.Roles;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin,Manager")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }
}
