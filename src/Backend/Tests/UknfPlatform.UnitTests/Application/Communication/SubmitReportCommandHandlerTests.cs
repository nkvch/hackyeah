using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Application.Communication.Reports.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Events;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Domain.Shared.Entities;
using UknfPlatform.Domain.Shared.Interfaces;
using Xunit;

namespace UknfPlatform.UnitTests.Application.Communication;

public class SubmitReportCommandHandlerTests
{
    private readonly Mock<IReportRepository> _reportRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEntityRepository> _entityRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<SubmitReportCommandHandler>> _loggerMock;
    private readonly SubmitReportCommandHandler _handler;

    public SubmitReportCommandHandlerTests()
    {
        _reportRepositoryMock = new Mock<IReportRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _entityRepositoryMock = new Mock<IEntityRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<SubmitReportCommandHandler>>();

        _handler = new SubmitReportCommandHandler(
            _reportRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _currentUserServiceMock.Object,
            _entityRepositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_SubmitsReportSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = 1001L;
        var fileName = "report_Q1_2025.xlsx";
        var storageKey = "reports/2025/01/guid_report.xlsx";

        var mockFile = CreateMockFile(fileName, 1024);
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = mockFile
        };

        var testEntity = Entity.Create(entityId, "Test Bank S.A.", "Bank");

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _currentUserServiceMock.Setup(s => s.FullName).Returns("Test User");
        _currentUserServiceMock.Setup(s => s.HasPermissionAsync("communication.reports.submit", entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _entityRepositoryMock.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        _reportRepositoryMock.Setup(r => r.ExistsAsync(entityId, "Quarterly", "Q1_2025", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _fileStorageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().NotBeEmpty();
        result.Status.Should().Be("Working");
        result.Message.Should().Contain("submitted successfully");
        result.SubmitterName.Should().Be("Test User");
        result.EntityName.Should().Be("Test Bank S.A.");

        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<ReportSubmittedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserLacksPermission_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var entityId = 1001L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(s => s.HasPermissionAsync("communication.reports.submit", entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var entityId = 9999L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        _currentUserServiceMock.Setup(s => s.HasPermissionAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _entityRepositoryMock.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entity?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_DuplicateReport_ThrowsInvalidOperationException()
    {
        // Arrange
        var entityId = 1001L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        var testEntity = Entity.Create(entityId, "Test Bank", "Bank");

        _currentUserServiceMock.Setup(s => s.HasPermissionAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _entityRepositoryMock.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        _reportRepositoryMock.Setup(r => r.ExistsAsync(entityId, "Quarterly", "Q1_2025", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Duplicate exists

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_FileUploadFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var entityId = 1001L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        var testEntity = Entity.Create(entityId, "Test Bank", "Bank");

        _currentUserServiceMock.Setup(s => s.HasPermissionAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _entityRepositoryMock.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        _reportRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _fileStorageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Storage unavailable"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*storage*");
    }

    [Fact]
    public async Task Handle_PublishesReportSubmittedEvent()
    {
        // Arrange
        var entityId = 1001L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        var testEntity = Entity.Create(entityId, "Test Bank", "Bank");

        SetupSuccessfulSubmission(entityId, testEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            p => p.PublishAsync(
                It.Is<ReportSubmittedEvent>(e =>
                    e.EntityId == entityId &&
                    e.ReportType == "Quarterly" &&
                    e.ReportingPeriod == "Q1_2025"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesReportWithCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = 1001L;
        var fileName = "test_report.xlsx";
        var fileSize = 2048L;

        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Annual",
            ReportingPeriod = "Annual_2025",
            File = CreateMockFile(fileName, fileSize)
        };

        var testEntity = Entity.Create(entityId, "Test Bank", "Bank");

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        SetupSuccessfulSubmission(entityId, testEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _reportRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Report>(report =>
                    report.EntityId == entityId &&
                    report.UserId == userId &&
                    report.FileName == fileName &&
                    report.FileSize == fileSize &&
                    report.ReportType == "Annual" &&
                    report.ReportingPeriod == "Annual_2025"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsInitialStatusToWorking()
    {
        // Arrange
        var entityId = 1001L;
        var command = new SubmitReportCommand
        {
            EntityId = entityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateMockFile("report.xlsx", 1024)
        };

        var testEntity = Entity.Create(entityId, "Test Bank", "Bank");
        SetupSuccessfulSubmission(entityId, testEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Working");
        result.UniqueValidationId.Should().BeNull(); // Not set until transmitted
    }

    // Helper methods
    private IFormFile CreateMockFile(string fileName, long fileSize)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileSize);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[fileSize]));
        return mockFile.Object;
    }

    private void SetupSuccessfulSubmission(long entityId, Entity entity)
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(s => s.FullName).Returns("Test User");
        _currentUserServiceMock.Setup(s => s.HasPermissionAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _entityRepositoryMock.Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _reportRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _fileStorageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("storage/key");
    }
}

