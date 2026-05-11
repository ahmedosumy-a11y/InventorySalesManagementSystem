using InventorySalesManagementSystem.Application.DTOs.Reports;
using InventorySalesManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Manager")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("sales/daily")]
    public async Task<ActionResult<DailySalesReportDto>> GetDailySales([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var report = await reportService.GetDailySalesAsync(date, cancellationToken);
        return Ok(report);
    }

    [HttpGet("sales/monthly")]
    public async Task<ActionResult<MonthlySalesReportDto>> GetMonthlySales([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var report = await reportService.GetMonthlySalesAsync(year, month, cancellationToken);
        return Ok(report);
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<IReadOnlyList<TopProductReportDto>>> GetTopProducts([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var report = await reportService.GetTopProductsAsync(count, cancellationToken);
        return Ok(report);
    }
}
// متبقي ان اعمل تقارير شهريه ويومية لكل مخزن على حدة 
