using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Categories;

namespace InventorySalesManagementSystem.Application.Validators;

public class CategoryCreateUpdateDtoValidator : AbstractValidator<CategoryCreateUpdateDto>
{
    public CategoryCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
