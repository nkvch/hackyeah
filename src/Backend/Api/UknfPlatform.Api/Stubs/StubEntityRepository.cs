using UknfPlatform.Domain.Shared.Entities;
using UknfPlatform.Domain.Shared.Interfaces;

namespace UknfPlatform.Api.Stubs;

/// <summary>
/// Stub implementation of IEntityRepository for development
/// TODO: Replace with proper implementation when Epic 2 is complete
/// </summary>
public class StubEntityRepository : IEntityRepository
{
    // Hardcoded test entities for development
    private static readonly List<Entity> _testEntities = new()
    {
        Entity.Create(1001, "Test Bank S.A.", "Bank"),
        Entity.Create(1002, "Test Insurance Company", "Insurance"),
        Entity.Create(1003, "Example Credit Union", "Credit Union")
    };

    public Task<Entity?> GetByIdAsync(long entityId, CancellationToken cancellationToken = default)
    {
        var entity = _testEntities.FirstOrDefault(e => e.EntityId == entityId);
        return Task.FromResult(entity);
    }

    public Task<bool> ExistsAsync(long entityId, CancellationToken cancellationToken = default)
    {
        var exists = _testEntities.Any(e => e.EntityId == entityId);
        return Task.FromResult(exists);
    }

    public Task<IEnumerable<Entity>> GetUserEntitiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // For MVP, return all test entities
        // In production, this would query based on user permissions
        return Task.FromResult<IEnumerable<Entity>>(_testEntities);
    }
}

