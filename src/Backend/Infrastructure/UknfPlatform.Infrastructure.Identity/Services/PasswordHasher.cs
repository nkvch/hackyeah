using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Infrastructure.Identity.Services;

/// <summary>
/// Password hashing service using BCrypt algorithm
/// BCrypt automatically handles salting and provides secure password storage
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private const int WorkFactor = 12; // BCrypt work factor (2^12 = 4096 iterations)

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Hashes a password using BCrypt with work factor 12
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        try
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
            _logger.LogDebug("Password hashed successfully");
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    /// <summary>
    /// Verifies a password against a BCrypt hash
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            _logger.LogDebug("Password verification completed: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }
}

