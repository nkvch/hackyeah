using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to authenticate a user with email and password
/// </summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;

