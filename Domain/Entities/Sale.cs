namespace InventorySalesManagementSystem.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public  string OrderSorce { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public decimal GrandTotal { get; set; }

    public Order? Order { get; set; }
}
