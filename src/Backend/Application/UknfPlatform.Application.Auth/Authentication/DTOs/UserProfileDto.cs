namespace UknfPlatform.Application.Auth.Authentication.DTOs;

/// <summary>
/// User profile information DTO
/// Contains public profile data (excludes sensitive information like full PESEL and password)
/// </summary>
public record UserProfileDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PendingEmail, // New email awaiting confirmation
    string PhoneNumber,
    string PeselLast4, // Masked - last 4 digits only
    string UserType,
    DateTime CreatedDate,
    DateTime? LastLoginDate
);

