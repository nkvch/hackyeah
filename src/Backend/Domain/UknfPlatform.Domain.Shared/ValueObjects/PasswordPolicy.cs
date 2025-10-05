using System.Text.RegularExpressions;

namespace UknfPlatform.Domain.Shared.ValueObjects;

/// <summary>
/// Value object representing password policy requirements
/// Validates passwords against configurable security rules
/// </summary>
public class PasswordPolicy
{
    public int MinLength { get; }
    public int MaxLength { get; }
    public bool RequireUppercase { get; }
    public bool RequireLowercase { get; }
    public bool RequireDigit { get; }
    public bool RequireSpecialChar { get; }
    public int MinUniqueChars { get; }

    // Default policy for UKNF platform
    public static PasswordPolicy Default => new(
        minLength: 8,
        maxLength: 128,
        requireUppercase: true,
        requireLowercase: true,
        requireDigit: true,
        requireSpecialChar: true,
        minUniqueChars: 5
    );

    public PasswordPolicy(
        int minLength,
        int maxLength,
        bool requireUppercase,
        bool requireLowercase,
        bool requireDigit,
        bool requireSpecialChar,
        int minUniqueChars)
    {
        if (minLength < 1) throw new ArgumentException("Minimum length must be at least 1", nameof(minLength));
        if (maxLength < minLength) throw new ArgumentException("Maximum length must be greater than or equal to minimum length", nameof(maxLength));
        if (minUniqueChars < 0) throw new ArgumentException("Minimum unique characters cannot be negative", nameof(minUniqueChars));

        MinLength = minLength;
        MaxLength = maxLength;
        RequireUppercase = requireUppercase;
        RequireLowercase = requireLowercase;
        RequireDigit = requireDigit;
        RequireSpecialChar = requireSpecialChar;
        MinUniqueChars = minUniqueChars;
    }

    /// <summary>
    /// Validates a password against this policy
    /// </summary>
    /// <returns>Validation result with success flag and error messages</returns>
    public PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required");
            return new PasswordValidationResult(false, errors);
        }

        if (password.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters");

        if (password.Length > MaxLength)
            errors.Add($"Password must not exceed {MaxLength} characters");

        if (RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        if (RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        if (RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");

        if (RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            errors.Add("Password must contain at least one special character");

        var uniqueCharCount = password.Distinct().Count();
        if (uniqueCharCount < MinUniqueChars)
            errors.Add($"Password must contain at least {MinUniqueChars} unique characters");

        return new PasswordValidationResult(errors.Count == 0, errors);
    }

    /// <summary>
    /// Calculates password strength score (0-100)
    /// Higher score = stronger password
    /// </summary>
    public int GetStrengthScore(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        int score = 0;

        // Length contribution (up to 40 points)
        score += Math.Min(password.Length * 4, 40);

        // Character diversity (up to 30 points)
        if (password.Any(char.IsUpper)) score += 10;
        if (password.Any(char.IsLower)) score += 10;
        if (password.Any(char.IsDigit)) score += 5;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 5;

        // Uniqueness (up to 20 points)
        int uniqueChars = password.Distinct().Count();
        score += Math.Min(uniqueChars * 2, 20);

        // Penalties (up to -30 points)
        if (ContainsCommonPattern(password)) score -= 10;
        if (ContainsRepeatingChars(password)) score -= 10;
        if (ContainsSequentialChars(password)) score -= 10;

        return Math.Max(0, Math.Min(score, 100));
    }

    /// <summary>
    /// Gets human-readable strength level from score
    /// </summary>
    public PasswordStrength GetStrengthLevel(int score)
    {
        return score switch
        {
            <= 40 => PasswordStrength.Weak,
            <= 60 => PasswordStrength.Fair,
            <= 80 => PasswordStrength.Good,
            _ => PasswordStrength.Strong
        };
    }

    private static bool ContainsCommonPattern(string password)
    {
        var commonPatterns = new[]
        {
            "password", "123456", "qwerty", "admin", "welcome",
            "letmein", "monkey", "dragon", "master", "sunshine"
        };

        return commonPatterns.Any(pattern =>
            password.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsRepeatingChars(string password)
    {
        // Check for 3+ repeating characters (e.g., "aaa", "111")
        for (int i = 0; i < password.Length - 2; i++)
        {
            if (password[i] == password[i + 1] && password[i] == password[i + 2])
                return true;
        }
        return false;
    }

    private static bool ContainsSequentialChars(string password)
    {
        // Check for sequential characters (e.g., "abc", "123", "xyz")
        var lower = password.ToLower();
        for (int i = 0; i < lower.Length - 2; i++)
        {
            if (lower[i] + 1 == lower[i + 1] && lower[i] + 2 == lower[i + 2])
                return true;
            if (lower[i] - 1 == lower[i + 1] && lower[i] - 2 == lower[i + 2])
                return true;
        }
        return false;
    }
}

/// <summary>
/// Result of password validation
/// </summary>
public record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors);

/// <summary>
/// Password strength levels
/// </summary>
public enum PasswordStrength
{
    Weak = 0,
    Fair = 1,
    Good = 2,
    Strong = 3
}

