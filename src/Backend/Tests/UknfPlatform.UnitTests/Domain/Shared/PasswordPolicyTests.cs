using FluentAssertions;
using UknfPlatform.Domain.Shared.ValueObjects;

namespace UknfPlatform.UnitTests.Domain.Shared;

/// <summary>
/// Unit tests for PasswordPolicy value object
/// Verifies password validation rules and strength calculation
/// </summary>
public class PasswordPolicyTests
{
    private readonly PasswordPolicy _defaultPolicy;

    public PasswordPolicyTests()
    {
        _defaultPolicy = new PasswordPolicy(
            minLength: 8,
            maxLength: 128,
            requireUppercase: true,
            requireLowercase: true,
            requireDigit: true,
            requireSpecialChar: true,
            minUniqueChars: 5
        );
    }

    [Fact]
    public void Validate_PasswordMeetsAllRequirements_ReturnsValid()
    {
        // Arrange
        var validPassword = "MyP@ssw0rd!";

        // Act
        var result = _defaultPolicy.Validate(validPassword);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_PasswordTooShort_ReturnsInvalid()
    {
        // Arrange
        var shortPassword = "Ab1!"; // Only 4 characters

        // Act
        var result = _defaultPolicy.Validate(shortPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 8 characters"));
    }

    [Fact]
    public void Validate_PasswordMissingUppercase_ReturnsInvalid()
    {
        // Arrange
        var password = "mypassword123!"; // No uppercase

        // Act
        var result = _defaultPolicy.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase letter"));
    }

    [Fact]
    public void Validate_PasswordMissingLowercase_ReturnsInvalid()
    {
        // Arrange
        var password = "MYPASSWORD123!"; // No lowercase

        // Act
        var result = _defaultPolicy.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase letter"));
    }

    [Fact]
    public void Validate_PasswordMissingDigit_ReturnsInvalid()
    {
        // Arrange
        var password = "MyPassword!"; // No digit

        // Act
        var result = _defaultPolicy.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public void Validate_PasswordMissingSpecialChar_ReturnsInvalid()
    {
        // Arrange
        var password = "MyPassword123"; // No special character

        // Act
        var result = _defaultPolicy.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }

    [Fact]
    public void Validate_PasswordInsufficientUniqueChars_ReturnsInvalid()
    {
        // Arrange
        var password = "Aaaa1!"; // Only 3 unique chars: A, a, 1, !

        // Act
        var result = _defaultPolicy.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 5 unique characters"));
    }

    [Fact]
    public void Validate_PasswordTooLong_ReturnsInvalid()
    {
        // Arrange
        var longPassword = new string('a', 129) + "A1!"; // 133 characters

        // Act
        var result = _defaultPolicy.Validate(longPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("must not exceed 128 characters"));
    }

    [Fact]
    public void Validate_NullPassword_ReturnsInvalid()
    {
        // Arrange
        string nullPassword = null!;

        // Act
        var result = _defaultPolicy.Validate(nullPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Password is required"));
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsInvalid()
    {
        // Arrange
        var emptyPassword = string.Empty;

        // Act
        var result = _defaultPolicy.Validate(emptyPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Password is required"));
    }

    [Fact]
    public void GetStrengthScore_StrongPassword_ReturnsHighScore()
    {
        // Arrange
        var strongPassword = "MyV3ry$tr0ngP@ssw0rd!";

        // Act
        var score = _defaultPolicy.GetStrengthScore(strongPassword);

        // Assert
        score.Should().BeGreaterThan(80, "strong password should score above 80");
        var strength = _defaultPolicy.GetStrengthLevel(score);
        strength.Should().Be(PasswordStrength.Strong);
    }

    [Fact]
    public void GetStrengthScore_WeakPassword_ReturnsLowScore()
    {
        // Arrange
        var weakPassword = "pass"; // Very short, common pattern, no complexity

        // Act
        var score = _defaultPolicy.GetStrengthScore(weakPassword);

        // Assert
        score.Should().BeLessThanOrEqualTo(40, "weak password should score 40 or below");
        var strength = _defaultPolicy.GetStrengthLevel(score);
        strength.Should().Be(PasswordStrength.Weak);
    }

    [Fact]
    public void GetStrengthScore_CommonPattern_PenalizesScore()
    {
        // Arrange
        var passwordWithCommonPattern = "MyPassword123!"; // Contains "password"
        var passwordWithoutCommonPattern = "MyS3cr3tW0rd!";

        // Act
        var scoreWithPattern = _defaultPolicy.GetStrengthScore(passwordWithCommonPattern);
        var scoreWithoutPattern = _defaultPolicy.GetStrengthScore(passwordWithoutCommonPattern);

        // Assert
        scoreWithPattern.Should().BeLessThan(scoreWithoutPattern, "password with common pattern should score lower");
    }

    [Fact]
    public void GetStrengthScore_SequentialChars_PenalizesScore()
    {
        // Arrange
        var passwordWithSequential = "Abc123!@#XYZ"; // Contains "abc", "123", "XYZ"
        var passwordWithoutSequential = "Xb3!@#ZcA"; // Same chars, no sequence

        // Act
        var scoreWithSequential = _defaultPolicy.GetStrengthScore(passwordWithSequential);
        var scoreWithoutSequential = _defaultPolicy.GetStrengthScore(passwordWithoutSequential);

        // Assert
        scoreWithSequential.Should().BeLessThan(scoreWithoutSequential, "password with sequential chars should score lower");
    }

    [Fact]
    public void GetStrengthScore_RepeatingChars_PenalizesScore()
    {
        // Arrange
        var passwordWithRepeating = "Aaa111!!!Bb"; // Contains "aaa", "111", "!!!"
        var passwordWithoutRepeating = "Xa1!bYc2#"; // Same char types, no repeats

        // Act
        var scoreWithRepeating = _defaultPolicy.GetStrengthScore(passwordWithRepeating);
        var scoreWithoutRepeating = _defaultPolicy.GetStrengthScore(passwordWithoutRepeating);

        // Assert
        scoreWithRepeating.Should().BeLessThan(scoreWithoutRepeating, "password with repeating chars should score lower");
    }

    [Theory]
    [InlineData("abc", PasswordStrength.Weak)]  // Very short, no complexity
    [InlineData("Ab1", PasswordStrength.Fair)]  // Short but has mixed case and digit
    [InlineData("Pass123!", PasswordStrength.Good)]  // 8 chars with mixed complexity
    [InlineData("MyP@ssw0rd2024", PasswordStrength.Strong)]  // Long with diversity
    [InlineData("Xk7#mP9@nQ2$wL5!", PasswordStrength.Strong)]  // Very strong
    public void GetStrengthLevel_VariousPasswords_ReturnsCorrectLevel(string password, PasswordStrength expectedStrength)
    {
        // Act
        var score = _defaultPolicy.GetStrengthScore(password);
        var strength = _defaultPolicy.GetStrengthLevel(score);

        // Assert
        strength.Should().Be(expectedStrength);
    }

    [Fact]
    public void Constructor_InvalidMinLength_ThrowsArgumentException()
    {
        // Act
        Action act = () => new PasswordPolicy(0, 128, true, true, true, true, 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Minimum length must be at least 1*");
    }

    [Fact]
    public void Constructor_MaxLengthLessThanMinLength_ThrowsArgumentException()
    {
        // Act
        Action act = () => new PasswordPolicy(10, 5, true, true, true, true, 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Maximum length must be greater than or equal to minimum length*");
    }

    [Fact]
    public void Constructor_NegativeMinUniqueChars_ThrowsArgumentException()
    {
        // Act
        Action act = () => new PasswordPolicy(8, 128, true, true, true, true, -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Minimum unique characters cannot be negative*");
    }

    [Fact]
    public void Default_ReturnsStandardPolicy()
    {
        // Act
        var defaultPolicy = PasswordPolicy.Default;

        // Assert
        defaultPolicy.MinLength.Should().Be(8);
        defaultPolicy.MaxLength.Should().Be(128);
        defaultPolicy.RequireUppercase.Should().BeTrue();
        defaultPolicy.RequireLowercase.Should().BeTrue();
        defaultPolicy.RequireDigit.Should().BeTrue();
        defaultPolicy.RequireSpecialChar.Should().BeTrue();
        defaultPolicy.MinUniqueChars.Should().Be(5);
    }
}

