using MediatR;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Settings;

namespace UknfPlatform.Application.Auth.Authentication.Queries;

/// <summary>
/// Handler for GetPasswordPolicyQuery
/// Returns current password policy configuration for frontend use
/// </summary>
public class GetPasswordPolicyQueryHandler : IRequestHandler<GetPasswordPolicyQuery, PasswordPolicyResponse>
{
    private readonly PasswordPolicySettings _passwordPolicy;

    public GetPasswordPolicyQueryHandler(IOptions<PasswordPolicySettings> passwordPolicy)
    {
        _passwordPolicy = passwordPolicy?.Value ?? throw new ArgumentNullException(nameof(passwordPolicy));
    }

    public Task<PasswordPolicyResponse> Handle(GetPasswordPolicyQuery request, CancellationToken cancellationToken)
    {
        var response = new PasswordPolicyResponse(
            MinLength: _passwordPolicy.MinLength,
            MaxLength: _passwordPolicy.MaxLength,
            RequireUppercase: _passwordPolicy.RequireUppercase,
            RequireLowercase: _passwordPolicy.RequireLowercase,
            RequireDigit: _passwordPolicy.RequireDigit,
            RequireSpecialChar: _passwordPolicy.RequireSpecialChar,
            MinUniqueChars: _passwordPolicy.MinUniqueChars
        );

        return Task.FromResult(response);
    }
}

