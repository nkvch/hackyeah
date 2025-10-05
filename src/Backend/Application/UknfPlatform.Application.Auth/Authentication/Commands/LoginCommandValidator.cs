using FluentValidation;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Validator for LoginCommand
/// Performs basic input validation (email format, required fields)
/// Detailed authentication checks are performed in the handler
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

