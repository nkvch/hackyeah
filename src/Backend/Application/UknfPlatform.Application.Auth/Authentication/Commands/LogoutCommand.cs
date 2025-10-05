using MediatR;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to logout a user (revoke refresh token)
/// </summary>
public record LogoutCommand(
    string RefreshToken
) : IRequest<Unit>; // Unit represents void in MediatR

