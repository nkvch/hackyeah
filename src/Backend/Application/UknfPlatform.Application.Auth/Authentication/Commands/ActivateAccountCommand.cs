using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to activate a user account using an activation token
/// </summary>
/// <param name="Token">Activation token string</param>
public record ActivateAccountCommand(string Token) : IRequest<ActivateAccountResponse>;

