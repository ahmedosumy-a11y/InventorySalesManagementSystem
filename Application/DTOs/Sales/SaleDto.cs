namespace InventorySalesManagementSystem.Application.DTOs.Sales;

public class SaleDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderSorce { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal GrandTotal { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
}
