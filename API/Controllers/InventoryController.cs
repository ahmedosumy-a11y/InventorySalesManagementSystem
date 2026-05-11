using InventorySalesManagementSystem.Application.DTOs.Inventory;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("warehouse/{id:int}")]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetByWarehouse(int id, CancellationToken cancellationToken)
    {
        var inventory = await inventoryService.GetByWarehouseAsync(id, cancellationToken);
        return Ok(inventory);
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Adjust(InventoryAdjustmentDto request, CancellationToken cancellationToken)
    {
        await inventoryService.AdjustAsync(request, cancellationToken);
        return Ok(new { message = "Inventory adjusted successfully." });
    }

    [HttpPost("transfer")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> Transfer(InventoryTransferDto request, CancellationToken cancellationToken)
    {
        await inventoryService.TransferAsync(request, cancellationToken);
        return Ok(new { message = "Inventory transferred successfully." });
    }
}
