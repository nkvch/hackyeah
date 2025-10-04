using FluentAssertions;
using UknfPlatform.Application.Auth.Authentication.Commands;

namespace UknfPlatform.UnitTests.Application.Auth;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "First name is required")]
    [InlineData(null, "First name is required")]
    public void Validate_EmptyFirstName_FailsValidation(string? firstName, string expectedError)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = firstName!,
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("First name is required"));
    }

    [Theory]
    [InlineData("", "Last name is required")]
    [InlineData(null, "Last name is required")]
    public void Validate_EmptyLastName_FailsValidation(string? lastName, string expectedError)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = lastName!,
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Last name is required"));
    }

    [Theory]
    [InlineData("", "Email is required")]
    [InlineData(null, "Email is required")]
    [InlineData("invalid-email", "Invalid email format")]
    [InlineData("@example.com", "Invalid email format")]
    [InlineData("test@", "Invalid email format")]
    public void Validate_InvalidEmail_FailsValidation(string? email, string expectedError)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = email!,
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("first.last+tag@example.com")]
    public void Validate_ValidEmail_PassesValidation(string email)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = email,
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Phone number is required")]
    [InlineData(null, "Phone number is required")]
    [InlineData("123456789", "international format")]
    [InlineData("48123456789", "international format")]
    [InlineData("+48", "international format")]
    [InlineData("+123", "international format")]
    public void Validate_InvalidPhone_FailsValidation(string? phone, string expectedMessagePart)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = phone!,
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("+48123456789")]
    [InlineData("+1234567890")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("+1 555 123 4567")]
    public void Validate_ValidPhone_PassesValidation(string phone)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = phone,
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "PESEL is required")]
    [InlineData(null, "PESEL is required")]
    [InlineData("123", "11 digits")]
    [InlineData("1234567890", "11 digits")]
    [InlineData("123456789012", "11 digits")]
    [InlineData("1234567890a", "only digits")]
    [InlineData("12345 67890", "only digits")]
    public void Validate_InvalidPesel_FailsValidation(string? pesel, string expectedMessagePart)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = pesel!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("98765432109")]
    [InlineData("00000000000")]
    public void Validate_ValidPesel_PassesValidation(string pesel)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = pesel
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FirstNameTooLong_FailsValidation()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = new string('a', 101), // 101 characters
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("100 characters"));
    }

    [Fact]
    public void Validate_EmailTooLong_FailsValidation()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // > 256 characters
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = longEmail,
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("256 characters"));
    }
}

