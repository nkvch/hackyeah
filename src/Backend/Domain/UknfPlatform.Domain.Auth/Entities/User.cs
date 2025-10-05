using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// User entity representing system users (both internal and external)
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string PeselEncrypted { get; private set; } = string.Empty;
    public string PeselLast4 { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public DateTime? LastPasswordChangeDate { get; private set; }
    public string? PendingEmail { get; private set; } // Email awaiting confirmation
    public UserType UserType { get; private set; }
    public bool IsActive { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateTime? LastLoginDate { get; private set; }

    // EF Core requires parameterless constructor
    private User() { }

    /// <summary>
    /// Creates a new external user during registration
    /// </summary>
    public static User CreateExternal(
        string firstName,
        string lastName,
        string email,
        string phone,
        string peselEncrypted,
        string peselLast4)
    {
        ValidateRegistrationData(firstName, lastName, email, phone, peselEncrypted, peselLast4);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            Phone = phone,
            PeselEncrypted = peselEncrypted,
            PeselLast4 = peselLast4,
            UserType = UserType.External,
            IsActive = false, // Inactive until email activation
            MustChangePassword = false
        };
    }

    /// <summary>
    /// Sets the password hash for the user
    /// </summary>
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        PasswordHash = passwordHash;
        LastPasswordChangeDate = DateTime.UtcNow;
        MustChangePassword = false; // Clear password change requirement
        UpdateTimestamp();
    }

    /// <summary>
    /// Activates the user account after email verification
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Records a successful login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates user profile information
    /// </summary>
    public void UpdateProfile(string firstName, string lastName, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        UpdateTimestamp();
    }

    /// <summary>
    /// Sets the flag indicating the user must change their password
    /// </summary>
    public void RequirePasswordChange()
    {
        MustChangePassword = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Requests an email change by setting the pending email
    /// The change won't take effect until confirmed
    /// </summary>
    public void RequestEmailChange(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty", nameof(newEmail));

        PendingEmail = newEmail.ToLowerInvariant();
        UpdateTimestamp();
    }

    /// <summary>
    /// Confirms the email change and applies the pending email
    /// </summary>
    public void ConfirmEmailChange()
    {
        if (string.IsNullOrWhiteSpace(PendingEmail))
            throw new InvalidOperationException("No pending email change to confirm");

        Email = PendingEmail;
        PendingEmail = null;
        UpdateTimestamp();
    }

    /// <summary>
    /// Cancels a pending email change
    /// </summary>
    public void CancelEmailChange()
    {
        PendingEmail = null;
        UpdateTimestamp();
    }

    private static void ValidateRegistrationData(
        string firstName,
        string lastName,
        string email,
        string phone,
        string peselEncrypted,
        string peselLast4)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required", nameof(phone));
        if (string.IsNullOrWhiteSpace(peselEncrypted))
            throw new ArgumentException("PESEL is required", nameof(peselEncrypted));
        if (string.IsNullOrWhiteSpace(peselLast4) || peselLast4.Length != 4)
            throw new ArgumentException("PESEL last 4 digits are required", nameof(peselLast4));
    }
}

