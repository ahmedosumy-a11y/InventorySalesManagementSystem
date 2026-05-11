using InventorySalesManagementSystem.Application.DTOs.Warehouses;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await warehouseService.GetAllAsync(cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.GetByIdAsync(id, cancellationToken);
        return Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<WarehouseDto>> Create(WarehouseCreateUpdateDto request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, WarehouseCreateUpdateDto request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.UpdateAsync(id, request, cancellationToken);
        return Ok(warehouse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await warehouseService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
