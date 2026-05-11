using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Warehouses;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySalesManagementSystem.Application.Services;

public class WarehouseService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<WarehouseService> logger) : IWarehouseService
{
    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await unitOfWork.Warehouses.Query()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await unitOfWork.Warehouses.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Warehouse not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(WarehouseCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        if (await unitOfWork.Warehouses.Query().AnyAsync(x => x.Name == normalizedName, cancellationToken))
        {
            throw new AppException("A warehouse with this name already exists.", System.Net.HttpStatusCode.Conflict);
        }

        var warehouse = new Warehouse
        {
            Name = normalizedName,
            Address = request.Address.Trim(),
            IsActive = request.IsActive
        };

        await unitOfWork.Warehouses.AddAsync(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created warehouse {WarehouseName}.", warehouse.Name);
        return mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, WarehouseCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        var warehouse = await unitOfWork.Warehouses.Query()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Warehouse not found.", System.Net.HttpStatusCode.NotFound);

        var normalizedName = request.Name.Trim();
        if (await unitOfWork.Warehouses.Query().AnyAsync(x => x.Id != id && x.Name == normalizedName, cancellationToken))
        {
            throw new AppException("A warehouse with this name already exists.", System.Net.HttpStatusCode.Conflict);
        }

        warehouse.Name = normalizedName;
        warehouse.Address = request.Address.Trim();
        warehouse.IsActive = request.IsActive;

        unitOfWork.Warehouses.Update(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated warehouse {WarehouseId}.", id);
        return mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await unitOfWork.Warehouses.GetByIdAsync(x => x.Id == id)
            ?? throw new AppException("Warehouse not found.", System.Net.HttpStatusCode.NotFound);

        warehouse.IsActive = false;
        unitOfWork.Warehouses.Update(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deactivated warehouse {WarehouseId}.", id);
    }
}
