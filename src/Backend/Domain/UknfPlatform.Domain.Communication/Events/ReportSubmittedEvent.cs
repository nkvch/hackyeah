using MediatR;

namespace UknfPlatform.Domain.Communication.Events;

/// <summary>
/// Domain event raised when a report is submitted
/// This event triggers the validation process (handled in Story 4.2)
/// </summary>
public record ReportSubmittedEvent : INotification
{
    /// <summary>
    /// Unique report identifier
    /// </summary>
    public Guid ReportId { get; init; }
    
    /// <summary>
    /// Entity that submitted the report
    /// </summary>
    public long EntityId { get; init; }
    
    /// <summary>
    /// User who submitted the report
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; init; } = string.Empty;
    
    /// <summary>
    /// Storage key for retrieving the file
    /// </summary>
    public string FileStorageKey { get; init; } = string.Empty;
    
    /// <summary>
    /// Report type (e.g., "Quarterly", "Annual")
    /// </summary>
    public string ReportType { get; init; } = string.Empty;
    
    /// <summary>
    /// Reporting period (e.g., "Q1_2025")
    /// </summary>
    public string ReportingPeriod { get; init; } = string.Empty;
    
    /// <summary>
    /// When the report was submitted
    /// </summary>
    public DateTime SubmittedDate { get; init; }
}

