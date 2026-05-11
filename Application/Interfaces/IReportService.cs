using InventorySalesManagementSystem.Application.DTOs.Reports;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IReportService
{
    Task<DailySalesReportDto> GetDailySalesAsync(DateTime? date, CancellationToken cancellationToken = default);
    Task<MonthlySalesReportDto> GetMonthlySalesAsync(int? year, int? month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default);
}
