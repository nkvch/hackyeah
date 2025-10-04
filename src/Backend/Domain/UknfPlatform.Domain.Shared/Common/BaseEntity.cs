namespace UknfPlatform.Domain.Shared.Common;

/// <summary>
/// Base entity with common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
    public DateTime UpdatedDate { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    protected void UpdateTimestamp()
    {
        UpdatedDate = DateTime.UtcNow;
    }
}

