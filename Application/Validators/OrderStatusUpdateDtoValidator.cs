using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Orders;

namespace InventorySalesManagementSystem.Application.Validators;

public class OrderStatusUpdateDtoValidator : AbstractValidator<OrderStatusUpdateDto>
{
    public OrderStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
