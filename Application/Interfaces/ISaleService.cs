using InventorySalesManagementSystem.Application.DTOs.Sales;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface ISaleService
{
    Task<SaleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SaleDto> GenerateInvoiceAsync(int orderId, CancellationToken cancellationToken = default);
}
