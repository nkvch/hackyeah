using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Refresh token entity for long-lived authentication
/// Used to obtain new access tokens without re-authentication
/// Stored in database for revocation support
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// User ID this refresh token belongs to
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Unique refresh token string (base64-encoded random bytes)
    /// </summary>
    public string Token { get; private set; }

    /// <summary>
    /// Token expiration date (typically 7-30 days from creation)
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Whether this token has been revoked (logout, security incident)
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// Date when token was revoked (nullable)
    /// </summary>
    public DateTime? RevokedDate { get; private set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private RefreshToken()
    {
        Token = string.Empty;
    }

    /// <summary>
    /// Factory method to create a new refresh token
    /// </summary>
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            RevokedDate = null,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revokes this refresh token (logout or security incident)
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked)
            return; // Already revoked, idempotent

        IsRevoked = true;
        RevokedDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Checks if this refresh token is valid (not expired and not revoked)
    /// </summary>
    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }
}

