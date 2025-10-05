namespace UknfPlatform.Application.Communication.Reports.Commands;

/// <summary>
/// Response after successful report submission
/// </summary>
public record SubmitReportResponse
{
    /// <summary>
    /// Unique report identifier
    /// </summary>
    public Guid ReportId { get; init; }
    
    /// <summary>
    /// Validation service unique ID (set after transmission)
    /// </summary>
    public string? UniqueValidationId { get; init; }
    
    /// <summary>
    /// Current validation status
    /// </summary>
    public string Status { get; init; } = string.Empty;
    
    /// <summary>
    /// Confirmation message
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Report submission timestamp
    /// </summary>
    public DateTime SubmittedDate { get; init; }
    
    /// <summary>
    /// Name of the user who submitted the report
    /// </summary>
    public string SubmitterName { get; init; } = string.Empty;
    
    /// <summary>
    /// Name of the entity for which the report was submitted
    /// </summary>
    public string EntityName { get; init; } = string.Empty;
}

