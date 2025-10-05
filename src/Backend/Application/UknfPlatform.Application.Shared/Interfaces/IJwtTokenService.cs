using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// Token contains user claims (UserId, Email, UserType, Roles)
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <param name="roles">User roles to include as claims (optional)</param>
    /// <returns>JWT access token string</returns>
    string GenerateAccessToken(User user, IEnumerable<string>? roles = null);

    /// <summary>
    /// Generates a secure random refresh token
    /// Refresh token is stored in database and used to obtain new access tokens
    /// </summary>
    /// <returns>Base64-encoded random refresh token</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token's signature, expiration, and claims
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if token is valid, false otherwise</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Extracts the user ID from a valid JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID from token claims</returns>
    /// <exception cref="ArgumentException">Thrown if token is invalid</exception>
    Guid GetUserIdFromToken(string token);
}

