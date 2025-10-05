using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Domain.Auth.Interfaces;

/// <summary>
/// Repository interface for AuthenticationAuditLog entity operations
/// </summary>
public interface IAuthenticationAuditLogRepository
{
    /// <summary>
    /// Adds a new authentication audit log entry
    /// </summary>
    Task AddAsync(AuthenticationAuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent authentication attempts for an email address (for rate limiting analysis)
    /// </summary>
    Task<IEnumerable<AuthenticationAuditLog>> GetRecentAttemptsByEmailAsync(
        string email,
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent authentication attempts from an IP address (for rate limiting analysis)
    /// </summary>
    Task<IEnumerable<AuthenticationAuditLog>> GetRecentAttemptsByIpAsync(
        string ipAddress,
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets authentication history for a user (for security monitoring)
    /// </summary>
    Task<IEnumerable<AuthenticationAuditLog>> GetUserAuthenticationHistoryAsync(
        Guid userId,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

