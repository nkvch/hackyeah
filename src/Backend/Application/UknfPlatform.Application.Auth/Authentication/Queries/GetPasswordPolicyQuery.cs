using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Queries;

/// <summary>
/// Query to retrieve current password policy configuration
/// Used by frontend to display requirements and validate client-side
/// </summary>
public record GetPasswordPolicyQuery : IRequest<PasswordPolicyResponse>;

