using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Domain.Auth.Interfaces;

/// <summary>
/// Repository interface for PasswordHistory entity
/// </summary>
public interface IPasswordHistoryRepository
{
    /// <summary>
    /// Gets the most recent N password hashes for a user
    /// Ordered by CreatedDate descending (newest first)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="count">Number of passwords to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of password histories</returns>
    Task<List<PasswordHistory>> GetRecentPasswordsAsync(Guid userId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new password history entry
    /// </summary>
    /// <param name="passwordHistory">Password history to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(PasswordHistory passwordHistory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

