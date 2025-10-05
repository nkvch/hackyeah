namespace UknfPlatform.Application.Shared.Models.Validation;

/// <summary>
/// Response returned when a report is submitted for validation.
/// </summary>
public record ValidationSubmissionResponse(
    string UniqueValidationId,
    string Status,
    DateTime SubmittedAt
);


