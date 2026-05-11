using AutoMapper;
using InventorySalesManagementSystem.Application.DTOs.Reports;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesManagementSystem.Application.Services;

public class ReportService(
    IUnitOfWork unitOfWork,

    ILogger<ReportService> logger) : IReportService
{
    public async Task<DailySalesReportDto> GetDailySalesAsync(DateTime? date, CancellationToken cancellationToken = default)
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;
        var nextDate = targetDate.AddDays(1);

        var sales = unitOfWork.Sales.Query()
            .AsNoTracking()
            .Where(x => x.SaleDate >= targetDate && x.SaleDate < nextDate);

        var totalSales = await sales.CountAsync(cancellationToken);
        var totalRevenue = totalSales == 0 ? 0 : await sales.SumAsync(x => x.GrandTotal, cancellationToken);

        logger.LogInformation("Generated daily sales report for {Date}.", targetDate);


        return new DailySalesReportDto
        {
            Date = targetDate,
            TotalSales = totalSales,
            TotalOrders = totalSales,
            TotalRevenue = totalRevenue
        };
    }

    public async Task<MonthlySalesReportDto> GetMonthlySalesAsync(int? year, int? month, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        var startDate = new DateTime(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1);

        var sales = unitOfWork.Sales.Query()
            .AsNoTracking()
            .Where(x => x.SaleDate >= startDate && x.SaleDate < endDate);

        var totalSales = await sales.CountAsync(cancellationToken);
        var totalRevenue = totalSales == 0 ? 0 : await sales.SumAsync(x => x.GrandTotal, cancellationToken);

        logger.LogInformation("Generated monthly sales report for {Year}-{Month}.", targetYear, targetMonth);

        return new MonthlySalesReportDto
        {
            Year = targetYear,
            Month = targetMonth,
            TotalSales = totalSales,
            TotalOrders = totalSales,
            TotalRevenue = totalRevenue
        };
    }

    public async Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default)
    {
        var safeCount = count <= 0 ? 10 : count;

        var result = await unitOfWork.OrderItems.Query()
            .AsNoTracking()
            .Where(x => x.Order != null && x.Order.Status == OrderStatus.Delivered && x.Product != null)
            .GroupBy(x => new
            {
                x.ProductId,
                ProductName = x.Product!.Name,
                x.Product.SKU
                
            })
            .Select(group => new TopProductReportDto
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.ProductName,
                SKU = group.Key.SKU,
                TotalQuantitySold = group.Sum(x => x.Quantity),
                TotalRevenue = group.Sum(x => x.TotalPrice)
                
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(safeCount)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Generated top products report with count {Count}.", safeCount);
        return result;
    }
    
}
