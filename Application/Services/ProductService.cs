using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Products;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySalesManagementSystem.Application.Services;

public class ProductService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await unitOfWork.Products.Query()
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.Query()
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Product not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        await ValidateProductRequestAsync(request, null, cancellationToken);

        var product = new Product
        {
            Name = request.Name.Trim(),
            SKU = request.SKU.Trim(),
            Barcode = request.Barcode.Trim(),
            Price = request.Price,
            CostPrice = request.CostPrice,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive
        };

        await unitOfWork.Products.AddAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created product {ProductName}.", product.Name);
        
        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.Query()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AppException("Product not found.", System.Net.HttpStatusCode.NotFound);

        await ValidateProductRequestAsync(request, id, cancellationToken);

        product.Name = request.Name.Trim();
        product.SKU = request.SKU.Trim();
        product.Barcode = request.Barcode.Trim();
        product.Price = request.Price;
        product.CostPrice = request.CostPrice;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated product {ProductId}.", id);
        return mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(c => c.Id == id)
            ?? throw new AppException("Product not found.", System.Net.HttpStatusCode.NotFound);

        product.IsActive = false;
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deactivated product {ProductId}.", id);
    }

    public async Task<IReadOnlyList<ProductInventoryDto>> GetInventoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var productExists = await unitOfWork.Products.Query().AnyAsync(x => x.Id == id, cancellationToken);
        if (!productExists)
        {
            throw new AppException("Product not found.", System.Net.HttpStatusCode.NotFound);
        }

        var inventory = await unitOfWork.Inventories.Query()
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .Include(x => x.Warehouse)
            .OrderBy(x => x.Warehouse!.Name)
            .Select(x => new ProductInventoryDto
            {
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse != null ? x.Warehouse.Name : string.Empty,
                Quantity = x.Quantity,
                ReservedQuantity = x.ReservedQuantity,
                AvailableQuantity = x.Quantity - x.ReservedQuantity
            })
            .ToListAsync(cancellationToken);

        return inventory;
    }

    private async Task ValidateProductRequestAsync(ProductCreateUpdateDto request, int? currentId, CancellationToken cancellationToken)
    {
        var categoryExists = await unitOfWork.Categories.Query().AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new AppException("Category not found.", System.Net.HttpStatusCode.NotFound);
        }

        var sku = request.SKU.Trim();
        var barcode = request.Barcode.Trim();

        var duplicateSkuExists = await unitOfWork.Products.Query()
            .AnyAsync(x => x.SKU == sku && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
        if (duplicateSkuExists)
        {
            throw new AppException("A product with this SKU already exists.", System.Net.HttpStatusCode.Conflict);
        }

        var duplicateBarcodeExists = await unitOfWork.Products.Query()
            .AnyAsync(x => x.Barcode == barcode && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
        if (duplicateBarcodeExists)
        {
            throw new AppException("A product with this barcode already exists.", System.Net.HttpStatusCode.Conflict);
        }
    }
}
