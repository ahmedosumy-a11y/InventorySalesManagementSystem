using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Users;

namespace InventorySalesManagementSystem.Application.Validators;

public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleDtoValidator()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0);
    }
}
