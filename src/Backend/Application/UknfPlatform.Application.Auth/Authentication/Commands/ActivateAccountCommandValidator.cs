using FluentValidation;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Validator for ActivateAccountCommand
/// </summary>
public class ActivateAccountCommandValidator : AbstractValidator<ActivateAccountCommand>
{
    public ActivateAccountCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Activation token is required")
            .MinimumLength(32).WithMessage("Invalid activation token format");
    }
}

