using System.Security.Cryptography;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Represents an email change confirmation token
/// </summary>
public class EmailChangeToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string NewEmail { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private EmailChangeToken() { } // Required for EF Core

    /// <summary>
    /// Creates a new email change token for a user
    /// </summary>
    /// <param name="userId">User ID to associate with this token</param>
    /// <param name="newEmail">New email address to be confirmed</param>
    /// <param name="expirationHours">Token expiration time in hours (default: 24)</param>
    /// <returns>New EmailChangeToken instance</returns>
    public static EmailChangeToken Create(Guid userId, string newEmail, int expirationHours = 24)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("New email is required", nameof(newEmail));

        return new EmailChangeToken
        {
            UserId = userId,
            NewEmail = newEmail.ToLowerInvariant(),
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

