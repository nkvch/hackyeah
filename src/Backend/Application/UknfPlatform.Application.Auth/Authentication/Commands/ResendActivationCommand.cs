using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to resend activation email to a user
/// </summary>
/// <param name="Email">User's email address</param>
public record ResendActivationCommand(string Email) : IRequest<ResendActivationResponse>;

