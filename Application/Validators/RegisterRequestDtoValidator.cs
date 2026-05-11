using FluentValidation;
using InventorySalesManagementSystem.Application.DTOs.Auth;

namespace InventorySalesManagementSystem.Application.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$")
            .WithMessage("Password must contain uppercase, lowercase, and numeric characters.");
    }
}
