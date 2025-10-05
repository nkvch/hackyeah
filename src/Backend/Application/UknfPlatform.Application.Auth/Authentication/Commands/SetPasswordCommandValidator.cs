using FluentValidation;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Shared.ValueObjects;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Validator for SetPasswordCommand
/// Enforces password policy requirements
/// </summary>
public class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    private readonly PasswordPolicySettings _policySettings;

    public SetPasswordCommandValidator(IOptions<PasswordPolicySettings> policySettings)
    {
        _policySettings = policySettings?.Value ?? throw new ArgumentNullException(nameof(policySettings));

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Activation token is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Custom(ValidatePasswordPolicy);

        RuleFor(x => x.PasswordConfirmation)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }

    private void ValidatePasswordPolicy(string password, ValidationContext<SetPasswordCommand> context)
    {
        if (string.IsNullOrEmpty(password))
            return; // Already validated by NotEmpty

        // Create PasswordPolicy from settings
        var policy = new PasswordPolicy(
            _policySettings.MinLength,
            _policySettings.MaxLength,
            _policySettings.RequireUppercase,
            _policySettings.RequireLowercase,
            _policySettings.RequireDigit,
            _policySettings.RequireSpecialChar,
            _policySettings.MinUniqueChars);

        // Validate password against policy
        var result = policy.Validate(password);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                context.AddFailure("Password", error);
            }
        }

        // Check against prohibited passwords
        if (_policySettings.ProhibitedPasswords.Any(prohibited =>
            password.Contains(prohibited, StringComparison.OrdinalIgnoreCase)))
        {
            context.AddFailure("Password", "Password contains commonly used words and is not secure");
        }
    }
}

