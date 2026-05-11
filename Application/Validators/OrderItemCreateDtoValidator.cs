using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Orders;

namespace InventorySalesManagementSystem.Application.Validators;

public class OrderItemCreateDtoValidator : AbstractValidator<OrderItemCreateDto>
{
    public OrderItemCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
