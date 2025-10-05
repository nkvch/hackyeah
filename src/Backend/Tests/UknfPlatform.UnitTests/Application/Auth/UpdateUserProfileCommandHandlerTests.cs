using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Application.Auth.Authentication.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.UnitTests.Application.Auth;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailChangeTokenRepository> _emailChangeTokenRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<UpdateUserProfileCommandHandler>> _loggerMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailChangeTokenRepositoryMock = new Mock<IEmailChangeTokenRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<UpdateUserProfileCommandHandler>>();

        _configurationMock.Setup(x => x["Frontend:Url"]).Returns("http://localhost:4200");

        _handler = new UpdateUserProfileCommandHandler(
            _userRepositoryMock.Object,
            _emailChangeTokenRepositoryMock.Object,
            _emailServiceMock.Object,
            _currentUserServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UpdateBasicFields_UpdatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan.kowalski@example.com",
            "+48123456789",
            "encrypted-pesel",
            "8901");

        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        user.Activate();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateUserProfileCommand
        {
            FirstName = "Janusz",
            LastName = "Nowak",
            PhoneNumber = "+48987654321",
            Email = "jan.kowalski@example.com" // Same email
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.EmailChangeRequiresConfirmation.Should().BeFalse();
        result.UpdatedProfile.FirstName.Should().Be("Janusz");
        result.UpdatedProfile.LastName.Should().Be("Nowak");
        result.UpdatedProfile.PhoneNumber.Should().Be("+48987654321");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailChangeConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailChange_GeneratesTokenAndSendsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan.kowalski@example.com",
            "+48123456789",
            "encrypted-pesel",
            "8901");

        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        user.Activate();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("jan.new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateUserProfileCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            PhoneNumber = "+48123456789",
            Email = "jan.new@example.com" // New email
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.EmailChangeRequiresConfirmation.Should().BeTrue();
        result.Message.Should().Contain("check your new email");
        result.UpdatedProfile.Email.Should().Be("jan.kowalski@example.com"); // Not changed yet
        result.UpdatedProfile.PendingEmail.Should().Be("jan.new@example.com");

        _emailChangeTokenRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<EmailChangeToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailChangeConfirmationAsync(
            "jan.new@example.com",
            "Jan",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.CreateExternal(
            "Jan",
            "Kowalski",
            "jan.kowalski@example.com",
            "+48123456789",
            "encrypted-pesel",
            "8901");

        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        user.Activate();

        var otherUser = User.CreateExternal(
            "Anna",
            "Nowak",
            "anna.nowak@example.com",
            "+48111222333",
            "other-encrypted-pesel",
            "7777");
        typeof(User).GetProperty("Id")!.SetValue(otherUser, Guid.NewGuid());

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("anna.nowak@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser); // Email already exists

        var command = new UpdateUserProfileCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            PhoneNumber = "+48123456789",
            Email = "anna.nowak@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new UpdateUserProfileCommand
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            PhoneNumber = "+48123456789",
            Email = "jan.kowalski@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

