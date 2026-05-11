using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Products;

namespace InventorySalesManagementSystem.Application.Validators;

public class ProductCreateUpdateDtoValidator : AbstractValidator<ProductCreateUpdateDto>
{
    public ProductCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SKU)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Barcode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
    }
}
