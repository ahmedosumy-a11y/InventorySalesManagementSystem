using InventorySalesManagementSystem.Application.DTOs.Users;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<UserDto>> UpdateStatus(int id, UpdateUserStatusDto request, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<UserDto>> UpdateRole(int id, UpdateUserRoleDto request, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateRoleAsync(id, request, cancellationToken);
        return Ok(user);
    }
}
