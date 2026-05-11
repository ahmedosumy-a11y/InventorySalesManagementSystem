namespace InventorySalesManagementSystem.Application.DTOs.Reports;

public class MonthlySalesReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
