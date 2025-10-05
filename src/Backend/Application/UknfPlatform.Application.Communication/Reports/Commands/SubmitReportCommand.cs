using MediatR;
using Microsoft.AspNetCore.Http;

namespace UknfPlatform.Application.Communication.Reports.Commands;

/// <summary>
/// Command to submit a regulatory report with file upload
/// </summary>
public record SubmitReportCommand : IRequest<SubmitReportResponse>
{
    /// <summary>
    /// Selected entity context - which entity is submitting the report
    /// </summary>
    public long EntityId { get; init; }
    
    /// <summary>
    /// Report category (e.g., "Quarterly", "Annual")
    /// </summary>
    public string ReportType { get; init; } = string.Empty;
    
    /// <summary>
    /// Reporting period identifier (e.g., "Q1_2025", "Annual_2025")
    /// </summary>
    public string ReportingPeriod { get; init; } = string.Empty;
    
    /// <summary>
    /// Uploaded XLSX file (multipart/form-data)
    /// </summary>
    public IFormFile? File { get; init; }
}

