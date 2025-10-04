namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Response after successful user registration
/// </summary>
public record RegisterUserResponse(
    Guid UserId,
    string Message
);

