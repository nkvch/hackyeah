using System.Security.Cryptography;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Represents an account activation token for email verification
/// </summary>
public class ActivationToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private ActivationToken() { } // Required for EF Core

    /// <summary>
    /// Creates a new activation token for a user
    /// </summary>
    /// <param name="userId">User ID to associate with this token</param>
    /// <param name="expirationHours">Token expiration time in hours (default: 24)</param>
    /// <returns>New ActivationToken instance</returns>
    public static ActivationToken Create(Guid userId, int expirationHours = 24)
    {
        return new ActivationToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            IsUsed = false
        };
    }

    /// <summary>
    /// Checks if the token is valid (not expired and not used)
    /// </summary>
    /// <returns>True if token is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Marks the token as used
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the token has expired
    /// </summary>
    /// <returns>True if token is expired, false otherwise</returns>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Generates a cryptographically secure random token
    /// Uses 32 bytes of random data converted to Base64 URL-safe string
    /// </summary>
    /// <returns>Secure token string (approximately 43 characters)</returns>
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to Base64 and make URL-safe
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}

