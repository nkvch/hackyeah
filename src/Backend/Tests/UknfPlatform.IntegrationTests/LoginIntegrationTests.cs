using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Tests.Integration.Api.Auth;

public class LoginIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LoginIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("login@test.com", password);

        var request = new LoginCommand("login@test.com", password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().Be(3600);
        result.User.Email.Should().Be(user.Email);
        result.User.FirstName.Should().Be(user.FirstName);
        result.User.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginCommand("nonexistent@test.com", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = await CreateActivatedUserWithPasswordAsync("user@test.com", "CorrectPassword123!");

        var request = new LoginCommand("user@test.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NotActivatedAccount_ReturnsForbidden()
    {
        // Arrange
        var user = await CreateUserAsync("notactivated@test.com");
        // Don't activate

        var request = new LoginCommand("notactivated@test.com", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_PasswordNotSet_ReturnsUnauthorized()
    {
        // Arrange
        var user = await CreateUserAsync("nopassword@test.com");
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            user.Activate();
            await dbContext.SaveChangesAsync();
        }

        var request = new LoginCommand("nopassword@test.com", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginCommand("invalid-email", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginCommand("test@test.com", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_SuccessfulLogin_CreatesRefreshToken()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("refresh@test.com", password);

        var request = new LoginCommand("refresh@test.com", password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id && rt.Token == result!.RefreshToken);

        refreshToken.Should().NotBeNull();
        refreshToken!.IsActive.Should().BeTrue();
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Login_SuccessfulLogin_CreatesAuditLog()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("audit@test.com", password);

        var request = new LoginCommand("audit@test.com", password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var auditLog = await dbContext.AuthenticationAuditLogs
            .FirstOrDefaultAsync(al => al.UserId == user.Id && al.Email == user.Email);

        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("Login");
        auditLog.Success.Should().BeTrue();
        auditLog.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task Login_FailedLogin_CreatesAuditLog()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("failaudit@test.com", password);

        var request = new LoginCommand("failaudit@test.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var auditLog = await dbContext.AuthenticationAuditLogs
            .FirstOrDefaultAsync(al => al.UserId == user.Id && al.Email == user.Email && !al.Success);

        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("Login");
        auditLog.Success.Should().BeFalse();
        auditLog.FailureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_SuccessfulLogin_UpdatesLastLoginDate()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("lastlogin@test.com", password);
        var previousLastLogin = user.LastLoginDate;

        var request = new LoginCommand("lastlogin@test.com", password);

        // Act
        await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser!.LastLoginDate.Should().NotBe(previousLastLogin);
        updatedUser.LastLoginDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewAccessToken()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("refreshtoken@test.com", password);

        var loginRequest = new LoginCommand("refreshtoken@test.com", password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new RefreshTokenCommand(loginResult!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().NotBe(loginResult.AccessToken);
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Logout_ValidRefreshToken_RevokesToken()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = await CreateActivatedUserWithPasswordAsync("logout@test.com", password);

        var loginRequest = new LoginCommand("logout@test.com", password);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var logoutRequest = new LogoutCommand(loginResult!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify token is revoked
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);

        refreshToken.Should().NotBeNull();
        refreshToken!.RevokedAt.Should().NotBeNull();
        refreshToken.IsActive.Should().BeFalse();
    }

    private async Task<User> CreateUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.CreateExternal(
            "Test",
            "User",
            email,
            "1234567890",
            "encrypted-pesel",
            "1234"
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<User> CreateActivatedUserWithPasswordAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = User.CreateExternal(
            "Test",
            "User",
            email,
            "1234567890",
            "encrypted-pesel",
            "1234"
        );

        user.Activate();
        var hashedPassword = passwordHasher.HashPassword(password);
        user.SetPassword(hashedPassword);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }
}
