namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for secure password hashing and verification
/// Implementation should use Argon2 or BCrypt algorithm
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using a secure algorithm with automatic salting
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Hashed password safe for storage</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against a stored hash
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);
}

