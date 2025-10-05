using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Infrastructure.Identity.Services;

namespace UknfPlatform.Tests.Unit.Infrastructure.Identity;

public class JwtTokenServiceTests
{
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _loggerMock = new Mock<ILogger<JwtTokenService>>();
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-minimum-32-characters-required-for-security",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_jwtSettings);
        _service = new JwtTokenService(options, _loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = User.CreateExternal(
            "Test",
            "User",
            "test@example.com",
            "1234567890",
            "encrypted-pesel",
            "1234"
        );

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == $"{user.FirstName} {user.LastName}");
        jwtToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == user.UserType.ToString());
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_ExternalUser_IncludesCorrectUserType()
    {
        // Arrange
        var user = User.CreateExternal(
            "Test",
            "User",
            "test@example.com",
            "1234567890",
            "encrypted-pesel",
            "1234"
        );

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "External");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64EncodedString()
    {
        // Act
        var token = _service.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(40); // 32 bytes base64 encoded
        
        // Should be valid base64
        FluentActions.Invoking(() => Convert.FromBase64String(token))
            .Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();
        var token3 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = User.CreateExternal(
            "Test",
            "User",
            "test@example.com",
            "1234567890",
            "encrypted-pesel",
            "1234"
        );
        var token = _service.GenerateAccessToken(user);

        // Act
        var isValid = _service.ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _service.ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }
}
