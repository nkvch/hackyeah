using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to refresh an access token using a refresh token
/// </summary>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<LoginResponse>;

