using InventorySalesManagementSystem.Application.DTOs.Sales;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Manager")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var sale = await saleService.GetByIdAsync(id, cancellationToken);
        return Ok(sale);
    }

    [HttpPost("generate-invoice/{orderId:int}")]
    public async Task<ActionResult<SaleDto>> GenerateInvoice(int orderId, CancellationToken cancellationToken)
    {
        var sale = await saleService.GenerateInvoiceAsync(orderId, cancellationToken);
        return Ok(sale);
    }
}
