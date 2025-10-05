using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to confirm email change using token from confirmation email
/// </summary>
public record ConfirmEmailChangeCommand(string Token) : IRequest<ConfirmEmailChangeResponse>;

/// <summary>
/// Response after confirming email change
/// </summary>
public record ConfirmEmailChangeResponse(
    string Message,
    string NewEmail
);

