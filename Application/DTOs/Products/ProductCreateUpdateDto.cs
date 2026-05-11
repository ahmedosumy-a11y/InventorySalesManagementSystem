namespace InventorySalesManagementSystem.Application.DTOs.Products;

public class ProductCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}
