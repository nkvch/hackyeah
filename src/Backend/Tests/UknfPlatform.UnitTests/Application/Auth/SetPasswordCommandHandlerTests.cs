using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.UnitTests.Application.Auth;

/// <summary>
/// Unit tests for SetPasswordCommandHandler
/// Verifies password setting business logic, validation, and history
/// </summary>
public class SetPasswordCommandHandlerTests
{
    private readonly Mock<IActivationTokenRepository> _activationTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IOptions<PasswordPolicySettings>> _passwordPolicyMock;
    private readonly Mock<IOptions<ApplicationSettings>> _applicationSettingsMock;
    private readonly Mock<ILogger<SetPasswordCommandHandler>> _loggerMock;
    private readonly SetPasswordCommandHandler _handler;

    public SetPasswordCommandHandlerTests()
    {
        _activationTokenRepositoryMock = new Mock<IActivationTokenRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHistoryRepositoryMock = new Mock<IPasswordHistoryRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<SetPasswordCommandHandler>>();

        var passwordPolicy = new PasswordPolicySettings
        {
            MinLength = 8,
            PasswordHistoryCount = 5
        };
        _passwordPolicyMock = new Mock<IOptions<PasswordPolicySettings>>();
        _passwordPolicyMock.Setup(x => x.Value).Returns(passwordPolicy);

        var applicationSettings = new ApplicationSettings
        {
            FrontendUrl = "http://localhost:4200"
        };
        _applicationSettingsMock = new Mock<IOptions<ApplicationSettings>>();
        _applicationSettingsMock.Setup(x => x.Value).Returns(applicationSettings);

        _handler = new SetPasswordCommandHandler(
            _activationTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _passwordHistoryRepositoryMock.Object,
            _passwordHasherMock.Object,
            _passwordPolicyMock.Object,
            _applicationSettingsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidTokenAndPassword_SetsPasswordSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _activationTokenRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _passwordHistoryRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Message.Should().Contain("Password set successfully");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidTokenException()
    {
        // Arrange
        var command = new SetPasswordCommand("invalid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActivationToken?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidTokenException>()
            .WithMessage("*invalid*");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsTokenExpiredException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        // Expire the token
        typeof(ActivationToken).GetProperty("ExpiresAt")!.SetValue(token, DateTime.UtcNow.AddHours(-1));

        var command = new SetPasswordCommand("expired-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TokenExpiredException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_AlreadyUsedToken_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        // User does NOT have password set yet (that's the key difference from idempotent test)
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        token.MarkAsUsed();
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        // Note: IsValid() returns false when token is used, so TokenExpiredException is thrown first
        // This is expected behavior - used tokens are considered invalid

        var command = new SetPasswordCommand("used-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert - IsValid() checks both expiration and usage, so TokenExpiredException is thrown
        await act.Should().ThrowAsync<TokenExpiredException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_PasswordInHistory_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var oldPasswordHash = "old-hashed-password";
        var historicalPassword = PasswordHistory.Create(userId, oldPasswordHash);
        var passwordHistory = new List<PasswordHistory> { historicalPassword };

        var command = new SetPasswordCommand("valid-token", "OldPassword123!", "OldPassword123!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordHistory);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("OldPassword123!", oldPasswordHash))
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*was used recently*");
    }

    [Fact]
    public async Task Handle_ValidPassword_HashesPasswordBeforeStorage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHasherMock
            .Setup(x => x.HashPassword("MyP@ssw0rd!"))
            .Returns("securely-hashed-password");

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _activationTokenRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _passwordHistoryRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasherMock.Verify(x => x.HashPassword("MyP@ssw0rd!"), Times.Once);
        user.PasswordHash.Should().Be("securely-hashed-password", "password should be hashed before storage");
    }

    [Fact]
    public async Task Handle_ValidPassword_AddsPasswordToHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _activationTokenRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _passwordHistoryRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHistoryRepositoryMock.Verify(
            x => x.AddAsync(It.Is<PasswordHistory>(ph => ph.UserId == userId), It.IsAny<CancellationToken>()),
            Times.Once,
            "password should be added to history");
    }

    [Fact]
    public async Task Handle_ValidPassword_ActivatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _activationTokenRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _passwordHistoryRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsActive.Should().BeTrue("user should be activated after setting password");
    }

    [Fact]
    public async Task Handle_ValidPassword_MarksTokenAsUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed-password");

        _passwordHistoryRepositoryMock
            .Setup(x => x.GetRecentPasswordsAsync(userId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _activationTokenRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _passwordHistoryRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        token.IsUsed.Should().BeTrue("token should be marked as used after password is set");
    }

    [Fact]
    public async Task Handle_PasswordAlreadySet_ReturnsSuccessIdempotently()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal("Jan", "Kowalski", "jan@example.com", "+48123456789", "encrypted", "1234");
        user.SetPassword("already-set-password-hash");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var token = ActivationToken.Create(userId);
        typeof(ActivationToken).GetProperty("User")!.SetValue(token, user);

        var command = new SetPasswordCommand("valid-token", "MyP@ssw0rd!", "MyP@ssw0rd!");

        _activationTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Message.Should().Contain("already been set");
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never, "should not hash password again if already set");
    }
}

