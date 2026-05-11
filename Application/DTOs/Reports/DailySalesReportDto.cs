namespace InventorySalesManagementSystem.Application.DTOs.Reports;

public class DailySalesReportDto
{
    public DateTime Date { get; set; }
    public int TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
