using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using UknfPlatform.Application.Communication.Reports.Commands;
using Xunit;

namespace UknfPlatform.UnitTests.Application.Communication;

public class SubmitReportCommandValidatorTests
{
    private readonly SubmitReportCommandValidator _validator;

    public SubmitReportCommandValidatorTests()
    {
        _validator = new SubmitReportCommandValidator();
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateValidXlsxFile("report.xlsx", 1024 * 1024) // 1 MB
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_InvalidEntityId_FailsValidation(long invalidEntityId)
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = invalidEntityId,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateValidXlsxFile("report.xlsx", 1024)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EntityId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyOrNullReportType_FailsValidation(string? reportType)
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = reportType!,
            ReportingPeriod = "Q1_2025",
            File = CreateValidXlsxFile("report.xlsx", 1024)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReportType);
    }

    [Fact]
    public async Task Validate_ReportTypeTooLong_FailsValidation()
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = new string('A', 251), // 251 characters (max is 250)
            ReportingPeriod = "Q1_2025",
            File = CreateValidXlsxFile("report.xlsx", 1024)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReportType);
    }

    [Theory]
    [InlineData("Q1_2025")]
    [InlineData("Q2_2024")]
    [InlineData("Q3_2030")]
    [InlineData("Q4_2023")]
    [InlineData("Annual_2025")]
    [InlineData("Annual_2020")]
    [InlineData("Monthly_2025")]
    [InlineData("Monthly_2024")]
    public async Task Validate_ValidReportingPeriod_PassesValidation(string reportingPeriod)
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = reportingPeriod,
            File = CreateValidXlsxFile("report.xlsx", 1024)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ReportingPeriod);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("Q5_2025")] // Invalid quarter
    [InlineData("Q0_2025")] // Invalid quarter
    [InlineData("Q1_25")] // Invalid year format
    [InlineData("2025_Q1")] // Wrong order
    [InlineData("Q1-2025")] // Wrong separator
    [InlineData("January_2025")] // Invalid format
    [InlineData("Q12025")] // Missing separator
    public async Task Validate_InvalidReportingPeriod_FailsValidation(string? reportingPeriod)
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = reportingPeriod!,
            File = CreateValidXlsxFile("report.xlsx", 1024)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReportingPeriod);
    }

    [Fact]
    public async Task Validate_NullFile_FailsValidation()
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = null!
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public async Task Validate_FileTooLarge_FailsValidation()
    {
        // Arrange
        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = CreateValidXlsxFile("huge_report.xlsx", 101 * 1024 * 1024) // 101 MB (exceeds 100 MB limit)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File)
            .WithErrorMessage("*100 MB*");
    }

    [Fact]
    public async Task Validate_InvalidFileType_FailsValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = mockFile.Object
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File)
            .WithErrorMessage("*XLSX*");
    }

    [Fact]
    public async Task Validate_FileWithWrongMagicBytes_FailsValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.xlsx");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Invalid magic bytes
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = mockFile.Object
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public async Task Validate_EmptyFile_FailsValidation()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("empty.xlsx");
        mockFile.Setup(f => f.Length).Returns(0);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var command = new SubmitReportCommand
        {
            EntityId = 1001,
            ReportType = "Quarterly",
            ReportingPeriod = "Q1_2025",
            File = mockFile.Object
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    // Helper methods
    private IFormFile CreateValidXlsxFile(string fileName, long fileSize)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileSize);
        mockFile.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        
        // Create a stream with valid XLSX magic bytes (ZIP signature: PK\x03\x04)
        var data = new byte[fileSize];
        data[0] = 0x50; // 'P'
        data[1] = 0x4B; // 'K'
        data[2] = 0x03;
        data[3] = 0x04;
        var stream = new MemoryStream(data);
        
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        return mockFile.Object;
    }
}

