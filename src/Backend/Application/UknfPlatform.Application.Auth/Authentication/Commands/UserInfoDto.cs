namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// User information DTO returned after successful authentication
/// </summary>
public record UserInfoDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string UserType
);

