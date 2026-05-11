namespace InventorySalesManagementSystem.Application.DTOs.Warehouses;

public class WarehouseCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
