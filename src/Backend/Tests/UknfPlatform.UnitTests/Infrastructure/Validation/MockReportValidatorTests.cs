using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UknfPlatform.Infrastructure.Validation.Services;
using Xunit;

namespace UknfPlatform.UnitTests.Infrastructure.Validation;

public class MockReportValidatorTests
{
    private readonly Mock<ILogger<MockReportValidator>> _logger = new();

    [Fact]
    public async Task ValidateReport_Q1_ShouldPassWithWarnings()
    {
        var sut = new MockReportValidator(_logger.Object);
        using var stream = CreateSimpleWorkbook();

        var result = await sut.ValidateReportAsync(stream, "Quarterly", "Q1_2025");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateReport_Q2_ShouldFailWithErrors()
    {
        var sut = new MockReportValidator(_logger.Object);
        using var stream = CreateSimpleWorkbook();

        var result = await sut.ValidateReportAsync(stream, "Quarterly", "Q2_2025");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateReport_Generic_ShouldPassBasicValidation()
    {
        var sut = new MockReportValidator(_logger.Object);
        using var stream = CreateSimpleWorkbook();

        var result = await sut.ValidateReportAsync(stream, "Quarterly", "Annual_2025");

        result.IsValid.Should().BeTrue();
    }

    private MemoryStream CreateSimpleWorkbook()
    {
        // Minimal ZIP header to satisfy ClosedXML open (we don't parse actual content here)
        // For real tests, we would generate a valid XLSX with ClosedXML.
        // Here we rely on ClosedXML's ability to open a basic stream; if it fails, tests can be adjusted.
        var ms = new MemoryStream();
        // A valid XLSX requires a proper ZIP structure; to avoid complexity, we generate a simple XLSX via ClosedXML once.
        using (var wb = new ClosedXML.Excel.XLWorkbook())
        {
            var ws = wb.Worksheets.Add("Sheet1");
            ws.Cell(1, 1).Value = "Header";
            wb.SaveAs(ms);
        }
        ms.Position = 0;
        return ms;
    }
}

