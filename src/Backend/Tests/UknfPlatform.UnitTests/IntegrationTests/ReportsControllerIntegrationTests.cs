using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UknfPlatform.Application.Communication.Reports.Commands;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Shared.Entities;
using UknfPlatform.Domain.Shared.Interfaces;
using Xunit;

namespace UknfPlatform.UnitTests.IntegrationTests;

public class ReportsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReportsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace services with mocks/stubs as needed for isolated testing
                var entityRepositoryDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEntityRepository));
                if (entityRepositoryDescriptor != null)
                {
                    services.Remove(entityRepositoryDescriptor);
                }

                services.AddScoped<IEntityRepository>(sp =>
                {
                    var mock = new Mock<IEntityRepository>();
                    var testEntity = Entity.Create(1001L, "Test Bank S.A.", "Bank");
                    mock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(testEntity);
                    return mock.Object;
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SubmitReport_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Quarterly", "Q1_2025", "report.xlsx", 1024 * 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("reportId");
        responseBody.Should().Contain("Working");
    }

    [Fact]
    public async Task SubmitReport_MissingEntityId_ReturnsBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Quarterly"), "reportType");
        content.Add(new StringContent("Q1_2025"), "reportingPeriod");
        content.Add(CreateValidXlsxFileContent("report.xlsx", 1024), "file", "report.xlsx");

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitReport_InvalidReportingPeriod_ReturnsBadRequest()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Quarterly", "InvalidPeriod", "report.xlsx", 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Reporting period");
    }

    [Fact]
    public async Task SubmitReport_MissingFile_ReturnsBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("1001"), "entityId");
        content.Add(new StringContent("Quarterly"), "reportType");
        content.Add(new StringContent("Q1_2025"), "reportingPeriod");
        // No file attached

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitReport_FileTooLarge_ReturnsBadRequestOrPayloadTooLarge()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Quarterly", "Q1_2025", "huge.xlsx", 101 * 1024 * 1024); // 101 MB

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        // Could be 400 (from validator) or 413 (from Kestrel)
        (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            .Should().BeTrue();
    }

    [Fact]
    public async Task SubmitReport_InvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("1001"), "entityId");
        content.Add(new StringContent("Quarterly"), "reportType");
        content.Add(new StringContent("Q1_2025"), "reportingPeriod");
        
        // Add a PDF file instead of XLSX
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var pdfContent = new ByteArrayContent(pdfBytes);
        pdfContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(pdfContent, "file", "report.pdf");

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("XLSX");
    }

    [Fact]
    public async Task SubmitReport_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        // Remove any auth headers
        unauthenticatedClient.DefaultRequestHeaders.Authorization = null;
        
        var content = CreateMultipartFormContent(1001, "Quarterly", "Q1_2025", "report.xlsx", 1024);

        // Act
        var response = await unauthenticatedClient.PostAsync("/api/reports/upload", content);

        // Assert
        // If [Authorize] attribute is working
        (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            .Should().BeTrue();
    }

    [Fact]
    public async Task SubmitReport_ValidAnnualReport_ReturnsCreated()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Annual", "Annual_2025", "annual_report.xlsx", 5 * 1024 * 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Annual");
        responseBody.Should().Contain("2025");
    }

    [Fact]
    public async Task SubmitReport_ValidMonthlyReport_ReturnsCreated()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Monthly", "Monthly_2025", "monthly_report.xlsx", 2 * 1024 * 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Monthly");
    }

    [Theory]
    [InlineData("Q2_2025")]
    [InlineData("Q3_2024")]
    [InlineData("Q4_2026")]
    [InlineData("Annual_2024")]
    [InlineData("Monthly_2023")]
    public async Task SubmitReport_VariousValidReportingPeriods_ReturnsCreated(string reportingPeriod)
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Quarterly", reportingPeriod, "report.xlsx", 1024 * 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SubmitReport_ReturnsExpectedResponseStructure()
    {
        // Arrange
        var content = CreateMultipartFormContent(1001, "Quarterly", "Q1_2025", "report.xlsx", 1024 * 1024);

        // Act
        var response = await _client.PostAsync("/api/reports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadAsStringAsync();
        
        // Verify response structure contains expected fields
        responseBody.Should().Contain("reportId");
        responseBody.Should().Contain("status");
        responseBody.Should().Contain("message");
        responseBody.Should().Contain("submittedDate");
        responseBody.Should().Contain("submitterName");
        responseBody.Should().Contain("entityName");
    }

    // Helper methods
    private MultipartFormDataContent CreateMultipartFormContent(
        long entityId,
        string reportType,
        string reportingPeriod,
        string fileName,
        long fileSize)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(entityId.ToString()), "entityId");
        content.Add(new StringContent(reportType), "reportType");
        content.Add(new StringContent(reportingPeriod), "reportingPeriod");
        content.Add(CreateValidXlsxFileContent(fileName, fileSize), "file", fileName);
        return content;
    }

    private ByteArrayContent CreateValidXlsxFileContent(string fileName, long fileSize)
    {
        var data = new byte[fileSize];
        // Add valid XLSX magic bytes (ZIP signature)
        data[0] = 0x50; // 'P'
        data[1] = 0x4B; // 'K'
        data[2] = 0x03;
        data[3] = 0x04;

        var fileContent = new ByteArrayContent(data);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        return fileContent;
    }
}

