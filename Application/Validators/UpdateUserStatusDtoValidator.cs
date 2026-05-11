using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Users;

namespace InventorySalesManagementSystem.Application.Validators;

public class UpdateUserStatusDtoValidator : AbstractValidator<UpdateUserStatusDto>
{
    public UpdateUserStatusDtoValidator()
    {
    }
}
