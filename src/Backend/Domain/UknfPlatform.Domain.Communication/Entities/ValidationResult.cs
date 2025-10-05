using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Communication.Entities;

/// <summary>
/// Stores detailed validation results for a report.
/// </summary>
public class ValidationResult : BaseEntity
{
    public Guid ReportId { get; private set; }
    public string UniqueValidationId { get; private set; } = string.Empty;
    public ValidationStatus Status { get; private set; }
    public DateTime ValidationStartedDate { get; private set; }
    public DateTime? ValidationCompletedDate { get; private set; }
    public string? ResultFileStorageKey { get; private set; }
    public bool IsValid { get; private set; }
    public string? ErrorsJson { get; private set; } // JSON array of errors
    public string? WarningsJson { get; private set; } // JSON array of warnings
    public string? ExtractedMetadataJson { get; private set; } // JSON object of extracted metadata
    public string? TechnicalErrorMessage { get; private set; }

    // EF Core navigation property
    public Report? Report { get; private set; }

    private ValidationResult() { }

    public static ValidationResult Create(Guid reportId, string uniqueValidationId)
    {
        if (reportId == Guid.Empty)
            throw new ArgumentException("ReportId cannot be empty", nameof(reportId));
        if (string.IsNullOrWhiteSpace(uniqueValidationId))
            throw new ArgumentException("UniqueValidationId cannot be empty", nameof(uniqueValidationId));

        return new ValidationResult
        {
            ReportId = reportId,
            UniqueValidationId = uniqueValidationId,
            Status = ValidationStatus.Ongoing,
            ValidationStartedDate = DateTime.UtcNow,
            IsValid = false
        };
    }

    public void CompleteValidation(bool isValid, string? errorsJson, string? warningsJson, string? resultFileStorageKey, string? extractedMetadataJson = null)
    {
        if (Status != ValidationStatus.Ongoing)
            throw new InvalidOperationException($"Cannot complete validation. Current status is {Status}, expected Ongoing.");

        IsValid = isValid;
        ErrorsJson = errorsJson;
        WarningsJson = warningsJson;
        ResultFileStorageKey = resultFileStorageKey;
        ExtractedMetadataJson = extractedMetadataJson;
        ValidationCompletedDate = DateTime.UtcNow;
        Status = isValid ? ValidationStatus.Successful : ValidationStatus.ValidationErrors;
        UpdateTimestamp();
    }

    public void MarkAsTechnicalError(string errorMessage)
    {
        if (Status != ValidationStatus.Ongoing)
            throw new InvalidOperationException($"Cannot mark as technical error. Current status is {Status}, expected Ongoing.");

        Status = ValidationStatus.TechnicalError;
        TechnicalErrorMessage = errorMessage;
        ValidationCompletedDate = DateTime.UtcNow;
        IsValid = false;
        UpdateTimestamp();
    }

    public void MarkAsTimeout()
    {
        if (Status != ValidationStatus.Ongoing)
            throw new InvalidOperationException($"Cannot mark as timeout. Current status is {Status}, expected Ongoing.");

        Status = ValidationStatus.TimeoutError;
        TechnicalErrorMessage = "Validation exceeded 24-hour time limit";
        ValidationCompletedDate = DateTime.UtcNow;
        IsValid = false;
        UpdateTimestamp();
    }
}


