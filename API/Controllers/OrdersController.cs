using System.Security.Claims;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Orders;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isPrivilegedUser = User.IsInRole("Admin") || User.IsInRole("Manager");
        var order = await orderService.GetByIdForUserAsync(id, userId, isPrivilegedUser, cancellationToken);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(OrderCreateDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var order = await orderService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var orders = await orderService.GetMyOrdersAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, OrderStatusUpdateDto request, CancellationToken cancellationToken)
    {
        var order = await orderService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(order);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new AppException("Authenticated user identifier is missing.", System.Net.HttpStatusCode.Unauthorized);
        }

        return userId;
    }
}
