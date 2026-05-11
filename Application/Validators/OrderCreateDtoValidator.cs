using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Orders;

namespace InventorySalesManagementSystem.Application.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemCreateDtoValidator());
    }
}
