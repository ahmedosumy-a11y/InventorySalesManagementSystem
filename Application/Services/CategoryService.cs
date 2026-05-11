using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Categories;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventorySalesManagementSystem.Application.Services;

public class CategoryService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await unitOfWork.Categories.GetAllAsync();

        return mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // var category = await unitOfWork.Categories.Query()
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        //     ?? throw new AppException("Category not found.", System.Net.HttpStatusCode.NotFound);
        var category = await unitOfWork.Categories.GetByIdAsync(i => i.Id == id)?? throw new AppException("Category not found.", System.Net.HttpStatusCode.NotFound);

        return mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        var normalizedName = request.Name.Trim();
        if (await unitOfWork.Categories.AnyAsync(x => x.Name == normalizedName))
        {
            throw new AppException("A category with this name already exists.", System.Net.HttpStatusCode.Conflict);
        }

        var category = new Category
        {
            Name = normalizedName,
            Description = request.Description?.Trim() ?? string.Empty
            // Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        await unitOfWork.Categories.AddAsync(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created category {CategoryName}.", category.Name);
        return mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryCreateUpdateDto request, CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.Categories
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new AppException("Category not found.", System.Net.HttpStatusCode.NotFound);

        var normalizedName = request.Name.Trim();
        if (await unitOfWork.Categories.Query().AnyAsync(x => x.Id != id && x.Name == normalizedName, cancellationToken))
        {
            throw new AppException("A category with this name already exists.", System.Net.HttpStatusCode.Conflict);
        }

        category.Name = normalizedName;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        unitOfWork.Categories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated category {CategoryId}.", id);
        return mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(c => c.Id == id)
            ?? throw new AppException("Category not found.", System.Net.HttpStatusCode.NotFound);

        if (await unitOfWork.Products.AnyAsync(x => x.CategoryId == id))
        {
            throw new AppException("Category cannot be deleted because it is linked to products.", System.Net.HttpStatusCode.Conflict);
        }

        unitOfWork.Categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted category {CategoryId}.", id);
    }
}
