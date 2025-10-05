using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Domain.Auth.Interfaces;

/// <summary>
/// Repository interface for EmailChangeToken entity operations
/// </summary>
public interface IEmailChangeTokenRepository
{
    /// <summary>
    /// Gets an email change token by its token string
    /// </summary>
    Task<EmailChangeToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all valid (unused and not expired) email change tokens for a user
    /// </summary>
    Task<IEnumerable<EmailChangeToken>> GetValidTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new email change token to the repository
    /// </summary>
    Task AddAsync(EmailChangeToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing email change token
    /// </summary>
    Task UpdateAsync(EmailChangeToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all existing tokens for a user (marks as used)
    /// </summary>
    Task InvalidateAllTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

