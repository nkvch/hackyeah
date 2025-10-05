using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Infrastructure.Identity.Services;

namespace UknfPlatform.UnitTests.Infrastructure.Identity;

/// <summary>
/// Unit tests for PasswordHasher service
/// Verifies BCrypt password hashing and verification
/// </summary>
public class PasswordHasherTests
{
    private readonly Mock<ILogger<PasswordHasher>> _loggerMock;
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _loggerMock = new Mock<ILogger<PasswordHasher>>();
        _passwordHasher = new PasswordHasher(_loggerMock.Object);
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsValidBCryptHash()
    {
        // Arrange
        var password = "MyP@ssw0rd!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2"); // BCrypt format starts with $2a$ or $2b$
        hash.Length.Should().Be(60); // BCrypt hash length is always 60 characters
    }

    [Fact]
    public void HashPassword_SamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "MyP@ssw0rd!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
        hash1.Should().NotBe(hash2, "BCrypt generates random salt per hash, so same password should produce different hashes");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MyP@ssw0rd!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue("correct password should verify against its hash");
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "MyP@ssw0rd!";
        var incorrectPassword = "WrongPassword123!";
        var hash = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse("incorrect password should not verify against hash");
    }

    [Fact]
    public void HashPassword_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        string nullPassword = null!;

        // Act
        Action act = () => _passwordHasher.HashPassword(nullPassword);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be null or empty*");
    }

    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var emptyPassword = string.Empty;

        // Act
        Action act = () => _passwordHasher.HashPassword(emptyPassword);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be null or empty*");
    }

    [Fact]
    public void VerifyPassword_NullPassword_ThrowsArgumentException()
    {
        // Arrange
        string nullPassword = null!;
        var hash = _passwordHasher.HashPassword("ValidPassword123!");

        // Act
        Action act = () => _passwordHasher.VerifyPassword(nullPassword, hash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be null or empty*");
    }

    [Fact]
    public void VerifyPassword_NullHash_ThrowsArgumentException()
    {
        // Arrange
        var password = "ValidPassword123!";
        string nullHash = null!;

        // Act
        Action act = () => _passwordHasher.VerifyPassword(password, nullHash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Hash cannot be null or empty*");
    }
}

