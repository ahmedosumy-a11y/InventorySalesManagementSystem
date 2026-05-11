using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Warehouses;

namespace InventorySalesManagementSystem.Application.Validators;

public class WarehouseCreateUpdateDtoValidator : AbstractValidator<WarehouseCreateUpdateDto>
{
    public WarehouseCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300);
    }
}
