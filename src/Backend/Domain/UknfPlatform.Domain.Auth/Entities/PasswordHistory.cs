namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Represents a historical password hash for a user
/// Used to prevent password reuse as per security policy
/// </summary>
public class PasswordHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    // EF Core constructor
    private PasswordHistory() { }

    private PasswordHistory(Guid userId, string passwordHash, DateTime createdDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        CreatedDate = createdDate;
    }

    /// <summary>
    /// Creates a new password history entry
    /// </summary>
    public static PasswordHistory Create(Guid userId, string passwordHash)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        return new PasswordHistory(userId, passwordHash, DateTime.UtcNow);
    }
}

