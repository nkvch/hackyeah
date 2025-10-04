namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Response for account activation
/// </summary>
/// <param name="UserId">ID of the activated user</param>
/// <param name="Message">Success message</param>
public record ActivateAccountResponse(Guid UserId, string Message);

