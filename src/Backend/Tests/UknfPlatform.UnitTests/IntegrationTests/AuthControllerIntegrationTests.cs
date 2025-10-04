using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.UnitTests.IntegrationTests;

/// <summary>
/// Integration tests for AuthController registration endpoint
/// Tests the full stack: API -> Application -> Domain -> Database
/// </summary>
public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactoryBase>, IAsyncLifetime
{
    private readonly WebApplicationFactoryBase _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactoryBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        // Clean database before each test
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task POST_Register_WithValidData_Returns201AndCreatesUser()
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
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
        result.Message.Should().Contain("Registration successful");

        // Verify user exists in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email.ToLowerInvariant());
        
        user.Should().NotBeNull();
        user!.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
        user.Email.Should().Be(command.Email.ToLowerInvariant());
        user.Phone.Should().Be(command.Phone);
        user.PeselLast4.Should().Be("8901"); // Last 4 digits of PESEL
        user.PeselEncrypted.Should().NotBeNullOrEmpty();
        user.IsActive.Should().BeFalse(); // Should be inactive until email activation
    }

    [Fact]
    public async Task POST_Register_WithExistingEmail_Returns409()
    {
        // Arrange - Create first user
        var firstCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "duplicate@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", firstCommand);

        // Arrange - Try to create user with same email but different PESEL
        var duplicateCommand = new RegisterUserCommand
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "duplicate@example.com",
            Phone = "+48987654321",
            Pesel = "98765432109"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("email already exists");
    }

    [Fact]
    public async Task POST_Register_WithExistingPesel_Returns409()
    {
        // Arrange - Create first user
        var firstCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "11111111111"
        };
        await _client.PostAsJsonAsync("/api/auth/register", firstCommand);

        // Arrange - Try to create user with different email but same PESEL
        var duplicateCommand = new RegisterUserCommand
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna@example.com",
            Phone = "+48987654321",
            Pesel = "11111111111"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("PESEL already exists");
    }

    [Fact]
    public async Task POST_Register_WithInvalidEmail_Returns400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "invalid-email", // Invalid email format
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task POST_Register_WithInvalidPhone_Returns400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "123456789", // Missing + prefix (not international format)
            Pesel = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("international format");
    }

    [Fact]
    public async Task POST_Register_WithInvalidPesel_Returns400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "123" // Too short
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("11 digits");
    }

    [Fact]
    public async Task POST_Register_WithMissingRequiredFields_Returns400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "", // Empty
            LastName = "", // Empty
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Register_WithVeryLongNames_Returns400()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = new string('a', 101), // 101 characters (max is 100)
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("100 characters");
    }

    [Fact]
    public async Task POST_Register_MultipleValidUsers_AllSucceed()
    {
        // Arrange
        var users = new[]
        {
            new RegisterUserCommand
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan@example.com",
                Phone = "+48123456789",
                Pesel = "11111111111"
            },
            new RegisterUserCommand
            {
                FirstName = "Anna",
                LastName = "Nowak",
                Email = "anna@example.com",
                Phone = "+48987654321",
                Pesel = "22222222222"
            },
            new RegisterUserCommand
            {
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                Email = "piotr@example.com",
                Phone = "+48555666777",
                Pesel = "33333333333"
            }
        };

        // Act & Assert
        foreach (var user in users)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", user);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Verify all users exist in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userCount = await db.Users.CountAsync();
        userCount.Should().Be(3);
    }

    [Fact]
    public async Task POST_Register_PeselIsEncryptedInDatabase()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify PESEL is encrypted (not stored in plain text)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == command.Email.ToLowerInvariant());
        
        user.PeselEncrypted.Should().NotBe(command.Pesel);
        user.PeselEncrypted.Should().NotBeNullOrEmpty();
        user.PeselLast4.Should().Be("8901");
    }
}

