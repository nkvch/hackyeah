namespace UknfPlatform.Application.Shared.Settings;

/// <summary>
/// Configuration settings for password policy
/// Maps to appsettings.json PasswordPolicy section
/// </summary>
public class PasswordPolicySettings
{
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int MinUniqueChars { get; set; } = 5;
    
    /// <summary>
    /// Number of previous passwords to remember (prevents reuse)
    /// Set to 0 to disable password history
    /// </summary>
    public int PasswordHistoryCount { get; set; } = 5;
    
    /// <summary>
    /// Number of days until password expires and must be changed
    /// Set to 0 to disable password expiration
    /// </summary>
    public int PasswordExpirationDays { get; set; } = 90;
    
    /// <summary>
    /// List of commonly used passwords that should be prohibited
    /// </summary>
    public List<string> ProhibitedPasswords { get; set; } = new()
    {
        "password",
        "123456",
        "qwerty",
        "admin",
        "welcome"
    };
}

