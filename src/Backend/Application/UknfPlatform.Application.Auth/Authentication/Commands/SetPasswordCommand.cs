using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to set initial password for newly activated user
/// Token must be a valid activation token from Story 1.2
/// </summary>
public record SetPasswordCommand(
    string Token,
    string Password,
    string PasswordConfirmation
) : IRequest<SetPasswordResponse>;

