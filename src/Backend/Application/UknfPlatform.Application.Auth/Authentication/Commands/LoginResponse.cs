namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Response returned after successful login
/// Contains JWT tokens and user information
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn, // Seconds until access token expires
    UserInfoDto User,
    bool RequiresPasswordChange
);

