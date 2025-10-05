using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Application.Auth.Authentication.Queries;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.UnitTests.Application.Auth;

public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<GetUserProfileQueryHandler>> _loggerMock;
    private readonly GetUserProfileQueryHandler _handler;

    public GetUserProfileQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<GetUserProfileQueryHandler>>();
        
        _handler = new GetUserProfileQueryHandler(
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsProfile()
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
        
        // Use reflection to set the Id and other properties
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        var lastLoginDate = DateTime.UtcNow.AddDays(-1);
        typeof(User).GetProperty("LastLoginDate")!.SetValue(user, lastLoginDate);
        user.Activate();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Jan");
        result.LastName.Should().Be("Kowalski");
        result.Email.Should().Be("jan.kowalski@example.com");
        result.PhoneNumber.Should().Be("+48123456789");
        result.PeselLast4.Should().Be("8901");
        result.UserType.Should().Be(UserType.External.ToString());
        result.PendingEmail.Should().BeNull();
        result.LastLoginDate.Should().Be(lastLoginDate);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        var query = new GetUserProfileQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetUserProfileQuery();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserWithPendingEmail_ReturnsPendingEmail()
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
        user.RequestEmailChange("jan.new@example.com");

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetUserProfileQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("jan.kowalski@example.com");
        result.PendingEmail.Should().Be("jan.new@example.com");
    }
}

