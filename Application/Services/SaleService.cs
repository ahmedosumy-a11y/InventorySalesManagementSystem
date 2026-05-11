using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Sales;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using InventorySalesManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySalesManagementSystem.Application.Services;

public class SaleService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<SaleService> logger) : ISaleService
{
    public async Task<SaleDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sale = await unitOfWork.Sales.Query()
            .AsNoTracking()
            .Include(x => x.Order)
                .ThenInclude(x => x!.User)
            .Include(x => x.Order)
                .ThenInclude(x => x!.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Sale not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<SaleDto>(sale);
    }

    public async Task<SaleDto> GenerateInvoiceAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.Orders.Query()
            .Include(x => x.User)
            .Include(x => x.Warehouse)
            .Include(x => x.Sale)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new AppException("Order not found.", System.Net.HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Delivered)
        {
            throw new AppException("Invoice can only be generated for delivered orders.");
        }

        if (order.Sale != null)
        {
            return await GetByIdAsync(order.Sale.Id, cancellationToken);
        }

        var sale = new Sale
        {
            OrderId = order.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}",
            SaleDate = DateTime.UtcNow,
            GrandTotal = order.TotalAmount,
            Order = order
        };

        await unitOfWork.Sales.AddAsync(sale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Generated invoice for order {OrderId}.", orderId);
        return await GetByIdAsync(sale.Id, cancellationToken);
    }
}
