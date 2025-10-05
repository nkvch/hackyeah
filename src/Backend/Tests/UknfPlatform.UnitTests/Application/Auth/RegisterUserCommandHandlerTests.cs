using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;

namespace UknfPlatform.UnitTests.Application.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IActivationTokenRepository> _activationTokenRepositoryMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IOptions<ApplicationSettings>> _applicationSettingsMock;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _activationTokenRepositoryMock = new Mock<IActivationTokenRepository>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();
        
        var applicationSettings = new ApplicationSettings
        {
            FrontendUrl = "http://localhost:4200"
        };
        _applicationSettingsMock = new Mock<IOptions<ApplicationSettings>>();
        _applicationSettingsMock.Setup(x => x.Value).Returns(applicationSettings);
        
        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _activationTokenRepositoryMock.Object,
            _encryptionServiceMock.Object,
            _emailServiceMock.Object,
            _applicationSettingsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesInactiveUser()
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

        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _encryptionServiceMock
            .Setup(e => e.Encrypt(command.Pesel))
            .Returns("encrypted_pesel");

        _userRepositoryMock
            .Setup(r => r.ExistsByPeselAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user)
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _activationTokenRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock
            .Setup(e => e.SendAccountActivationEmailAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeEmpty();
        result.Message.Should().Contain("Registration successful");

        capturedUser.Should().NotBeNull();
        capturedUser!.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.Email.Should().Be(command.Email.ToLowerInvariant());
        capturedUser.Phone.Should().Be(command.Phone);
        capturedUser.PeselLast4.Should().Be("8901");
        capturedUser.IsActive.Should().BeFalse();

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _activationTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendAccountActivationEmailAsync(
            command.Email, 
            command.FirstName, 
            It.Is<string>(url => url.Contains("activate?token=")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "existing@example.com",
            Phone = "+48123456789",
            Pesel = "12345678901"
        };

        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email already exists*");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingPesel_ThrowsInvalidOperationException()
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

        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _encryptionServiceMock
            .Setup(e => e.Encrypt(command.Pesel))
            .Returns("encrypted_pesel");

        _userRepositoryMock
            .Setup(r => r.ExistsByPeselAsync("encrypted_pesel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PESEL already exists*");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_EncryptsPeselAndStoresLast4Digits()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@example.com",
            Phone = "+48123456789",
            Pesel = "98765432109"
        };

        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _encryptionServiceMock
            .Setup(e => e.Encrypt(command.Pesel))
            .Returns("encrypted_98765432109");

        _userRepositoryMock
            .Setup(r => r.ExistsByPeselAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _activationTokenRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock
            .Setup(e => e.SendAccountActivationEmailAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _encryptionServiceMock.Verify(e => e.Encrypt("98765432109"), Times.Once);
        capturedUser.Should().NotBeNull();
        capturedUser!.PeselLast4.Should().Be("2109");
        capturedUser.PeselEncrypted.Should().Be("encrypted_98765432109");
    }

    [Fact]
    public async Task Handle_ValidCommand_LogsRegistrationEvent()
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

        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _encryptionServiceMock
            .Setup(e => e.Encrypt(command.Pesel))
            .Returns("encrypted_pesel");

        _userRepositoryMock
            .Setup(r => r.ExistsByPeselAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _activationTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _activationTokenRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailServiceMock
            .Setup(e => e.SendAccountActivationEmailAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("registered successfully")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}

