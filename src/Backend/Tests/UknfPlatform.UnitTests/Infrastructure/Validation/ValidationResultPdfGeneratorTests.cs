using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Infrastructure.Validation.Services;
using Xunit;

namespace UknfPlatform.UnitTests.Infrastructure.Validation;

public class ValidationResultPdfGeneratorTests
{
    private readonly Mock<ILogger<ValidationResultPdfGenerator>> _logger = new();

    [Fact]
    public async Task GeneratePdfAsync_ShouldProduceNonEmptyStream()
    {
        var generator = new ValidationResultPdfGenerator(_logger.Object);

        var validationResult = ValidationResult.CreateValid(
            warnings: new List<ValidationWarning> { new("WARN001", "Test warning", null) },
            metadata: new Dictionary<string, string> { ["Key"] = "Value" });

        var metadata = new ReportMetadata(
            ReportId: Guid.NewGuid(),
            FileName: "report.xlsx",
            ReportType: "Quarterly",
            ReportingPeriod: "Q1_2025",
            EntityName: "Test Bank S.A.",
            EntityCode: "UKNF000001",
            SubmittedDate: DateTime.UtcNow.AddMinutes(-5),
            ValidationCompletedDate: DateTime.UtcNow
        );

        using var stream = await generator.GeneratePdfAsync(validationResult, metadata, "VAL-123");

        stream.Should().NotBeNull();
        stream.Length.Should().BeGreaterThan(0);
    }
}

