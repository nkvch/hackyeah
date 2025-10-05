using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Shared.Entities;

/// <summary>
/// Supervised entity (e.g., Bank, Insurance Company)
/// This is a shared entity used across multiple modules
/// Full implementation will be added in Epic 2
/// </summary>
public class Entity : BaseEntity
{
    public long EntityId { get; private set; } // UKNF Code (numeric ID)
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // e.g., "Bank", "Insurance"
    public bool IsActive { get; private set; }

    // EF Core requires parameterless constructor
    private Entity() { }

    /// <summary>
    /// Creates a new entity (stub implementation)
    /// </summary>
    public static Entity Create(long entityId, string name, string type)
    {
        return new Entity
        {
            EntityId = entityId,
            Name = name,
            Type = type,
            IsActive = true
        };
    }
}

