namespace UknfPlatform.Domain.Communication.Messages;

/// <summary>
/// Message published to RabbitMQ queue to trigger async report validation.
/// </summary>
public record ReportValidationJob
{
    public Guid ReportId { get; init; }
    public long EntityId { get; init; }
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileStorageKey { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public string ReportingPeriod { get; init; } = string.Empty;
    public DateTime SubmittedDate { get; init; }
}


