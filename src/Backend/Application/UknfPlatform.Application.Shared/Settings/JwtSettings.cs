namespace UknfPlatform.Application.Shared.Settings;

/// <summary>
/// JWT token configuration settings
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key for signing JWT tokens (minimum 256 bits / 32 characters)
    /// IMPORTANT: Must be securely generated and stored (environment variables, Key Vault)
    /// NEVER commit to source control
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (application name)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (intended recipients)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in minutes (recommended: 15-60 minutes)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration time in days (recommended: 7-30 days)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

