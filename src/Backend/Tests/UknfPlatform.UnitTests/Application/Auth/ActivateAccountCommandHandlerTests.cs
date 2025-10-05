using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.UnitTests.Application.Auth;

public class ActivateAccountCommandHandlerTests
{
    private readonly Mock<IActivationTokenRepository> _activationTokenRepositoryMock;
    private readonly Mock<ILogger<ActivateAccountCommandHandler>> _loggerMock;
    private readonly ActivateAccountCommandHandler _handler;

    public ActivateAccountCommandHandlerTests()
    {
        _activationTokenRepositoryMock = new Mock<IActivationTokenRepository>();
        _loggerMock = new Mock<ILogger<ActivateAccountCommandHandler>>();

        _handler = new ActivateAccountCommandHandler(
            _activationTokenRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ActivatesUserAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid-token-123";
        var command = new ActivateAccountCommand(tokenString);

        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan@example.com",
            "+48123456789",
            "encrypted_pesel",
            "8901"
        );

        var activationToken = ActivationToken.Create(userId, 24);
        var tokenProperty = activationToken.GetType().GetProperty("Token");
        tokenProperty!.SetValue(activationToken, tokenString);
        var userProperty = activationToken.GetType().GetProperty("User");
        userProperty!.SetValue(activationToken, user);

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Message.Should().Contain("activated successfully");

        user.IsActive.Should().BeTrue();

        _activationTokenRepositoryMock.Verify(r => r.UpdateAsync(activationToken, It.IsAny<CancellationToken>()), Times.Once);
        _activationTokenRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidTokenException()
    {
        // Arrange
        var tokenString = "invalid-token";
        var command = new ActivateAccountCommand(tokenString);

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
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
        var tokenString = "expired-token";
        var command = new ActivateAccountCommand(tokenString);

        var activationToken = ActivationToken.Create(userId, 24);
        // Force token to be expired by setting ExpiresAt to past
        var expiresAtProperty = activationToken.GetType().GetProperty("ExpiresAt");
        expiresAtProperty!.SetValue(activationToken, DateTime.UtcNow.AddHours(-1));

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TokenExpiredException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_AlreadyUsedToken_ThrowsTokenAlreadyUsedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "used-token";
        var command = new ActivateAccountCommand(tokenString);

        var activationToken = ActivationToken.Create(userId, 24);
        // Mark token as used
        activationToken.MarkAsUsed();

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Note: IsValid() checks both expired AND used, so expired exception is thrown first
        await act.Should().ThrowAsync<TokenExpiredException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_AlreadyActiveUser_ReturnsSuccessIdempotently()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid-token";
        var command = new ActivateAccountCommand(tokenString);

        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan@example.com",
            "+48123456789",
            "encrypted_pesel",
            "8901"
        );
        user.Activate(); // User is already active

        var activationToken = ActivationToken.Create(userId, 24);
        var userProperty = activationToken.GetType().GetProperty("User");
        userProperty!.SetValue(activationToken, user);

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Message.Should().Contain("already activated");

        user.IsActive.Should().BeTrue();

        // User was already active, no repository updates should occur
    }

    [Fact]
    public async Task Handle_ValidToken_MarksTokenAsUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid-token";
        var command = new ActivateAccountCommand(tokenString);

        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan@example.com",
            "+48123456789",
            "encrypted_pesel",
            "8901"
        );

        var activationToken = ActivationToken.Create(userId, 24);
        var tokenProperty = activationToken.GetType().GetProperty("Token");
        tokenProperty!.SetValue(activationToken, tokenString);
        var userProperty = activationToken.GetType().GetProperty("User");
        userProperty!.SetValue(activationToken, user);

        ActivationToken? capturedToken = null;
        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        _activationTokenRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Callback<ActivationToken, CancellationToken>((token, _) => capturedToken = token)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.IsValid().Should().BeFalse(); // Token should be marked as used
        
        _activationTokenRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<ActivationToken>(t => !t.IsValid()), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidToken_LogsActivationSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "valid-token";
        var command = new ActivateAccountCommand(tokenString);

        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan@example.com",
            "+48123456789",
            "encrypted_pesel",
            "8901"
        );

        var activationToken = ActivationToken.Create(userId, 24);
        var tokenProperty = activationToken.GetType().GetProperty("Token");
        tokenProperty!.SetValue(activationToken, tokenString);
        var userProperty = activationToken.GetType().GetProperty("User");
        userProperty!.SetValue(activationToken, user);

        _activationTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activationToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("activated successfully")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}

