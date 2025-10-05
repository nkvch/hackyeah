namespace UknfPlatform.Application.Shared.Models.Validation;

/// <summary>
/// Metadata for report validation submission.
/// </summary>
public record ValidationMetadata(
    string FileName,
    string ReportType,
    string ReportingPeriod,
    long EntityId,
    string EntityName,
    Guid UserId,
    string UserName,
    string UserEmail
);


