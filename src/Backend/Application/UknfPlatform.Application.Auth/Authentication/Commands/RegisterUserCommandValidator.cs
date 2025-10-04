using FluentValidation;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Validator for RegisterUserCommand
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+(?:[0-9] ?){6,14}[0-9]$")
            .WithMessage("Phone number must be in international format (e.g., +48123456789)");

        RuleFor(x => x.Pesel)
            .NotEmpty().WithMessage("PESEL is required")
            .Length(11).WithMessage("PESEL must be exactly 11 digits")
            .Matches(@"^\d{11}$").WithMessage("PESEL must contain only digits");
    }
}

