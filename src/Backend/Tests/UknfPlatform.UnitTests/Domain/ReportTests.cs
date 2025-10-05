using FluentAssertions;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using Xunit;

namespace UknfPlatform.UnitTests.Domain;

public class ReportTests
{
    [Fact]
    public void Create_WithValidData_CreatesReportWithWorkingStatus()
    {
        // Arrange
        var entityId = 1001L;
        var userId = Guid.NewGuid();
        var fileName = "report_Q1_2025.xlsx";
        var fileStorageKey = "reports/2025/report123.xlsx";
        var fileSize = 1024000L;
        var reportType = "Quarterly";
        var reportingPeriod = "Q1_2025";

        // Act
        var report = Report.Create(
            entityId,
            userId,
            fileName,
            fileStorageKey,
            fileSize,
            reportType,
            reportingPeriod);

        // Assert
        report.Should().NotBeNull();
        report.Id.Should().NotBeEmpty();
        report.EntityId.Should().Be(entityId);
        report.UserId.Should().Be(userId);
        report.FileName.Should().Be(fileName);
        report.FileStorageKey.Should().Be(fileStorageKey);
        report.FileSize.Should().Be(fileSize);
        report.ReportType.Should().Be(reportType);
        report.ReportingPeriod.Should().Be(reportingPeriod);
        report.ValidationStatus.Should().Be(ValidationStatus.Working);
        report.IsArchived.Should().BeFalse();
        report.SubmittedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0L, "Entity ID must be positive")]
    [InlineData(-1L, "Entity ID must be positive")]
    public void Create_WithInvalidEntityId_ThrowsArgumentException(long entityId, string expectedMessage)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Report.Create(
            entityId,
            userId,
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*{expectedMessage}*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act
        var act = () => Report.Create(
            1001L,
            Guid.Empty,
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User ID must be valid*");
    }

    [Theory]
    [InlineData("", "File name is required")]
    [InlineData(" ", "File name is required")]
    [InlineData(null, "File name is required")]
    public void Create_WithInvalidFileName_ThrowsArgumentException(string? fileName, string expectedMessage)
    {
        // Act
        var act = () => Report.Create(
            1001L,
            Guid.NewGuid(),
            fileName!,
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*{expectedMessage}*");
    }

    [Fact]
    public void CreateCorrection_WithValidData_CreatesReportWithCorrectionLink()
    {
        // Arrange
        var originalReportId = Guid.NewGuid();

        // Act
        var report = Report.CreateCorrection(
            1001L,
            Guid.NewGuid(),
            "correction.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025",
            originalReportId);

        // Assert
        report.Should().NotBeNull();
        report.IsCorrectionOfReportId.Should().Be(originalReportId);
        report.ValidationStatus.Should().Be(ValidationStatus.Working);
    }

    [Fact]
    public void StartValidation_WhenStatusIsWorking_UpdatesToTransmitted()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        var validationId = "VAL-12345";

        // Act
        report.StartValidation(validationId);

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.Transmitted);
        report.UniqueValidationId.Should().Be(validationId);
        report.ValidationStartedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartValidation_WhenStatusIsNotWorking_ThrowsInvalidOperationException()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        report.StartValidation("VAL-123");

        // Act
        var act = () => report.StartValidation("VAL-456");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start validation*");
    }

    [Fact]
    public void UpdateToOngoing_WhenStatusIsTransmitted_UpdatesToOngoing()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        report.StartValidation("VAL-123");

        // Act
        report.UpdateToOngoing();

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.Ongoing);
    }

    [Fact]
    public void CompleteValidationSuccessfully_WhenStatusIsOngoing_UpdatesToSuccessful()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        report.StartValidation("VAL-123");
        report.UpdateToOngoing();
        var resultFileKey = "results/result123.pdf";

        // Act
        report.CompleteValidationSuccessfully(resultFileKey);

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.Successful);
        report.ValidationResultFileKey.Should().Be(resultFileKey);
        report.ValidationCompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompleteValidationWithErrors_WhenStatusIsOngoing_UpdatesToValidationErrors()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        report.StartValidation("VAL-123");
        report.UpdateToOngoing();
        var errorDescription = "Invalid data in row 5";
        var resultFileKey = "results/errors123.pdf";

        // Act
        report.CompleteValidationWithErrors(errorDescription, resultFileKey);

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.ValidationErrors);
        report.ErrorDescription.Should().Be(errorDescription);
        report.ValidationResultFileKey.Should().Be(resultFileKey);
        report.ValidationCompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordTechnicalError_UpdatesStatusToTechnicalError()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        var errorDescription = "Service unavailable";

        // Act
        report.RecordTechnicalError(errorDescription);

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.TechnicalError);
        report.ErrorDescription.Should().Be(errorDescription);
        report.ValidationCompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordTimeoutError_UpdatesStatusToTimeoutError()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");

        // Act
        report.RecordTimeoutError();

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.TimeoutError);
        report.ErrorDescription.Should().Contain("24-hour timeout");
        report.ValidationCompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ContestByUKNF_WithValidData_UpdatesStatusToContested()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        var uknfUserId = Guid.NewGuid();
        var description = "Data inconsistency detected";

        // Act
        report.ContestByUKNF(uknfUserId, description);

        // Assert
        report.ValidationStatus.Should().Be(ValidationStatus.ContestedByUKNF);
        report.ContestedByUserId.Should().Be(uknfUserId);
        report.ContestedDescription.Should().Be(description);
        report.ContestedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Archive_SetsIsArchivedToTrue()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");

        // Act
        report.Archive();

        // Assert
        report.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Unarchive_SetsIsArchivedToFalse()
    {
        // Arrange
        var report = Report.Create(
            1001L,
            Guid.NewGuid(),
            "file.xlsx",
            "key",
            1000L,
            "Quarterly",
            "Q1_2025");
        report.Archive();

        // Act
        report.Unarchive();

        // Assert
        report.IsArchived.Should().BeFalse();
    }
}

