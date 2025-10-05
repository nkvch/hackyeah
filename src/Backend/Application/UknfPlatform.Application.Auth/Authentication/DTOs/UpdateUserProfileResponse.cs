namespace UknfPlatform.Application.Auth.Authentication.DTOs;

/// <summary>
/// Response after updating user profile
/// </summary>
public record UpdateUserProfileResponse(
    bool Success,
    string Message,
    bool EmailChangeRequiresConfirmation,
    UserProfileDto UpdatedProfile
);

