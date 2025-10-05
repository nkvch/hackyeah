using UknfPlatform.Domain.Shared.Entities;

namespace UknfPlatform.Domain.Shared.Interfaces;

/// <summary>
/// Repository interface for Entity (supervised entities like banks, insurance companies)
/// Stub implementation for Epic 4 - full implementation in Epic 2
/// </summary>
public interface IEntityRepository
{
    /// <summary>
    /// Gets an entity by its ID (UKNF Code)
    /// </summary>
    Task<Entity?> GetByIdAsync(long entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    Task<bool> ExistsAsync(long entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities for a specific user (for entity selection dropdown)
    /// </summary>
    Task<IEnumerable<Entity>> GetUserEntitiesAsync(Guid userId, CancellationToken cancellationToken = default);
}

