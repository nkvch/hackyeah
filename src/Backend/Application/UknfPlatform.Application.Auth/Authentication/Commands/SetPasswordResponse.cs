namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Response after successfully setting password
/// </summary>
public record SetPasswordResponse(
    Guid UserId,
    string Message,
    string RedirectUrl
);

