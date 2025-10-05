namespace UknfPlatform.Application.Auth.Authentication.Queries;

/// <summary>
/// Response containing password policy configuration
/// Used by frontend to display requirements and validate passwords
/// </summary>
public record PasswordPolicyResponse(
    int MinLength,
    int MaxLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialChar,
    int MinUniqueChars
);

