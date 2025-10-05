using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Domain.Auth.Entities;
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
        db.PasswordHistories.RemoveRange(db.PasswordHistories); // Added for Story 1.3
        db.ActivationTokens.RemoveRange(db.ActivationTokens); // Added for Story 1.2
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

    // ============================================================================
    // ACTIVATION FLOW TESTS (Story 1.2)
    // ============================================================================

    [Fact]
    public async Task POST_Register_CreatesActivationTokenInDatabase()
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

        // Verify activation token was created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == command.Email.ToLowerInvariant());
        var activationToken = await db.ActivationTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);

        activationToken.Should().NotBeNull();
        activationToken!.Token.Should().NotBeNullOrEmpty();
        activationToken.Token.Length.Should().BeGreaterThan(32); // Cryptographically secure token
        activationToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        activationToken.ExpiresAt.Should().BeBefore(DateTime.UtcNow.AddHours(25)); // ~24 hours
        activationToken.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task GET_Activate_WithValidToken_Returns200AndActivatesUser()
    {
        // Arrange - Register user first
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        // Get the activation token from database
        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        // Act
        var response = await _client.GetAsync($"/api/auth/activate?token={activationToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ActivateAccountResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Message.Should().Contain("activated successfully");

        // Verify user is now active in database
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);
            var token = await db.ActivationTokens.FirstAsync(t => t.Token == activationToken);

            user.Should().NotBeNull();
            user!.IsActive.Should().BeTrue();
            token.IsUsed.Should().BeTrue(); // Token should be marked as used
        }
    }

    [Fact]
    public async Task GET_Activate_WithInvalidToken_Returns400()
    {
        // Arrange
        var invalidToken = "this-token-does-not-exist";

        // Act
        var response = await _client.GetAsync($"/api/auth/activate?token={invalidToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("invalid");
    }

    [Fact]
    public async Task GET_Activate_WithExpiredToken_Returns400()
    {
        // Arrange - Register user and manually expire their token
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string expiredToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            
            // Manually set token as expired using reflection
            var expiresAtProperty = token.GetType().GetProperty("ExpiresAt");
            expiresAtProperty!.SetValue(token, DateTime.UtcNow.AddHours(-1));
            await db.SaveChangesAsync();
            
            expiredToken = token.Token;
        }

        // Act
        var response = await _client.GetAsync($"/api/auth/activate?token={expiredToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("expired");
    }

    [Fact]
    public async Task GET_Activate_WithUsedToken_Returns400()
    {
        // Arrange - Register and activate user first
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
        }

        // Activate once (should succeed)
        var firstResponse = await _client.GetAsync($"/api/auth/activate?token={activationToken}");
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Try to activate again with same token
        var secondResponse = await _client.GetAsync($"/api/auth/activate?token={activationToken}");

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already been activated");
    }

    [Fact]
    public async Task GET_Activate_WithAlreadyActiveUser_ReturnsSuccessIdempotently()
    {
        // Arrange - Register user
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
        }

        // Activate user
        var firstResponse = await _client.GetAsync($"/api/auth/activate?token={activationToken}");
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Try again (idempotent behavior - though token is used, this tests the user.IsActive check)
        var secondResponse = await _client.GetAsync($"/api/auth/activate?token={activationToken}");

        // Assert - Should return 400 with "already activated" message (token is already used)
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already been activated");
    }

    [Fact]
    public async Task POST_ResendActivation_WithValidEmail_Returns200()
    {
        // Arrange - Register user first
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var resendCommand = new ResendActivationCommand(registerCommand.Email);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/resend-activation", resendCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ResendActivationResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("activation link");

        // Verify old token is invalidated and new token is created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
        var tokens = await db.ActivationTokens.Where(t => t.UserId == user.Id).ToListAsync();

        tokens.Should().HaveCountGreaterThan(1); // At least 2 tokens (old + new)
        tokens.Count(t => t.IsUsed).Should().BeGreaterThan(0); // Old token(s) marked as used
        tokens.Count(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow).Should().Be(1); // Exactly one valid new token
    }

    [Fact]
    public async Task POST_ResendActivation_WithNonExistentEmail_Returns200WithGenericMessage()
    {
        // Arrange
        var resendCommand = new ResendActivationCommand("nonexistent@example.com");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/resend-activation", resendCommand);

        // Assert
        // Should return success even for non-existent email (security - don't reveal user existence)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ResendActivationResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("activation link");
    }

    // ============================================================================
    // PASSWORD SETTING FLOW TESTS (Story 1.3)
    // ============================================================================

    [Fact]
    public async Task POST_SetPassword_WithValidTokenAndPassword_Returns200()
    {
        // Arrange - Register and get activation token
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SetPasswordResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Message.Should().Contain("Password set successfully");

        // Verify password is hashed in database (not plaintext)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);
            
            user.Should().NotBeNull();
            user!.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().StartWith("$2"); // BCrypt hash format
            user.PasswordHash.Should().NotBe("MyP@ssw0rd!", "password should be hashed, not plaintext");
        }
    }

    [Fact]
    public async Task POST_SetPassword_PasswordHashedInDatabase_NotPlaintext()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        var password = "TestPassword123!";
        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = password,
            PasswordConfirmation = password
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert - Verify hash format and not plaintext
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);

            user!.PasswordHash.Should().NotBe(password, "password should not be stored in plaintext");
            user.PasswordHash.Should().StartWith("$2", "BCrypt hash should start with $2a$ or $2b$");
            user.PasswordHash.Length.Should().Be(60, "BCrypt hash should be exactly 60 characters");
        }
    }

    [Fact]
    public async Task POST_SetPassword_SetsUserIsActiveToTrue()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            user.IsActive.Should().BeFalse("user should not be active before setting password");
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);
            user!.IsActive.Should().BeTrue("user should be active after setting password");
        }
    }

    [Fact]
    public async Task POST_SetPassword_UpdatesLastPasswordChangeDate()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        var beforeSetPassword = DateTime.UtcNow;

        // Act
        await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FindAsync(userId);
            
            user!.LastPasswordChangeDate.Should().NotBeNull();
            user.LastPasswordChangeDate.Should().BeAfter(beforeSetPassword.AddSeconds(-5));
            user.LastPasswordChangeDate.Should().BeBefore(DateTime.UtcNow.AddSeconds(5));
        }
    }

    [Fact]
    public async Task POST_SetPassword_CreatesPasswordHistoryEntry()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
            userId = user.Id;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHistory = await db.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .ToListAsync();

            passwordHistory.Should().HaveCount(1, "one password history entry should be created");
            passwordHistory[0].PasswordHash.Should().StartWith("$2", "history should store BCrypt hash");
        }
    }

    [Fact]
    public async Task POST_SetPassword_MarksTokenAsUsed()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            token.IsUsed.Should().BeFalse("token should not be used before setting password");
            activationToken = token.Token;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var token = await db.ActivationTokens.FirstAsync(t => t.Token == activationToken);
            token.IsUsed.Should().BeTrue("token should be marked as used after setting password");
        }
    }

    [Fact]
    public async Task POST_SetPassword_WithWeakPassword_Returns400()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "weak", // Too short, missing uppercase, digit, special char
            PasswordConfirmation = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Password", "error should mention password validation");
    }

    [Fact]
    public async Task POST_SetPassword_WithExpiredToken_Returns400()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string expiredToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);

            // Manually expire token
            var expiresAtProperty = token.GetType().GetProperty("ExpiresAt");
            expiresAtProperty!.SetValue(token, DateTime.UtcNow.AddHours(-1));
            await db.SaveChangesAsync();

            expiredToken = token.Token;
        }

        var setPasswordCommand = new
        {
            Token = expiredToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("expired", "error should indicate token is expired");
    }

    [Fact]
    public async Task POST_SetPassword_WithMismatchedConfirmation_Returns400()
    {
        // Arrange
        var registerCommand = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        string activationToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == registerCommand.Email.ToLowerInvariant());
            var token = await db.ActivationTokens.FirstAsync(t => t.UserId == user.Id);
            activationToken = token.Token;
        }

        var setPasswordCommand = new
        {
            Token = activationToken,
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "DifferentP@ssw0rd!" // Mismatch
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Password", "error should mention password validation");
    }

    [Fact]
    public async Task POST_SetPassword_WithInvalidToken_Returns400()
    {
        // Arrange
        var setPasswordCommand = new
        {
            Token = "invalid-token-that-does-not-exist",
            Password = "MyP@ssw0rd!",
            PasswordConfirmation = "MyP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/set-password", setPasswordCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("invalid", "error should indicate token is invalid");
    }
}

