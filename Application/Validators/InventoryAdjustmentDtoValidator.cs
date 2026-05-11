using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Inventory;

namespace InventorySalesManagementSystem.Application.Validators;

public class InventoryAdjustmentDtoValidator : AbstractValidator<InventoryAdjustmentDto>
{
    public InventoryAdjustmentDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.QuantityChange)
            .NotEqual(0);
    }
}
