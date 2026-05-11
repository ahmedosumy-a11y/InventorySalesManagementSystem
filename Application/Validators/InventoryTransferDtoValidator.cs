using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Inventory;

namespace InventorySalesManagementSystem.Application.Validators;

public class InventoryTransferDtoValidator : AbstractValidator<InventoryTransferDto>
{
    public InventoryTransferDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.FromWarehouseId).GreaterThan(0);
        RuleFor(x => x.ToWarehouseId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ToWarehouseId)
            .NotEqual(x => x.FromWarehouseId)
            .WithMessage("Source and destination warehouse must be different.");
    }
}
