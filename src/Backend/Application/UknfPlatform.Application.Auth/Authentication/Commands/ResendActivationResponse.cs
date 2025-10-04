namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Response for resend activation request
/// </summary>
/// <param name="Message">Generic success message (doesn't reveal if user exists)</param>
public record ResendActivationResponse(string Message);

