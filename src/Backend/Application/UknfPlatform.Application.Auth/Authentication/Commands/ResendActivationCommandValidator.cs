using FluentValidation;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Validator for ResendActivationCommand
/// </summary>
public class ResendActivationCommandValidator : AbstractValidator<ResendActivationCommand>
{
    public ResendActivationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");
    }
}

